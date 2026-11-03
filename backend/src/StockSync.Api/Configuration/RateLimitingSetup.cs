using System.Globalization;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using StockSync.Infrastructure.Auth;

namespace StockSync.Api.Configuration;

internal static class RateLimitingSetup
{
    public const string AuthPolicy = "auth";
    public const string SessionPolicy = "session";
    public const string ApiPolicy = "api";

    public static IServiceCollection AddApiRateLimiting(this IServiceCollection services)
    {
        services.AddOptions<RateLimitingOptions>()
            .BindConfiguration(RateLimitingOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = (context, _) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter =
                        ((int)retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture);
                }

                return ValueTask.CompletedTask;
            };

            options.AddPolicy(AuthPolicy, http => FixedWindow(http, ClientAddress(http), o => o.AuthPermitLimit));
            options.AddPolicy(SessionPolicy, http => FixedWindow(http, ClientAddress(http), o => o.SessionPermitLimit));
            options.AddPolicy(ApiPolicy, http => FixedWindow(http, UserOrClientAddress(http), o => o.ApiPermitLimit));
        });

        return services;
    }

    private static RateLimitPartition<string> FixedWindow(HttpContext http, string partitionKey, Func<RateLimitingOptions, int> permitLimit)
    {
        var options = http.RequestServices.GetRequiredService<IOptions<RateLimitingOptions>>().Value;
        return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = permitLimit(options),
            Window = TimeSpan.FromSeconds(options.WindowSeconds),
            QueueLimit = 0,
        });
    }

    private static string ClientAddress(HttpContext http) =>
        http.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    private static string UserOrClientAddress(HttpContext http) =>
        http.User.FindFirstValue(AppClaims.UserId) is { } userId ? $"user:{userId}" : $"ip:{ClientAddress(http)}";
}
