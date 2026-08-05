using System.Globalization;
using System.Text;
using System.Text.Json;
using CsvHelper;
using CsvHelper.Configuration;
using HomeLibrary.Api;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// --- GRACEFUL STARTUP / DATABASE RETRY ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var dbContext = services.GetRequiredService<LibraryContext>();

    int maxRetries = 5;
    int delaySeconds = 3;

    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            logger.LogInformation("Attempting to connect to the database (Attempt {Attempt}/{Max})...", i + 1, maxRetries);
            dbContext.Database.EnsureCreated();
            logger.LogInformation("Successfully connected to the database!");
            break;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Database connection failed. Retrying in {Delay} seconds...", delaySeconds);
            if (i == maxRetries - 1)
            {
                throw;
            }
            Thread.Sleep(TimeSpan.FromSeconds(delaySeconds));
        }
    }
}
// ------------------------------------------

app.UseSwagger();
app.UseSwaggerUI();

// 1. Endpoint POST /api/imports (Upload CSV)
app.MapPost("/api/imports", async (IFormFile file, IConnection rabbitConnection) =>
{
    if (file == null || file.Length == 0)
        return Results.BadRequest(new { error = "No file uploaded or file is empty." });

    if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        return Results.BadRequest(new { error = "Invalid file type. Please upload a CSV file." });

    var queueName = "library-books";
    var publishedCount = 0;

    using var channel = rabbitConnection.CreateModel();
    channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

    using (var stream = file.OpenReadStream())
    using (var reader = new StreamReader(stream))
    using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true }))
    {
        try
        {
            while (await csv.ReadAsync())
            {
                var name = csv.GetField(0);
                var author = csv.GetField(1);
                var genre = csv.GetField(2);

                if (string.Equals(name, "name", StringComparison.OrdinalIgnoreCase)
                    && string.Equals(author, "author", StringComparison.OrdinalIgnoreCase)
                    && string.Equals(genre, "genre", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(author) || string.IsNullOrWhiteSpace(genre))
                    continue;

                var message = new BookImportMessage(name.Trim(), author.Trim(), genre.Trim());
                var payload = JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(payload);

                channel.BasicPublish(exchange: string.Empty, routingKey: queueName, mandatory: false, basicProperties: null, body: body);
                publishedCount++;
            }
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { error = "Failed to parse CSV file.", details = ex.Message });
        }
    }

    return Results.Ok(new { imported = publishedCount });
}).DisableAntiforgery();

// 2. Endpoint GET /api/books
app.MapGet("/api/books", async (LibraryContext db) =>
{
    var books = await db.Library
        .OrderByDescending(b => b.ImportDate)
        .ToListAsync();

    return Results.Ok(books);
});

app.Run();