using StockSync.Domain.Channels;
using StockSync.Domain.Orders;
using StockSync.Domain.Products;

namespace StockSync.Domain.Tests;

public class OrderTests
{
    [Fact]
    public void Place_reserves_stock_and_calculates_total()
    {
        var caseProduct = TestData.Product("CASE-01", stock: 10, price: 150m);
        var cable = TestData.Product("CABLE-01", stock: 5, price: 80m);

        var result = Order.Place(TestData.Channel(), "TY-1001", "Ayşe Yılmaz", [new OrderItem(caseProduct, 2), new OrderItem(cable, 1)], TestData.Now);

        Assert.True(result.IsSuccess);
        var order = result.Value;
        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.Equal(380m, order.Total);
        Assert.Equal(2, caseProduct.Reserved);
        Assert.Equal(8, caseProduct.Available);
        Assert.Equal(1, cable.Reserved);
        Assert.Equal(StockMovementType.Reservation, caseProduct.Movements.Last().Type);
        Assert.Equal(order.Id, caseProduct.Movements.Last().OrderId);
    }

    [Fact]
    public void Place_is_all_or_nothing_when_one_line_lacks_stock()
    {
        var plenty = TestData.Product("CASE-01", stock: 10);
        var scarce = TestData.Product("CABLE-01", stock: 1);

        var result = Order.Place(TestData.Channel(), "TY-1002", "Mehmet Kaya", [new OrderItem(plenty, 3), new OrderItem(scarce, 2)], TestData.Now);

        Assert.Equal("Product.InsufficientStock", result.Error!.Code);
        Assert.Equal(0, plenty.Reserved);
        Assert.Equal(0, scarce.Reserved);
    }

    [Fact]
    public void Place_checks_combined_quantity_when_a_product_repeats()
    {
        var product = TestData.Product(stock: 3);

        var result = Order.Place(TestData.Channel(), "TY-1003", "Can Demir", [new OrderItem(product, 2), new OrderItem(product, 2)], TestData.Now);

        Assert.Equal("Product.InsufficientStock", result.Error!.Code);
        Assert.Equal(0, product.Reserved);
    }

    [Fact]
    public void Place_merges_repeated_products_into_one_line()
    {
        var product = TestData.Product(stock: 10, price: 50m);

        var order = Order.Place(TestData.Channel(), "TY-1004", "Can Demir", [new OrderItem(product, 2), new OrderItem(product, 3)], TestData.Now).Value;

        var line = Assert.Single(order.Lines);
        Assert.Equal(5, line.Quantity);
        Assert.Equal(250m, order.Total);
    }

    [Fact]
    public void Place_uses_channel_price_when_given()
    {
        var product = TestData.Product(price: 100m);

        var order = Order.Place(TestData.Channel(), "TY-1005", "Elif Şahin", [new OrderItem(product, 2, UnitPrice: 89.90m)], TestData.Now).Value;

        Assert.Equal(179.80m, order.Total);
    }

    [Fact]
    public void Place_rejects_products_from_another_tenant()
    {
        var foreign = TestData.Product(tenantId: Guid.CreateVersion7());

        var result = Order.Place(TestData.Channel(), "TY-1006", "Elif Şahin", [new OrderItem(foreign, 1)], TestData.Now);

        Assert.Equal(OrderErrors.ProductMissing, result.Error);
    }

    [Theory]
    [InlineData("", "Customer", "Order.NumberRequired")]
    [InlineData("TY-1", " ", "Order.CustomerRequired")]
    public void Place_validates_header_fields(string number, string customer, string expectedCode)
    {
        var result = Order.Place(TestData.Channel(), number, customer, [new OrderItem(TestData.Product(), 1)], TestData.Now);

        Assert.Equal(expectedCode, result.Error!.Code);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(Order.MaxQuantityPerLine + 1)]
    public void Place_rejects_out_of_range_quantities(int quantity)
    {
        var product = TestData.Product(stock: Order.MaxQuantityPerLine * 2);

        var result = Order.Place(TestData.Channel(), "TY-Q", "Customer", [new OrderItem(product, quantity)], TestData.Now);

        Assert.Equal(OrderErrors.InvalidQuantity, result.Error);
        Assert.Equal(0, product.Reserved);
    }

    [Fact]
    public void Place_rejects_negative_unit_price()
    {
        var result = Order.Place(TestData.Channel(), "TY-P", "Customer", [new OrderItem(TestData.Product(), 1, UnitPrice: -1m)], TestData.Now);

        Assert.Equal(OrderErrors.InvalidUnitPrice, result.Error);
    }

    [Fact]
    public void Place_limits_the_number_of_lines()
    {
        var items = Enumerable.Range(0, Order.MaxLines + 1)
            .Select(i => new OrderItem(TestData.Product($"SKU-{i}"), 1))
            .ToList();

        var result = Order.Place(TestData.Channel(), "TY-L", "Customer", items, TestData.Now);

        Assert.Equal(OrderErrors.TooManyItems, result.Error);
    }

    [Fact]
    public void Place_requires_items()
    {
        Assert.Equal(OrderErrors.NoItems, Order.Place(TestData.Channel(), "TY-1", "Customer", [], TestData.Now).Error);
    }

    [Fact]
    public void Ship_removes_reserved_units_from_on_hand()
    {
        var product = TestData.Product(stock: 10);
        var order = Order.Place(TestData.Channel(), "TY-2001", "Ali Veli", [new OrderItem(product, 4)], TestData.Now).Value;

        var result = order.Ship(TestData.Lookup(product), TestData.Now.AddHours(2));

        Assert.True(result.IsSuccess);
        Assert.Equal(OrderStatus.Shipped, order.Status);
        Assert.NotNull(order.ShippedAt);
        Assert.Equal(6, product.OnHand);
        Assert.Equal(0, product.Reserved);
        Assert.Equal(StockMovementType.Shipment, product.Movements.Last().Type);
    }

    [Fact]
    public void Cancel_releases_reservation()
    {
        var product = TestData.Product(stock: 10);
        var order = Order.Place(TestData.Channel(), "TY-2002", "Ali Veli", [new OrderItem(product, 4)], TestData.Now).Value;

        var result = order.Cancel(TestData.Lookup(product), "Customer changed their mind", TestData.Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.Equal("Customer changed their mind", order.CancellationReason);
        Assert.Equal(10, product.OnHand);
        Assert.Equal(10, product.Available);
    }

    [Fact]
    public void Shipped_order_cannot_be_cancelled_or_shipped_again()
    {
        var product = TestData.Product(stock: 10);
        var order = Order.Place(TestData.Channel(), "TY-2003", "Ali Veli", [new OrderItem(product, 1)], TestData.Now).Value;
        order.Ship(TestData.Lookup(product), TestData.Now);

        Assert.Equal("Order.InvalidStatusTransition", order.Cancel(TestData.Lookup(product), null, TestData.Now).Error!.Code);
        Assert.Equal("Order.InvalidStatusTransition", order.Ship(TestData.Lookup(product), TestData.Now).Error!.Code);
        Assert.Equal(9, product.OnHand);
    }

    [Fact]
    public void Inactive_channel_cannot_take_orders()
    {
        var channel = TestData.Channel(ChannelType.Hepsiburada);
        channel.Deactivate();

        var result = Order.Place(channel, "HB-1", "Customer", [new OrderItem(TestData.Product(), 1)], TestData.Now);

        Assert.Equal(OrderErrors.ChannelInactive, result.Error);
    }
}
