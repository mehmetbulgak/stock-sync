using StockSync.Domain.Common;

namespace StockSync.Domain.Products;

public static class ProductErrors
{
    public static readonly Error SkuRequired = Error.Validation("Product.SkuRequired", "SKU is required.");

    public static readonly Error InvalidName = Error.Validation("Product.InvalidName", $"Name must be 1-{Product.NameMaxLength} characters.");

    public static readonly Error InvalidPrice = Error.Validation(
        "Product.InvalidPrice",
        $"Price must be between 0 and {Product.MaxPrice:N0}.");

    public static readonly Error InvalidStock = Error.Validation(
        "Product.InvalidStock",
        $"Stock must be between 0 and {Product.MaxStock:N0} units.");

    public static readonly Error InvalidThreshold = Error.Validation(
        "Product.InvalidThreshold",
        $"Low-stock threshold must be between 0 and {Product.MaxStock:N0}.");

    public static readonly Error ZeroAdjustment = Error.Validation("Product.ZeroAdjustment", "Adjustment quantity cannot be zero.");

    public static readonly Error InvalidQuantity = Error.Validation("Product.InvalidQuantity", "Quantity must be greater than zero.");

    public static Error NotFound(Guid id) => Error.NotFound("Product.NotFound", $"Product '{id}' was not found.");

    public static Error DuplicateSku(string sku) => Error.Conflict("Product.DuplicateSku", $"A product with SKU '{sku}' already exists.");

    public static Error Inactive(string sku) => Error.Conflict("Product.Inactive", $"Product '{sku}' is inactive and cannot be ordered.");

    public static Error InsufficientStock(string sku, int available, int requested) => Error.Conflict(
        "Product.InsufficientStock",
        $"Not enough stock for '{sku}': {available} available, {requested} requested.");

    public static Error StockBelowReserved(string sku, int reserved) => Error.Conflict(
        "Product.StockBelowReserved",
        $"On-hand stock for '{sku}' cannot drop below the {reserved} unit(s) reserved for open orders.");

    public static Error StockAboveLimit(string sku) => Error.Conflict(
        "Product.StockAboveLimit",
        $"On-hand stock for '{sku}' cannot exceed {Product.MaxStock:N0} units.");
}
