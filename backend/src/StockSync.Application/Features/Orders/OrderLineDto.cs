namespace StockSync.Application.Features.Orders;

public sealed record OrderLineDto(Guid ProductId, string Sku, string ProductName, int Quantity, decimal UnitPrice, decimal LineTotal);
