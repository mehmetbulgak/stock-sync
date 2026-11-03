using StockSync.Domain.Common;

namespace StockSync.Domain.Orders;

public static class OrderErrors
{
    public static readonly Error NoItems = Error.Validation("Order.NoItems", "An order needs at least one item.");

    public static readonly Error TooManyItems = Error.Validation("Order.TooManyItems", $"An order can have at most {Order.MaxLines} lines.");

    public static readonly Error InvalidQuantity = Error.Validation(
        "Order.InvalidQuantity",
        $"Each line needs a quantity between 1 and {Order.MaxQuantityPerLine:N0}.");

    public static readonly Error InvalidUnitPrice = Error.Validation("Order.InvalidUnitPrice", "Unit price is out of range.");

    public static readonly Error NumberRequired = Error.Validation("Order.NumberRequired", "Order number is required.");

    public static readonly Error CustomerRequired = Error.Validation("Order.CustomerRequired", "Customer name is required.");

    public static readonly Error ChannelInactive = Error.Conflict("Order.ChannelInactive", "The sales channel is inactive.");

    public static readonly Error ProductMissing = Error.Validation("Order.ProductMissing", "Every order line must reference an existing product.");

    public static Error NotFound(Guid id) => Error.NotFound("Order.NotFound", $"Order '{id}' was not found.");

    public static Error Duplicate(string number) => Error.Conflict("Order.Duplicate", $"Order '{number}' already exists on this channel.");

    public static Error InvalidTransition(OrderStatus from, string action) => Error.Conflict(
        "Order.InvalidStatusTransition",
        $"Cannot {action} an order that is already {from}.");
}
