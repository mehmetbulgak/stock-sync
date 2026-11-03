using StockSync.Api.Http;
using StockSync.Application.Common;
using StockSync.Application.Features.Products;

namespace StockSync.Api.Endpoints;

internal sealed class ProductEndpoints : IEndpointGroup
{
    public void Map(IEndpointRouteBuilder api)
    {
        var group = api.MapGroup("/products").WithTags("Products").RequireAuthorization();

        group.MapGet("/", ListAsync)
            .WithValidation<ListProducts.Query>()
            .Produces<PagedResult<ProductDto>>();

        group.MapGet("/{id:guid}", GetAsync).Produces<ProductDto>();

        group.MapPost("/", CreateAsync)
            .WithValidation<CreateProduct.Command>()
            .Produces<ProductDto>(StatusCodes.Status201Created);

        group.MapPut("/{id:guid}", UpdateAsync)
            .WithValidation<UpdateProduct.Command>()
            .Produces<ProductDto>();

        group.MapPost("/{id:guid}/stock-adjustments", AdjustStockAsync)
            .WithValidation<AdjustStock.Command>()
            .Produces<ProductDto>();

        group.MapGet("/{id:guid}/movements", ListMovementsAsync).Produces<IReadOnlyList<StockMovementDto>>();
    }

    private static async Task<IResult> ListAsync([AsParameters] ListProducts.Query query, ListProducts.Handler handler, CancellationToken ct) =>
        TypedResults.Ok(await handler.HandleAsync(query, ct));

    private static async Task<IResult> GetAsync(Guid id, GetProduct.Handler handler, CancellationToken ct) =>
        (await handler.HandleAsync(id, ct)).ToHttpResult();

    private static async Task<IResult> CreateAsync(CreateProduct.Command command, CreateProduct.Handler handler, CancellationToken ct) =>
        (await handler.HandleAsync(command, ct)).ToHttpResult(p => TypedResults.Created($"/api/products/{p.Id}", p));

    private static async Task<IResult> UpdateAsync(Guid id, UpdateProduct.Command command, UpdateProduct.Handler handler, CancellationToken ct) =>
        (await handler.HandleAsync(id, command, ct)).ToHttpResult();

    private static async Task<IResult> AdjustStockAsync(Guid id, AdjustStock.Command command, AdjustStock.Handler handler, CancellationToken ct) =>
        (await handler.HandleAsync(id, command, ct)).ToHttpResult();

    private static async Task<IResult> ListMovementsAsync(Guid id, int? limit, ListStockMovements.Handler handler, CancellationToken ct) =>
        (await handler.HandleAsync(id, limit, ct)).ToHttpResult();
}
