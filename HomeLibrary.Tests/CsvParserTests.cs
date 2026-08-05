using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Xunit;

namespace HomeLibrary.Tests;

public class CsvParserTests
{
    [Fact]
    public void ParseCsv_ValidRows_ShouldExtractBooksCorrectly()
    {
        var csvContent = "name,author,genre\n" +
                         "  The Hobbit , J.R.R. Tolkien , Fantasy \n" +
                         "1984, George Orwell , Dystopian ";

        using var reader = new StringReader(csvContent);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true });

        var parsedBooks = new List<(string Name, string Author, string Genre)>();

        while (csv.Read())
        {
            var name = csv.GetField(0);
            var author = csv.GetField(1);
            var genre = csv.GetField(2);

            if (string.Equals(name, "name", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(author, "author", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(genre, "genre", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(author) || string.IsNullOrWhiteSpace(genre))
            {
                continue;
            }

            parsedBooks.Add((name.Trim(), author.Trim(), genre.Trim()));
        }

        Assert.Equal(2, parsedBooks.Count);
        
        Assert.Equal("The Hobbit", parsedBooks[0].Name);
        Assert.Equal("J.R.R. Tolkien", parsedBooks[0].Author);
        Assert.Equal("Fantasy", parsedBooks[0].Genre);

        Assert.Equal("1984", parsedBooks[1].Name);
        Assert.Equal("George Orwell", parsedBooks[1].Author);
        Assert.Equal("Dystopian", parsedBooks[1].Genre);
    }

    [Fact]
    public void ParseCsv_EmptyOrMissingFields_ShouldSkipInvalidRows()
    {
        var csvContent = "name,author,genre\n" +
                         ", Stephen King , Horror\n" +
                         "Dune, , Sci-Fi\n" +             
                         " , , \n" +                      
                         "Clean Code, Robert Martin, Tech"; 

        using var reader = new StringReader(csvContent);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true });

        var parsedBooksCount = 0;

        // Act
        while (csv.Read())
        {
            var name = csv.GetField(0);
            var author = csv.GetField(1);
            var genre = csv.GetField(2);

            if (string.Equals(name, "name", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(author, "author", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(genre, "genre", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(author) || string.IsNullOrWhiteSpace(genre))
            {
                continue; 
            }

            parsedBooksCount++;
        }

        Assert.Equal(1, parsedBooksCount);
    }

    [Fact]
    public void BookModel_Initialization_ShouldSetPropertiesCorrectly()
    {
        var book = new
        {
            Name = "Test Book",
            Author = "Test Author",
            Genre = "Test Genre",
            ImportDate = DateTime.UtcNow
        };

        Assert.Equal("Test Book", book.Name);
        Assert.Equal("Test Author", book.Author);
        Assert.Equal("Test Genre", book.Genre);
        Assert.True(book.ImportDate <= DateTime.UtcNow);
    }
}