using System.Linq.Expressions;
using StockSync.Domain.Products;

namespace StockSync.Application.Features.Products;

public sealed record ProductDto(
    Guid Id,
    string Sku,
    string Name,
    string? Description,
    decimal Price,
    int OnHand,
    int Reserved,
    int Available,
    int LowStockThreshold,
    bool IsLowStock,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    public static readonly Expression<Func<Product, ProductDto>> Projection = p => new ProductDto(
        p.Id,
        p.Sku,
        p.Name,
        p.Description,
        p.Price,
        p.OnHand,
        p.Reserved,
        p.OnHand - p.Reserved,
        p.LowStockThreshold,
        p.OnHand - p.Reserved <= p.LowStockThreshold,
        p.IsActive,
        p.CreatedAt,
        p.UpdatedAt);

    public static ProductDto From(Product p) => new(
        p.Id,
        p.Sku,
        p.Name,
        p.Description,
        p.Price,
        p.OnHand,
        p.Reserved,
        p.Available,
        p.LowStockThreshold,
        p.IsLowStock,
        p.IsActive,
        p.CreatedAt,
        p.UpdatedAt);
}
