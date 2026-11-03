using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StockSync.Infrastructure.Persistence.Seeding;

namespace StockSync.Infrastructure.Persistence;

public sealed partial class DatabaseInitializer(
    AppDbContext db,
    DemoDataSeeder seeder,
    IOptions<DatabaseOptions> options,
    ILogger<DatabaseInitializer> logger)
{
    public async Task InitializeAsync(CancellationToken ct = default)
    {
        if (options.Value.MigrateOnStartup)
        {
            await db.Database.MigrateAsync(ct);
            LogMigrated(logger);
        }

        if (options.Value.SeedDemoData)
        {
            await seeder.SeedAsync(ct);
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Database migrations applied")]
    private static partial void LogMigrated(ILogger logger);
}
