namespace StockSync.Api.Http;

internal sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
    private const string ApiContentSecurityPolicy = "default-src 'none'; frame-ancestors 'none'";

    private static readonly Func<object, Task> ApplyHeaders = state =>
    {
        var context = (HttpContext)state;
        var headers = context.Response.Headers;
        headers.XContentTypeOptions = "nosniff";
        headers.XFrameOptions = "DENY";
        headers["Referrer-Policy"] = "no-referrer";
        headers["Cross-Origin-Resource-Policy"] = "same-origin";

        if (context.Request.Path.StartsWithSegments("/api"))
        {
            headers.CacheControl = "no-store";
            headers.ContentSecurityPolicy = ApiContentSecurityPolicy;
            headers["X-Robots-Tag"] = "noindex";
        }

        return Task.CompletedTask;
    };

    public Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(ApplyHeaders, context);
        return next(context);
    }
}
