using StockSync.Domain.Products;

namespace StockSync.Domain.Tests;

public class ProductTests
{
    [Fact]
    public void Create_normalizes_sku_and_records_initial_stock()
    {
        var result = Product.Create(TestData.TenantId, "  case-01 ", " iPhone Case ", "  ", 149.90m, 25, 5, TestData.Now);

        Assert.True(result.IsSuccess);
        var product = result.Value;
        Assert.Equal("CASE-01", product.Sku);
        Assert.Equal("iPhone Case", product.Name);
        Assert.Null(product.Description);
        Assert.Equal(25, product.Available);

        var movement = Assert.Single(product.Movements);
        Assert.Equal(StockMovementType.InitialStock, movement.Type);
        Assert.Equal(25, movement.OnHandAfter);
    }

    [Fact]
    public void Create_without_initial_stock_records_no_movement()
    {
        var product = TestData.Product(stock: 0);

        Assert.Empty(product.Movements);
        Assert.True(product.IsLowStock);
    }

    [Theory]
    [InlineData("", "Name", 10, 0, 0, "Product.SkuRequired")]
    [InlineData("SKU", " ", 10, 0, 0, "Product.InvalidName")]
    [InlineData("SKU", "Name", -1, 0, 0, "Product.InvalidPrice")]
    [InlineData("SKU", "Name", 1_000_001, 0, 0, "Product.InvalidPrice")]
    [InlineData("SKU", "Name", 10, -5, 0, "Product.InvalidStock")]
    [InlineData("SKU", "Name", 10, Product.MaxStock + 1, 0, "Product.InvalidStock")]
    [InlineData("SKU", "Name", 10, 0, -1, "Product.InvalidThreshold")]
    public void Create_rejects_invalid_input(string sku, string name, decimal price, int stock, int threshold, string expectedCode)
    {
        var result = Product.Create(TestData.TenantId, sku, name, null, price, stock, threshold, TestData.Now);

        Assert.True(result.IsFailure);
        Assert.Equal(expectedCode, result.Error!.Code);
    }

    [Fact]
    public void AdjustStock_changes_on_hand_and_writes_ledger_entry()
    {
        var product = TestData.Product(stock: 10);

        var result = product.AdjustStock(-3, "Damaged in warehouse", TestData.Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(7, product.OnHand);
        var movement = product.Movements.Last();
        Assert.Equal(StockMovementType.Adjustment, movement.Type);
        Assert.Equal(-3, movement.Quantity);
        Assert.Equal("Damaged in warehouse", movement.Note);
    }

    [Fact]
    public void AdjustStock_cannot_drop_below_reserved_units()
    {
        var product = TestData.Product(stock: 10);
        product.Reserve(8, Guid.NewGuid(), TestData.Now);

        var result = product.AdjustStock(-5, "Count correction", TestData.Now);

        Assert.Equal("Product.StockBelowReserved", result.Error!.Code);
        Assert.Equal(10, product.OnHand);
    }

    [Fact]
    public void AdjustStock_cannot_exceed_the_stock_limit()
    {
        var product = TestData.Product(stock: Product.MaxStock - 5);

        var result = product.AdjustStock(int.MaxValue, "Overflow attempt", TestData.Now);

        Assert.Equal("Product.StockAboveLimit", result.Error!.Code);
        Assert.Equal(Product.MaxStock - 5, product.OnHand);
    }

    [Fact]
    public void AdjustStock_rejects_zero()
    {
        var product = TestData.Product();

        Assert.Equal(ProductErrors.ZeroAdjustment, product.AdjustStock(0, "noop", TestData.Now).Error);
    }

    [Fact]
    public void Reserve_fails_when_not_enough_available()
    {
        var product = TestData.Product(stock: 2);

        var result = product.Reserve(3, Guid.NewGuid(), TestData.Now);

        Assert.Equal("Product.InsufficientStock", result.Error!.Code);
        Assert.Equal(0, product.Reserved);
    }

    [Fact]
    public void Inactive_product_cannot_be_reserved()
    {
        var product = TestData.Product();
        product.Update(product.Name, null, product.Price, product.LowStockThreshold, isActive: false, TestData.Now);

        Assert.Equal("Product.Inactive", product.Reserve(1, Guid.NewGuid(), TestData.Now).Error!.Code);
    }

    [Fact]
    public void IsLowStock_compares_available_units_with_threshold()
    {
        var product = TestData.Product(stock: 5, threshold: 2);
        Assert.False(product.IsLowStock);

        product.Reserve(3, Guid.NewGuid(), TestData.Now);

        Assert.Equal(2, product.Available);
        Assert.True(product.IsLowStock);
    }
}
