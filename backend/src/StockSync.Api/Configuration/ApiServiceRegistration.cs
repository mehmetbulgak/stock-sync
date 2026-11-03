using System.Text.Json.Serialization;
using StockSync.Api.Http;
using StockSync.Application.Abstractions;
using StockSync.Infrastructure.Persistence;

namespace StockSync.Api.Configuration;

internal static class ApiServiceRegistration
{
    public const long MaxRequestBodyBytes = 1024 * 1024;

    public static WebApplicationBuilder AddApi(this WebApplicationBuilder builder)
    {
        builder.WebHost.ConfigureKestrel(kestrel =>
        {
            kestrel.AddServerHeader = false;
            kestrel.Limits.MaxRequestBodySize = MaxRequestBodyBytes;
        });

        var services = builder.Services;
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.ConfigureHttpJsonOptions(o =>
        {
            o.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
            o.SerializerOptions.RespectNullableAnnotations = true;
        });
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddHealthChecks().AddDbContextCheck<AppDbContext>("database");
        services.AddHostedService<StartupDiagnostics>();

        services
            .AddApiAuthentication()
            .AddApiRateLimiting()
            .AddReverseProxySupport()
            .AddApiDocumentation();

        return builder;
    }
}
