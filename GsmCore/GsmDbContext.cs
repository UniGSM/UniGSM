using System.Text.Json;
using GsmCore.Model;
using Microsoft.EntityFrameworkCore;

namespace GsmCore;

public class GsmDbContext : DbContext
{
    public DbSet<Server> Servers { get; set; }
    public DbSet<Setting> Settings { get; set; }
    public DbSet<BackupRepository> BackupRepositories { get; set; }

    private string DbPath { get; }

    public GsmDbContext()
    {
        const Environment.SpecialFolder folder = Environment.SpecialFolder.CommonApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = Path.Join(path, "dayzgsm", "gsm.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={DbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BackupRepository>()
            .Property(e => e.Data)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions)null));
    }
}