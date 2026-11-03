using Microsoft.EntityFrameworkCore;
using StockSync.Application.Abstractions;
using StockSync.Domain.Common;
using StockSync.Domain.Orders;
using StockSync.Domain.Products;

namespace StockSync.Application.Features.Orders;

internal static class OrderTransition
{
    public static async Task<Result<OrderDto>> RunAsync(
        IAppDbContext db,
        Guid orderId,
        Func<Order, IReadOnlyDictionary<Guid, Product>, Result> transition,
        CancellationToken ct)
    {
        var order = await db.Orders
            .Include(o => o.Channel)
            .Include(o => o.Lines)
            .SingleOrDefaultAsync(o => o.Id == orderId, ct);

        if (order is null)
        {
            return OrderErrors.NotFound(orderId);
        }

        var productIds = order.Lines.Select(l => l.ProductId).Distinct().ToList();
        var products = await db.Products.Where(p => productIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id, ct);

        var result = transition(order, products);
        if (result.IsFailure)
        {
            return result.Error!;
        }

        await db.SaveChangesAsync(ct);
        return OrderDto.From(order);
    }
}
