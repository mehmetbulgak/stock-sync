using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using StockSync.Api.Http;

namespace StockSync.Api.Configuration;

internal static class ApiPipeline
{
    public static WebApplication UseApiPipeline(this WebApplication app)
    {
        app.UseMiddleware<SecurityHeadersMiddleware>();

        if (app.Services.GetRequiredService<IOptions<ReverseProxyOptions>>().Value.Enabled)
        {
            app.UseForwardedHeaders();
        }

        app.UseExceptionHandler();
        app.UseStatusCodePages();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi().AllowAnonymous();
            app.MapScalarApiReference("/docs").AllowAnonymous();
        }

        app.UseAuthentication();
        app.UseRateLimiter();
        app.UseAuthorization();

        return app;
    }
}
