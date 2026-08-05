using HomeLibrary.Api;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<LibraryContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Library")));

builder.Services.AddSingleton<IConnection>(_ =>
{
    var factory = new ConnectionFactory
    {
        HostName = builder.Configuration["RabbitMq__HostName"] ?? "rabbitmq",
        UserName = builder.Configuration["RabbitMq__UserName"] ?? "guest",
        Password = builder.Configuration["RabbitMq__Password"] ?? "guest",
        Port = AmqpTcpEndpoint.UseDefaultPort,
    };

    return factory.CreateConnection();
});

builder.Services.AddHostedService<LibraryBookConsumer>();

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<LibraryContext>();
    dbContext.Database.EnsureCreated();
}

await host.RunAsync();

public sealed class LibraryBookConsumer : BackgroundService
{
    private const string QueueName = "library-books";
    private readonly IConnection _connection;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<LibraryBookConsumer> _logger;

    public LibraryBookConsumer(IConnection connection, IServiceScopeFactory scopeFactory, ILogger<LibraryBookConsumer> logger)
    {
        _connection = connection;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var channel = _connection.CreateModel();
        channel.QueueDeclare(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (_, eventArgs) =>
        {
            try
            {
                var body = eventArgs.Body.ToArray();
                var payload = Encoding.UTF8.GetString(body);
                var message = JsonSerializer.Deserialize<BookImportMessage>(payload);

                if (message is null)
                {
                    _logger.LogWarning("Received a malformed RabbitMQ payload and will not ack it.");
                    channel.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: false);
                    return;
                }

                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<LibraryContext>();
                dbContext.Library.Add(new Book
                {
                    Name = message.Name.Trim(),
                    Author = message.Author.Trim(),
                    Genre = message.Genre.Trim(),
                    ImportDate = DateTime.UtcNow,
                });

                dbContext.SaveChanges();
                channel.BasicAck(eventArgs.DeliveryTag, multiple: false);

                _logger.LogInformation(
                    "Consumed book message: {Name}, {Author}, {Genre}",
                    message.Name,
                    message.Author,
                    message.Genre);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process message from RabbitMQ queue.");
                channel.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: false);
            }
        };

        channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(250, stoppingToken);
        }
    }
}
