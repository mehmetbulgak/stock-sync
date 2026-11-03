using StockSync.Domain.Common;

namespace StockSync.Domain.Products;

public sealed class Product : Entity, ITenantOwned
{
    public const int SkuMaxLength = 64;
    public const int NameMaxLength = 200;
    public const int DescriptionMaxLength = 2000;
    public const int MaxStock = 10_000_000;
    public const decimal MaxPrice = 1_000_000m;

    private readonly List<StockMovement> _movements = [];

    private Product()
    {
    }

    public Guid TenantId { get; private set; }

    public string Sku { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public string? Description { get; private set; }

    public decimal Price { get; private set; }

    public int OnHand { get; private set; }

    public int Reserved { get; private set; }

    public int LowStockThreshold { get; private set; }

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public uint Version { get; private set; }

    public int Available => OnHand - Reserved;

    public bool IsLowStock => Available <= LowStockThreshold;

    public IReadOnlyCollection<StockMovement> Movements => _movements;

    public static string NormalizeSku(string sku) => sku.Trim().ToUpperInvariant();

    public static Result<Product> Create(
        Guid tenantId,
        string sku,
        string name,
        string? description,
        decimal price,
        int initialStock,
        int lowStockThreshold,
        DateTimeOffset now)
    {
        sku = NormalizeSku(sku);
        name = name.Trim();

        if (sku.Length == 0)
        {
            return ProductErrors.SkuRequired;
        }

        var details = ValidateDetails(name, price, lowStockThreshold);
        if (details.IsFailure)
        {
            return details.Error!;
        }

        if (initialStock is < 0 or > MaxStock)
        {
            return ProductErrors.InvalidStock;
        }

        var product = new Product
        {
            TenantId = tenantId,
            Sku = sku,
            Name = name,
            Description = NormalizeDescription(description),
            Price = price,
            OnHand = initialStock,
            LowStockThreshold = lowStockThreshold,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
        };

        if (initialStock > 0)
        {
            product.Record(StockMovementType.InitialStock, initialStock, "Initial stock", null, now);
        }

        return product;
    }

    public Result Update(string name, string? description, decimal price, int lowStockThreshold, bool isActive, DateTimeOffset now)
    {
        name = name.Trim();

        var details = ValidateDetails(name, price, lowStockThreshold);
        if (details.IsFailure)
        {
            return details;
        }

        Name = name;
        Description = NormalizeDescription(description);
        Price = price;
        LowStockThreshold = lowStockThreshold;
        IsActive = isActive;
        UpdatedAt = now;
        return Result.Success();
    }

    public Result AdjustStock(int delta, string reason, DateTimeOffset now)
    {
        if (delta == 0)
        {
            return ProductErrors.ZeroAdjustment;
        }

        var newOnHand = (long)OnHand + delta;
        if (newOnHand < Reserved)
        {
            return ProductErrors.StockBelowReserved(Sku, Reserved);
        }

        if (newOnHand > MaxStock)
        {
            return ProductErrors.StockAboveLimit(Sku);
        }

        OnHand = (int)newOnHand;
        UpdatedAt = now;
        Record(StockMovementType.Adjustment, delta, reason.Trim(), null, now);
        return Result.Success();
    }

    internal Result EnsureCanReserve(int quantity)
    {
        if (quantity <= 0)
        {
            return ProductErrors.InvalidQuantity;
        }

        if (!IsActive)
        {
            return ProductErrors.Inactive(Sku);
        }

        return Available < quantity
            ? ProductErrors.InsufficientStock(Sku, Available, quantity)
            : Result.Success();
    }

    internal Result Reserve(int quantity, Guid orderId, DateTimeOffset now)
    {
        var check = EnsureCanReserve(quantity);
        if (check.IsFailure)
        {
            return check;
        }

        Reserved += quantity;
        UpdatedAt = now;
        Record(StockMovementType.Reservation, quantity, null, orderId, now);
        return Result.Success();
    }

    internal void ReleaseReservation(int quantity, Guid orderId, DateTimeOffset now)
    {
        var released = Math.Min(quantity, Reserved);
        Reserved -= released;
        UpdatedAt = now;
        Record(StockMovementType.ReservationReleased, -released, null, orderId, now);
    }

    internal void CommitReservation(int quantity, Guid orderId, DateTimeOffset now)
    {
        var committed = Math.Min(quantity, Reserved);
        Reserved -= committed;
        OnHand -= committed;
        UpdatedAt = now;
        Record(StockMovementType.Shipment, -committed, null, orderId, now);
    }

    private static Result ValidateDetails(string name, decimal price, int lowStockThreshold)
    {
        if (name.Length is 0 or > NameMaxLength)
        {
            return ProductErrors.InvalidName;
        }

        if (price is < 0 or > MaxPrice)
        {
            return ProductErrors.InvalidPrice;
        }

        return lowStockThreshold is < 0 or > MaxStock
            ? ProductErrors.InvalidThreshold
            : Result.Success();
    }

    private static string? NormalizeDescription(string? description) =>
        string.IsNullOrWhiteSpace(description) ? null : description.Trim();

    private void Record(StockMovementType type, int quantity, string? note, Guid? orderId, DateTimeOffset now) =>
        _movements.Add(StockMovement.Record(this, type, quantity, note, orderId, now));
}
