using System.Net;
using StockSync.Application.Features.Channels;
using StockSync.Application.Features.Products;

namespace StockSync.Api.IntegrationTests.Infrastructure;

public static class Catalog
{
    public static object NewProduct(string sku, int stock, decimal price = 99.90m) =>
        new { sku, name = $"Product {sku}", description = (string?)null, price, initialStock = stock, lowStockThreshold = 2 };

    public static async Task<ProductDto> CreateProductAsync(this Seller seller, string sku, int stock, decimal price = 99.90m)
    {
        var response = await seller.PostAsync("/api/products", NewProduct(sku, stock, price));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await response.ReadAsync<ProductDto>();
    }

    public static async Task<ChannelDto> ChannelAsync(this Seller seller, string name) =>
        (await seller.GetAsync<List<ChannelDto>>("/api/channels")).Single(c => c.Name == name);

    public static object Order(ChannelDto channel, string? number, Guid productId, int quantity) => new
    {
        channelId = channel.Id,
        number,
        customerName = "Test Customer",
        items = new[] { new { productId, quantity } },
    };
}
