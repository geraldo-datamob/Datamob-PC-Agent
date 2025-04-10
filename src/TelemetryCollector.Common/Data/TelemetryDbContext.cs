using Microsoft.EntityFrameworkCore;
using TelemetryCollector.Common.Data.Entities;

namespace TelemetryCollector.Common.Data;

public class TelemetryDbContext : DbContext
{
    public DbSet<Credential> Credentials { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        var dbPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "telemetry.db");
        options.UseSqlite($"Data Source={dbPath}");
    }
}