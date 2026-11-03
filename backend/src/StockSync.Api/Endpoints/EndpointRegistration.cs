using System.Reflection;
using StockSync.Api.Configuration;

namespace StockSync.Api.Endpoints;

internal static class EndpointRegistration
{
    public static WebApplication MapApiEndpoints(this WebApplication app)
    {
        app.MapHealthChecks("/health")
            .AllowAnonymous()
            .RequireRateLimiting(RateLimitingSetup.ApiPolicy);

        var api = app.MapGroup("/api").RequireRateLimiting(RateLimitingSetup.ApiPolicy);

        var groups = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(IEndpointGroup).IsAssignableFrom(t))
            .Select(t => (IEndpointGroup)Activator.CreateInstance(t)!);

        foreach (var group in groups)
        {
            group.Map(api);
        }

        return app;
    }
}
