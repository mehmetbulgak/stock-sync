using StockSync.Api.Http;
using StockSync.Application.Common;
using StockSync.Application.Features.Orders;

namespace StockSync.Api.Endpoints;

internal sealed class OrderEndpoints : IEndpointGroup
{
    public void Map(IEndpointRouteBuilder api)
    {
        var group = api.MapGroup("/orders").WithTags("Orders").RequireAuthorization();

        group.MapGet("/", ListAsync)
            .WithValidation<ListOrders.Query>()
            .Produces<PagedResult<OrderSummaryDto>>();

        group.MapGet("/{id:guid}", GetAsync).Produces<OrderDto>();

        group.MapPost("/", CreateAsync)
            .WithValidation<CreateOrder.Command>()
            .Produces<OrderDto>(StatusCodes.Status201Created);

        group.MapPost("/{id:guid}/ship", ShipAsync).Produces<OrderDto>();

        group.MapPost("/{id:guid}/cancel", CancelAsync)
            .WithValidation<CancelOrder.Command>()
            .Produces<OrderDto>();
    }

    private static async Task<IResult> ListAsync([AsParameters] ListOrders.Query query, ListOrders.Handler handler, CancellationToken ct) =>
        TypedResults.Ok(await handler.HandleAsync(query, ct));

    private static async Task<IResult> GetAsync(Guid id, GetOrder.Handler handler, CancellationToken ct) =>
        (await handler.HandleAsync(id, ct)).ToHttpResult();

    private static async Task<IResult> CreateAsync(CreateOrder.Command command, CreateOrder.Handler handler, CancellationToken ct) =>
        (await handler.HandleAsync(command, ct)).ToHttpResult(o => TypedResults.Created($"/api/orders/{o.Id}", o));

    private static async Task<IResult> ShipAsync(Guid id, ShipOrder.Handler handler, CancellationToken ct) =>
        (await handler.HandleAsync(id, ct)).ToHttpResult();

    private static async Task<IResult> CancelAsync(Guid id, CancelOrder.Command? command, CancelOrder.Handler handler, CancellationToken ct) =>
        (await handler.HandleAsync(id, command ?? new CancelOrder.Command(null), ct)).ToHttpResult();
}
