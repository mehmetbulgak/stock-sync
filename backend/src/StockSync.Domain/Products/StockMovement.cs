using StockSync.Domain.Common;

namespace StockSync.Domain.Products;

public sealed class StockMovement : Entity, ITenantOwned
{
    public const int NoteMaxLength = 300;

    private StockMovement()
    {
    }

    public Guid TenantId { get; private set; }

    public Guid ProductId { get; private set; }

    public StockMovementType Type { get; private set; }

    public int Quantity { get; private set; }

    public int OnHandAfter { get; private set; }

    public int ReservedAfter { get; private set; }

    public string? Note { get; private set; }

    public Guid? OrderId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    internal static StockMovement Record(Product product, StockMovementType type, int quantity, string? note, Guid? orderId, DateTimeOffset now) => new()
    {
        TenantId = product.TenantId,
        ProductId = product.Id,
        Type = type,
        Quantity = quantity,
        OnHandAfter = product.OnHand,
        ReservedAfter = product.Reserved,
        Note = note,
        OrderId = orderId,
        CreatedAt = now,
    };
}
