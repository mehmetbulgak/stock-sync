namespace StockSync.Infrastructure.Persistence;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    public bool MigrateOnStartup { get; set; }

    public bool SeedDemoData { get; set; }
}
