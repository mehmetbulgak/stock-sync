using System.Net;
using StockSync.Api.IntegrationTests.Infrastructure;
using StockSync.Application.Common;
using StockSync.Application.Features.Dashboard;
using StockSync.Application.Features.Orders;
using StockSync.Application.Features.Products;

namespace StockSync.Api.IntegrationTests;

[Collection(ApiCollection.Name)]
public class InventoryFlowTests(ApiFactory factory)
{
    [Fact]
    public async Task Order_lifecycle_reserves_ships_and_never_oversells()
    {
        var seller = await Seller.RegisterAsync(factory);
        var trendyol = await seller.ChannelAsync("Trendyol");
        var product = await seller.CreateProductAsync("CASE-01", stock: 5);

        var created = await seller.PostAsync("/api/orders", Catalog.Order(trendyol, "TY-1001", product.Id, 3));
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var order = await created.ReadAsync<OrderDto>();
        Assert.Equal("Pending", order.Status);

        var afterReserve = await seller.GetAsync<ProductDto>($"/api/products/{product.Id}");
        Assert.Equal((5, 3, 2), (afterReserve.OnHand, afterReserve.Reserved, afterReserve.Available));

        var oversell = await seller.PostAsync("/api/orders", Catalog.Order(trendyol, "TY-1002", product.Id, 3));
        Assert.Equal(HttpStatusCode.Conflict, oversell.StatusCode);
        Assert.Equal("Product.InsufficientStock", await oversell.ProblemCodeAsync());

        Assert.Equal(HttpStatusCode.OK, (await seller.PostAsync($"/api/orders/{order.Id}/ship")).StatusCode);
        var afterShip = await seller.GetAsync<ProductDto>($"/api/products/{product.Id}");
        Assert.Equal((2, 0, 2), (afterShip.OnHand, afterShip.Reserved, afterShip.Available));

        var cancelShipped = await seller.PostAsync($"/api/orders/{order.Id}/cancel", new { reason = "too late" });
        Assert.Equal(HttpStatusCode.Conflict, cancelShipped.StatusCode);
        Assert.Equal("Order.InvalidStatusTransition", await cancelShipped.ProblemCodeAsync());

        var movements = await seller.GetAsync<List<StockMovementDto>>($"/api/products/{product.Id}/movements");
        Assert.Equal(["Shipment", "Reservation", "InitialStock"], movements.Select(m => m.Type).ToArray());
    }

    [Fact]
    public async Task Cancelling_a_pending_order_releases_its_stock()
    {
        var seller = await Seller.RegisterAsync(factory);
        var channel = await seller.ChannelAsync("Web Store");
        var product = await seller.CreateProductAsync("CABLE-01", stock: 4);

        var order = await (await seller.PostAsync("/api/orders", Catalog.Order(channel, null, product.Id, 4))).ReadAsync<OrderDto>();
        Assert.StartsWith("WEB-", order.Number, StringComparison.Ordinal);

        var cancelled = await seller.PostAsync($"/api/orders/{order.Id}/cancel", new { reason = "Customer request" });

        Assert.Equal(HttpStatusCode.OK, cancelled.StatusCode);
        Assert.Equal("Cancelled", (await cancelled.ReadAsync<OrderDto>()).Status);
        Assert.Equal(4, (await seller.GetAsync<ProductDto>($"/api/products/{product.Id}")).Available);
    }

    [Fact]
    public async Task The_same_channel_order_cannot_be_imported_twice()
    {
        var seller = await Seller.RegisterAsync(factory);
        var channel = await seller.ChannelAsync("Hepsiburada");
        var product = await seller.CreateProductAsync("GLASS-01", stock: 10);

        var first = await seller.PostAsync("/api/orders", Catalog.Order(channel, "HB-555", product.Id, 1));
        var second = await seller.PostAsync("/api/orders", Catalog.Order(channel, "HB-555", product.Id, 1));

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
        Assert.Equal("Order.Duplicate", await second.ProblemCodeAsync());
        Assert.Equal(1, (await seller.GetAsync<ProductDto>($"/api/products/{product.Id}")).Reserved);
    }

    [Fact]
    public async Task Concurrent_orders_for_the_last_units_never_oversell()
    {
        var seller = await Seller.RegisterAsync(factory);
        var channel = await seller.ChannelAsync("Trendyol");
        var product = await seller.CreateProductAsync("HOT-ITEM", stock: 3);

        var responses = await Task.WhenAll(Enumerable.Range(0, 12).Select(i =>
            seller.PostAsync("/api/orders", Catalog.Order(channel, $"TY-RACE-{i}", product.Id, 1))));

        var succeeded = responses.Count(r => r.StatusCode == HttpStatusCode.Created);
        Assert.All(responses, r => Assert.Contains(r.StatusCode, new[] { HttpStatusCode.Created, HttpStatusCode.Conflict }));
        Assert.InRange(succeeded, 1, 3);

        var after = await seller.GetAsync<ProductDto>($"/api/products/{product.Id}");
        Assert.Equal(succeeded, after.Reserved);
        Assert.True(after.Available >= 0);
    }

    [Fact]
    public async Task Stock_cannot_be_adjusted_below_reserved_units()
    {
        var seller = await Seller.RegisterAsync(factory);
        var channel = await seller.ChannelAsync("Manual");
        var product = await seller.CreateProductAsync("PB-10K", stock: 5);
        await seller.PostAsync("/api/orders", Catalog.Order(channel, null, product.Id, 4));

        var tooMuch = await seller.PostAsync($"/api/products/{product.Id}/stock-adjustments", new { quantity = -3, reason = "Count" });
        var fine = await seller.PostAsync($"/api/products/{product.Id}/stock-adjustments", new { quantity = 10, reason = "Supplier delivery" });

        Assert.Equal(HttpStatusCode.Conflict, tooMuch.StatusCode);
        Assert.Equal("Product.StockBelowReserved", await tooMuch.ProblemCodeAsync());
        Assert.Equal(HttpStatusCode.OK, fine.StatusCode);
        Assert.Equal(15, (await fine.ReadAsync<ProductDto>()).OnHand);
    }

    [Fact]
    public async Task Duplicate_sku_in_the_same_store_is_rejected()
    {
        var seller = await Seller.RegisterAsync(factory);
        await seller.CreateProductAsync("dup-1", stock: 1);

        var again = await seller.PostAsync("/api/products", Catalog.NewProduct("DUP-1", 1));

        Assert.Equal(HttpStatusCode.Conflict, again.StatusCode);
        Assert.Equal("Product.DuplicateSku", await again.ProblemCodeAsync());
    }

    [Fact]
    public async Task Product_list_supports_search_and_low_stock_filter()
    {
        var seller = await Seller.RegisterAsync(factory);
        await seller.CreateProductAsync("CASE-RED", stock: 50);
        await seller.CreateProductAsync("CASE-BLUE", stock: 1);
        await seller.CreateProductAsync("CHARGER-20W", stock: 50);

        var cases = await seller.GetAsync<PagedResult<ProductDto>>("/api/products?search=case&sort=Sku");
        var low = await seller.GetAsync<PagedResult<ProductDto>>("/api/products?lowStockOnly=true");

        Assert.Equal(["CASE-BLUE", "CASE-RED"], cases.Items.Select(p => p.Sku).ToArray());
        Assert.Equal("CASE-BLUE", Assert.Single(low.Items).Sku);
    }

    [Fact]
    public async Task Dashboard_summarises_the_store()
    {
        var seller = await Seller.RegisterAsync(factory);
        var channel = await seller.ChannelAsync("Trendyol");
        var product = await seller.CreateProductAsync("DASH-1", stock: 10, price: 100m);
        await seller.PostAsync("/api/orders", Catalog.Order(channel, "TY-D1", product.Id, 2));

        var dashboard = await seller.GetAsync<DashboardDto>("/api/dashboard");

        Assert.Equal(1, dashboard.ActiveProducts);
        Assert.Equal(1, dashboard.PendingOrders);
        Assert.Equal(1, dashboard.OrdersToday);
        Assert.Equal(200m, dashboard.RevenueLast7Days);
        Assert.Equal(GetDashboard.Days, dashboard.DailySales.Count);
        Assert.Equal("Trendyol", Assert.Single(dashboard.SalesByChannel).ChannelName);
    }
}
