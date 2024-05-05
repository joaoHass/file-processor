using ImageProcessor.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ImageProcessor.Tests;

public class DbContextFactory : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ApplicationDbContext _context;

    public DbContextFactory()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.OpenAsync();
        
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;
        
        _context = new ApplicationDbContext(options);
        _context.Database.Migrate();
        _context.SaveChanges();
    }
    
    public ApplicationDbContext Create()
    {
        return _context;
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _connection.Close();
    }
}