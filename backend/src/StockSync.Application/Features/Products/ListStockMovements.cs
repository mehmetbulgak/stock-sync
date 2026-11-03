using Microsoft.EntityFrameworkCore;
using StockSync.Application.Abstractions;
using StockSync.Domain.Common;
using StockSync.Domain.Products;

namespace StockSync.Application.Features.Products;

public static class ListStockMovements
{
    public const int DefaultLimit = 50;
    public const int MaxLimit = 200;

    public sealed class Handler(IAppDbContext db)
    {
        public async Task<Result<IReadOnlyList<StockMovementDto>>> HandleAsync(Guid productId, int? limit, CancellationToken ct)
        {
            if (!await db.Products.AnyAsync(p => p.Id == productId, ct))
            {
                return ProductErrors.NotFound(productId);
            }

            var movements = await db.StockMovements.AsNoTracking()
                .Where(m => m.ProductId == productId)
                .OrderByDescending(m => m.CreatedAt)
                .ThenByDescending(m => m.Id)
                .Take(Math.Clamp(limit ?? DefaultLimit, 1, MaxLimit))
                .Select(m => new StockMovementDto(
                    m.Id,
                    m.Type.ToString(),
                    m.Quantity,
                    m.OnHandAfter,
                    m.ReservedAfter,
                    m.Note,
                    m.OrderId,
                    m.CreatedAt))
                .ToListAsync(ct);

            return Result<IReadOnlyList<StockMovementDto>>.Success(movements);
        }
    }
}
