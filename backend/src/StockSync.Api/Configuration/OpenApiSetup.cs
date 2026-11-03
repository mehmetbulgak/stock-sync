namespace StockSync.Api.Configuration;

internal static class OpenApiSetup
{
    public static IServiceCollection AddApiDocumentation(this IServiceCollection services) =>
        services.AddOpenApi(options => options.AddDocumentTransformer<BearerSecuritySchemeTransformer>());
}
