using StockSync.Domain.Channels;
using StockSync.Domain.Common;
using StockSync.Domain.Products;

namespace StockSync.Domain.Orders;

public sealed class Order : Entity, ITenantOwned
{
    public const int NumberMaxLength = 64;
    public const int CustomerNameMaxLength = 150;
    public const int CancellationReasonMaxLength = 300;
    public const int MaxLines = 50;
    public const int MaxQuantityPerLine = 10_000;

    private readonly List<OrderLine> _lines = [];

    private Order()
    {
    }

    public Guid TenantId { get; private set; }

    public Guid ChannelId { get; private set; }

    public SalesChannel Channel { get; private set; } = null!;

    public string Number { get; private set; } = null!;

    public string CustomerName { get; private set; } = null!;

    public OrderStatus Status { get; private set; }

    public decimal Total { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? ShippedAt { get; private set; }

    public DateTimeOffset? CancelledAt { get; private set; }

    public string? CancellationReason { get; private set; }

    public uint Version { get; private set; }

    public IReadOnlyCollection<OrderLine> Lines => _lines;

    public static Result<Order> Place(
        SalesChannel channel,
        string number,
        string customerName,
        IReadOnlyCollection<OrderItem> items,
        DateTimeOffset now)
    {
        number = number.Trim();
        customerName = customerName.Trim();

        var header = ValidateHeader(channel, number, customerName, items);
        if (header.IsFailure)
        {
            return header.Error!;
        }

        var merged = items
            .GroupBy(i => i.Product.Id)
            .Select(g => new OrderItem(g.First().Product, g.Sum(i => i.Quantity), g.First().UnitPrice))
            .ToList();

        foreach (var item in merged)
        {
            var check = ValidateItem(channel, item);
            if (check.IsFailure)
            {
                return check.Error!;
            }
        }

        var order = new Order
        {
            TenantId = channel.TenantId,
            ChannelId = channel.Id,
            Channel = channel,
            Number = number,
            CustomerName = customerName,
            Status = OrderStatus.Pending,
            CreatedAt = now,
        };

        foreach (var item in merged)
        {
            var reserved = item.Product.Reserve(item.Quantity, order.Id, now);
            if (reserved.IsFailure)
            {
                throw new InvalidOperationException(reserved.Error!.Message);
            }

            var unitPrice = item.UnitPrice ?? item.Product.Price;
            order._lines.Add(OrderLine.Create(order.Id, item.Product.Id, item.Product.Sku, item.Product.Name, item.Quantity, unitPrice));
        }

        order.Total = order._lines.Sum(l => l.LineTotal);
        return order;
    }

    public Result Ship(IReadOnlyDictionary<Guid, Product> products, DateTimeOffset now)
    {
        var allowed = EnsureTransition(products, "ship");
        if (allowed.IsFailure)
        {
            return allowed;
        }

        foreach (var line in _lines)
        {
            products[line.ProductId].CommitReservation(line.Quantity, Id, now);
        }

        Status = OrderStatus.Shipped;
        ShippedAt = now;
        return Result.Success();
    }

    public Result Cancel(IReadOnlyDictionary<Guid, Product> products, string? reason, DateTimeOffset now)
    {
        var allowed = EnsureTransition(products, "cancel");
        if (allowed.IsFailure)
        {
            return allowed;
        }

        foreach (var line in _lines)
        {
            products[line.ProductId].ReleaseReservation(line.Quantity, Id, now);
        }

        Status = OrderStatus.Cancelled;
        CancelledAt = now;
        CancellationReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        return Result.Success();
    }

    private static Result ValidateHeader(SalesChannel channel, string number, string customerName, IReadOnlyCollection<OrderItem> items)
    {
        if (!channel.IsActive)
        {
            return OrderErrors.ChannelInactive;
        }

        if (number.Length is 0 or > NumberMaxLength)
        {
            return OrderErrors.NumberRequired;
        }

        if (customerName.Length is 0 or > CustomerNameMaxLength)
        {
            return OrderErrors.CustomerRequired;
        }

        if (items.Count == 0)
        {
            return OrderErrors.NoItems;
        }

        return items.Count > MaxLines ? OrderErrors.TooManyItems : Result.Success();
    }

    private static Result ValidateItem(SalesChannel channel, OrderItem item)
    {
        if (item.Product.TenantId != channel.TenantId)
        {
            return OrderErrors.ProductMissing;
        }

        if (item.Quantity is < 1 or > MaxQuantityPerLine)
        {
            return OrderErrors.InvalidQuantity;
        }

        if (item.UnitPrice is < 0 or > Product.MaxPrice)
        {
            return OrderErrors.InvalidUnitPrice;
        }

        return item.Product.EnsureCanReserve(item.Quantity);
    }

    private Result EnsureTransition(IReadOnlyDictionary<Guid, Product> products, string action)
    {
        if (Status != OrderStatus.Pending)
        {
            return OrderErrors.InvalidTransition(Status, action);
        }

        return _lines.TrueForAll(l => products.ContainsKey(l.ProductId))
            ? Result.Success()
            : OrderErrors.ProductMissing;
    }
}
