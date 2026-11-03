using StockSync.Domain.Orders;

namespace StockSync.Application.Features.Orders;

public sealed record OrderDto(
    Guid Id,
    string Number,
    Guid ChannelId,
    string ChannelName,
    string ChannelType,
    string CustomerName,
    string Status,
    decimal Total,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ShippedAt,
    DateTimeOffset? CancelledAt,
    string? CancellationReason,
    IReadOnlyList<OrderLineDto> Lines)
{
    public static OrderDto From(Order order) => new(
        order.Id,
        order.Number,
        order.ChannelId,
        order.Channel.Name,
        order.Channel.Type.ToString(),
        order.CustomerName,
        order.Status.ToString(),
        order.Total,
        order.CreatedAt,
        order.ShippedAt,
        order.CancelledAt,
        order.CancellationReason,
        [.. order.Lines
            .OrderBy(l => l.Sku, StringComparer.Ordinal)
            .Select(l => new OrderLineDto(l.ProductId, l.Sku, l.ProductName, l.Quantity, l.UnitPrice, l.LineTotal))]);
}
