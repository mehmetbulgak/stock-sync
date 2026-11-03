using StockSync.Domain.Channels;
using StockSync.Domain.Products;

namespace StockSync.Domain.Tests;

internal static class TestData
{
    public static readonly Guid TenantId = Guid.CreateVersion7();

    public static readonly DateTimeOffset Now = new(2026, 11, 3, 9, 0, 0, TimeSpan.Zero);

    public static Product Product(string sku = "CASE-01", int stock = 10, decimal price = 100m, int threshold = 2, Guid? tenantId = null) =>
        Domain.Products.Product.Create(tenantId ?? TenantId, sku, $"Product {sku}", null, price, stock, threshold, Now).Value;

    public static SalesChannel Channel(ChannelType type = ChannelType.Trendyol, Guid? tenantId = null) =>
        SalesChannel.Create(tenantId ?? TenantId, type.ToString(), type, Now);

    public static Dictionary<Guid, Product> Lookup(params Product[] products) => products.ToDictionary(p => p.Id);
}
