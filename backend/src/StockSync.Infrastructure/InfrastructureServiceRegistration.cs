using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StockSync.Application.Abstractions;
using StockSync.Infrastructure.Auth;
using StockSync.Infrastructure.Persistence;
using StockSync.Infrastructure.Persistence.Seeding;

namespace StockSync.Infrastructure;

public static class InfrastructureServiceRegistration
{
    private const string ConnectionStringName = "Default";
    private const int MaxRetryCount = 3;

    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddDbContext<AppDbContext>((sp, options) => options
            .UseNpgsql(ResolveConnectionString(sp), npgsql => npgsql.EnableRetryOnFailure(MaxRetryCount))
            .UseSnakeCaseNamingConvention()
            .ConfigureWarnings(w => w.Ignore(CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning)));
        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        services.AddOptions<DatabaseOptions>().BindConfiguration(DatabaseOptions.SectionName);
        services.AddOptions<JwtOptions>()
            .BindConfiguration(JwtOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton<IPostConfigureOptions<JwtOptions>, JwtOptionsSetup>();
        services.AddOptions<LoginThrottleOptions>()
            .BindConfiguration(LoginThrottleOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ILoginThrottle, LoginThrottle>();
        services.AddScoped<DemoDataSeeder>();
        services.AddScoped<DatabaseInitializer>();

        return services;
    }

    private static string ResolveConnectionString(IServiceProvider services) =>
        services.GetRequiredService<IConfiguration>().GetConnectionString(ConnectionStringName)
        ?? throw new InvalidOperationException($"Connection string '{ConnectionStringName}' is not configured.");
}
