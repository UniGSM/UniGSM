using System.Text.Json;
using GsmApi.Authentication;
using GsmCore.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GsmApi;

public class GsmDbContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<Server> Servers { get; set; }
    public DbSet<Setting> Settings { get; set; }
    public DbSet<BackupRepository> BackupRepositories { get; set; }
    public DbSet<CronChain> CronChains { get; set; }
    public DbSet<CronTask> CronTasks { get; set; }

    private string DbPath { get; }

    public GsmDbContext()
    {
        const Environment.SpecialFolder folder = Environment.SpecialFolder.CommonApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = Path.Join(path, "dayzgsm", "gsm.db");
        Directory.CreateDirectory(Path.Join(path, "dayzgsm"));
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={DbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<BackupRepository>()
            .Property(e => e.Data)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions)null));
    }
}