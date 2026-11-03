using StockSync.Infrastructure.Persistence;

namespace StockSync.Api.Configuration;

internal static class DatabaseStartup
{
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<DatabaseInitializer>().InitializeAsync(app.Lifetime.ApplicationStopping);
    }
}
