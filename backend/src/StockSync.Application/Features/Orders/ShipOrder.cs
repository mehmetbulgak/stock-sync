using StockSync.Application.Abstractions;
using StockSync.Domain.Common;

namespace StockSync.Application.Features.Orders;

public static class ShipOrder
{
    public sealed class Handler(IAppDbContext db, TimeProvider clock)
    {
        public Task<Result<OrderDto>> HandleAsync(Guid id, CancellationToken ct) =>
            OrderTransition.RunAsync(db, id, (order, products) => order.Ship(products, clock.GetUtcNow()), ct);
    }
}
