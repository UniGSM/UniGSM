using GsmCore.Model;
using Microsoft.EntityFrameworkCore;

namespace GsmCore;

public class GsmDbContext : DbContext
{
    public DbSet<Server> Servers { get; set; }
    public DbSet<Setting> Settings { get; set; }

    private string DbPath { get; }

    public GsmDbContext()
    {
        const Environment.SpecialFolder folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = Path.Join(path, "gsm.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={DbPath}");
    }
}