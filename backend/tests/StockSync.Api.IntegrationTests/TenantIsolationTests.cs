using System.Net;
using System.Net.Http.Json;
using StockSync.Api.IntegrationTests.Infrastructure;
using StockSync.Application.Common;
using StockSync.Application.Features.Orders;
using StockSync.Application.Features.Products;

namespace StockSync.Api.IntegrationTests;

[Collection(ApiCollection.Name)]
public class TenantIsolationTests(ApiFactory factory)
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Sellers_cannot_read_each_others_products()
    {
        var (alice, bob) = (await Seller.RegisterAsync(factory), await Seller.RegisterAsync(factory));
        var product = await alice.CreateProductAsync("SECRET-1", 1);

        var direct = await bob.Client.GetAsync($"/api/products/{product.Id}", Ct);
        var movements = await bob.Client.GetAsync($"/api/products/{product.Id}/movements", Ct);
        var list = await bob.GetAsync<PagedResult<ProductDto>>("/api/products");

        Assert.Equal(HttpStatusCode.NotFound, direct.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, movements.StatusCode);
        Assert.Empty(list.Items);
    }

    [Fact]
    public async Task Sellers_cannot_change_each_others_products()
    {
        var (alice, bob) = (await Seller.RegisterAsync(factory), await Seller.RegisterAsync(factory));
        var product = await alice.CreateProductAsync("SECRET-2", 5);

        var update = await bob.Client.PutAsJsonAsync(
            $"/api/products/{product.Id}",
            new { name = "Hijacked", description = (string?)null, price = 1m, lowStockThreshold = 0, isActive = true },
            Ct);
        var adjust = await bob.PostAsync($"/api/products/{product.Id}/stock-adjustments", new { quantity = -5, reason = "Theft" });

        Assert.Equal(HttpStatusCode.NotFound, update.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, adjust.StatusCode);
        var unchanged = await alice.GetAsync<ProductDto>($"/api/products/{product.Id}");
        Assert.Equal((5, "Product SECRET-2"), (unchanged.OnHand, unchanged.Name));
    }

    [Fact]
    public async Task Sellers_cannot_order_from_or_into_another_store()
    {
        var (alice, bob) = (await Seller.RegisterAsync(factory), await Seller.RegisterAsync(factory));
        var aliceProduct = await alice.CreateProductAsync("SECRET-3", 5);
        var aliceChannel = await alice.ChannelAsync("Trendyol");
        var bobChannel = await bob.ChannelAsync("Trendyol");

        var foreignProduct = await bob.PostAsync("/api/orders", Catalog.Order(bobChannel, "TY-X1", aliceProduct.Id, 1));
        var foreignChannel = await bob.PostAsync("/api/orders", Catalog.Order(aliceChannel, "TY-X2", aliceProduct.Id, 1));

        Assert.Equal(HttpStatusCode.BadRequest, foreignProduct.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, foreignChannel.StatusCode);
        Assert.Equal(0, (await alice.GetAsync<ProductDto>($"/api/products/{aliceProduct.Id}")).Reserved);
    }

    [Fact]
    public async Task Sellers_cannot_ship_each_others_orders()
    {
        var (alice, bob) = (await Seller.RegisterAsync(factory), await Seller.RegisterAsync(factory));
        var product = await alice.CreateProductAsync("SECRET-4", 5);
        var channel = await alice.ChannelAsync("Manual");
        var order = await (await alice.PostAsync("/api/orders", Catalog.Order(channel, null, product.Id, 1))).ReadAsync<OrderDto>();

        var ship = await bob.PostAsync($"/api/orders/{order.Id}/ship");
        var orders = await bob.GetAsync<PagedResult<OrderSummaryDto>>("/api/orders");

        Assert.Equal(HttpStatusCode.NotFound, ship.StatusCode);
        Assert.Empty(orders.Items);
    }

    [Fact]
    public async Task The_same_sku_can_exist_in_different_stores()
    {
        var (alice, bob) = (await Seller.RegisterAsync(factory), await Seller.RegisterAsync(factory));

        await alice.CreateProductAsync("SHARED-SKU", 1);
        await bob.CreateProductAsync("SHARED-SKU", 1);
    }
}
