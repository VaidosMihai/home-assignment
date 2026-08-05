using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using HomeLibrary.Api;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<LibraryContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// 1. Endpoint POST /api/imports (Upload CSV)
app.MapPost("/api/imports", async (IFormFile file, LibraryContext db) =>
{
    if (file == null || file.Length == 0)
        return Results.BadRequest(new { error = "No file uploaded or file is empty." });

    if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        return Results.BadRequest(new { error = "Invalid file type. Please upload a CSV file." });

    var booksToInsert = new List<Book>();

    using (var stream = file.OpenReadStream())
    using (var reader = new StreamReader(stream))
    using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true }))
    {
        try
        {
            // CSV line read (name, author, genre)
            while (await csv.ReadAsync())
            {
                var name = csv.GetField(0);
                var author = csv.GetField(1);
                var genre = csv.GetField(2);

                // Skip not okey values
                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(author) || string.IsNullOrWhiteSpace(genre))
                    continue;

                booksToInsert.Add(new Book
                {
                    Name = name.Trim(),
                    Author = author.Trim(),
                    Genre = genre.Trim(),
                    ImportDate = DateTime.UtcNow
                });
            }
        }
        catch
        {
            return Results.BadRequest(new { error = "Failed to parse CSV file." });
        }
    }

    if (booksToInsert.Count > 0)
    {
        db.Library.AddRange(booksToInsert);
        await db.SaveChangesAsync();
    }

    return Results.Ok(new { imported = booksToInsert.Count });
});

// 2. Endpoint GET /api/books
app.MapGet("/api/books", async (LibraryContext db) =>
{
    var books = await db.Library
        .OrderByDescending(b => b.ImportDate)
        .ToListAsync();

    return Results.Ok(books);
});

app.Run();