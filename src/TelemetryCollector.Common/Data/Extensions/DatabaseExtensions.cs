using TelemetryCollector.Common.Data.Entities;

namespace TelemetryCollector.Common.Data.Extensions;

public static class DatabaseExtensions
{
    public static async Task SeedDatabaseAsync(this TelemetryDbContext context)
    {
        if (!context.Credentials.Any())
        {
            context.Credentials.Add(new Credential
            {
                Username = "geraldo-neto",
                Password = "n2e4t0o4GAN$"
            });

            await context.SaveChangesAsync();
        }
    }
}
