using StockSync.Domain.Common;

namespace StockSync.Domain.Orders;

public sealed class OrderLine : Entity
{
    private OrderLine()
    {
    }

    public Guid OrderId { get; private set; }

    public Guid ProductId { get; private set; }

    public string Sku { get; private set; } = null!;

    public string ProductName { get; private set; } = null!;

    public int Quantity { get; private set; }

    public decimal UnitPrice { get; private set; }

    public decimal LineTotal { get; private set; }

    internal static OrderLine Create(Guid orderId, Guid productId, string sku, string productName, int quantity, decimal unitPrice) => new()
    {
        OrderId = orderId,
        ProductId = productId,
        Sku = sku,
        ProductName = productName,
        Quantity = quantity,
        UnitPrice = unitPrice,
        LineTotal = quantity * unitPrice,
    };
}
