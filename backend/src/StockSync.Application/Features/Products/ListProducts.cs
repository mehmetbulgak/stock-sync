using FluentValidation;
using Microsoft.EntityFrameworkCore;
using StockSync.Application.Abstractions;
using StockSync.Application.Common;
using StockSync.Domain.Products;

namespace StockSync.Application.Features.Products;

public static class ListProducts
{
    public sealed record Query(
        string? Search = null,
        bool LowStockOnly = false,
        bool IncludeInactive = false,
        ProductSort Sort = ProductSort.Name,
        bool Descending = false,
        int? Page = null,
        int? PageSize = null);

    public sealed class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(q => q.Search).ValidSearch();
            RuleFor(q => q.Sort).IsInEnum();
            RuleFor(q => q.Page).ValidPage();
            RuleFor(q => q.PageSize).ValidPageSize();
        }
    }

    public sealed class Handler(IAppDbContext db)
    {
        public Task<PagedResult<ProductDto>> HandleAsync(Query query, CancellationToken ct)
        {
            var products = db.Products.AsNoTracking();

            if (!query.IncludeInactive)
            {
                products = products.Where(p => p.IsActive);
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var term = query.Search.Trim().ToLowerInvariant();
                products = products.Where(p => p.Sku.ToLower().Contains(term) || p.Name.ToLower().Contains(term));
            }

            if (query.LowStockOnly)
            {
                products = products.Where(p => p.OnHand - p.Reserved <= p.LowStockThreshold);
            }

            return Sort(products, query.Sort, query.Descending)
                .Select(ProductDto.Projection)
                .ToPagedResultAsync(query.Page, query.PageSize, ct);
        }

        private static IQueryable<Product> Sort(IQueryable<Product> products, ProductSort sort, bool descending) =>
            (sort, descending) switch
            {
                (ProductSort.Sku, false) => products.OrderBy(p => p.Sku),
                (ProductSort.Sku, true) => products.OrderByDescending(p => p.Sku),
                (ProductSort.Available, false) => products.OrderBy(p => p.OnHand - p.Reserved).ThenBy(p => p.Name),
                (ProductSort.Available, true) => products.OrderByDescending(p => p.OnHand - p.Reserved).ThenBy(p => p.Name),
                (ProductSort.UpdatedAt, false) => products.OrderBy(p => p.UpdatedAt),
                (ProductSort.UpdatedAt, true) => products.OrderByDescending(p => p.UpdatedAt),
                (_, true) => products.OrderByDescending(p => p.Name),
                _ => products.OrderBy(p => p.Name),
            };
    }
}
