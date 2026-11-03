namespace StockSync.Application.Features.Orders;

public sealed record OrderSummaryDto(
    Guid Id,
    string Number,
    Guid ChannelId,
    string ChannelName,
    string ChannelType,
    string CustomerName,
    string Status,
    decimal Total,
    int ItemCount,
    DateTimeOffset CreatedAt);
