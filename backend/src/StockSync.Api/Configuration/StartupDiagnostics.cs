using Microsoft.Extensions.Options;
using StockSync.Infrastructure.Auth;
using StockSync.Infrastructure.Persistence;

namespace StockSync.Api.Configuration;

internal sealed partial class StartupDiagnostics(
    IOptions<JwtOptions> jwtOptions,
    IOptions<DatabaseOptions> databaseOptions,
    ILogger<StartupDiagnostics> logger) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (jwtOptions.Value.UsesEphemeralSigningKey)
        {
            LogEphemeralSigningKey(logger);
        }

        if (databaseOptions.Value.SeedDemoData)
        {
            LogDemoDataEnabled(logger);
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Jwt:SigningKey is not set. A random key was generated for this process; sessions end when it restarts and cannot be shared between instances.")]
    private static partial void LogEphemeralSigningKey(ILogger logger);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Database:SeedDemoData is enabled. The public demo account exists; disable this setting for real deployments.")]
    private static partial void LogDemoDataEnabled(ILogger logger);
}
