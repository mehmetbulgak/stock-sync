using Microsoft.EntityFrameworkCore;
using StockSync.Application.Abstractions;
using StockSync.Domain.Common;
using StockSync.Domain.Products;

namespace StockSync.Application.Features.Products;

public static class GetProduct
{
    public sealed class Handler(IAppDbContext db)
    {
        public async Task<Result<ProductDto>> HandleAsync(Guid id, CancellationToken ct)
        {
            var product = await db.Products.AsNoTracking()
                .Where(p => p.Id == id)
                .Select(ProductDto.Projection)
                .SingleOrDefaultAsync(ct);

            return product is null ? ProductErrors.NotFound(id) : product;
        }
    }
}
