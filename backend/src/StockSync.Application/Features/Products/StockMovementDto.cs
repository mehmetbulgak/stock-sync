namespace StockSync.Application.Features.Products;

public sealed record StockMovementDto(
    Guid Id,
    string Type,
    int Quantity,
    int OnHandAfter,
    int ReservedAfter,
    string? Note,
    Guid? OrderId,
    DateTimeOffset CreatedAt);
