using Microsoft.EntityFrameworkCore;

namespace HomeLibrary.Api;

public class LibraryContext : DbContext
{
    public LibraryContext(DbContextOptions<LibraryContext> options) : base(options) { }

    public DbSet<Book> Library { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>(entity =>
        {
            entity.ToTable("library");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Author).HasColumnName("author");
            entity.Property(e => e.Genre).HasColumnName("genre");
            entity.Property(e => e.ImportDate).HasColumnName("import_date");
        });
    }
}