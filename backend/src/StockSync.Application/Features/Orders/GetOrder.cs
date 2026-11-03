using Microsoft.EntityFrameworkCore;
using StockSync.Application.Abstractions;
using StockSync.Domain.Common;
using StockSync.Domain.Orders;

namespace StockSync.Application.Features.Orders;

public static class GetOrder
{
    public sealed class Handler(IAppDbContext db)
    {
        public async Task<Result<OrderDto>> HandleAsync(Guid id, CancellationToken ct)
        {
            var order = await db.Orders.AsNoTracking()
                .Include(o => o.Channel)
                .Include(o => o.Lines)
                .SingleOrDefaultAsync(o => o.Id == id, ct);

            return order is null ? OrderErrors.NotFound(id) : OrderDto.From(order);
        }
    }
}
