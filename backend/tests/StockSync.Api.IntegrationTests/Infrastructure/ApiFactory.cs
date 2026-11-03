using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Testcontainers.PostgreSql;

namespace StockSync.Api.IntegrationTests.Infrastructure;

public class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string ExternalDatabaseVariable = "STOCKSYNC_TEST_DB";
    private const string PostgresImage = "postgres:17-alpine";

    private PostgreSqlContainer? _container;
    private string? _externalDatabase;
    private string _connectionString = null!;

    public TestClock Clock { get; } = new();

    protected virtual int AuthPermitLimit => 100_000;

    protected virtual int MaxLoginFailuresPerAccount => 1000;

    public async ValueTask InitializeAsync()
    {
        var external = Environment.GetEnvironmentVariable(ExternalDatabaseVariable);
        if (!string.IsNullOrWhiteSpace(external))
        {
            var builder = new NpgsqlConnectionStringBuilder(external) { Database = $"stocksync_test_{Guid.NewGuid():N}" };
            _externalDatabase = builder.Database;
            _connectionString = builder.ConnectionString;
        }
        else
        {
            _container = new PostgreSqlBuilder(PostgresImage).Build();
            await _container.StartAsync();
            _connectionString = _container.GetConnectionString();
        }

        _ = Server;
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();

        if (_container is not null)
        {
            await _container.DisposeAsync();
        }

        if (_externalDatabase is not null)
        {
            await DropExternalDatabaseAsync(_externalDatabase);
        }

        GC.SuppressFinalize(this);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration(config => config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Default"] = _connectionString,
            ["Database:MigrateOnStartup"] = "true",
            ["Database:SeedDemoData"] = "false",
            ["RateLimiting:AuthPermitLimit"] = AuthPermitLimit.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["RateLimiting:SessionPermitLimit"] = "100000",
            ["RateLimiting:ApiPermitLimit"] = "100000",
            ["LoginThrottling:MaxFailuresPerAccount"] = MaxLoginFailuresPerAccount.ToString(System.Globalization.CultureInfo.InvariantCulture),
        }));
        builder.ConfigureTestServices(services => services.AddSingleton<TimeProvider>(Clock));
    }

    private async Task DropExternalDatabaseAsync(string database)
    {
        NpgsqlConnection.ClearAllPools();
        var admin = new NpgsqlConnectionStringBuilder(_connectionString) { Database = "postgres" };
        await using var connection = new NpgsqlConnection(admin.ConnectionString);
        await connection.OpenAsync();

        var quoted = new NpgsqlCommandBuilder().QuoteIdentifier(database);
        await using var drop = connection.CreateCommand();
        drop.CommandText = $"DROP DATABASE IF EXISTS {quoted} WITH (FORCE)";
        await drop.ExecuteNonQueryAsync();
    }
}
