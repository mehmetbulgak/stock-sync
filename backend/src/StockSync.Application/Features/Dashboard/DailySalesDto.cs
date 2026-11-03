namespace StockSync.Application.Features.Dashboard;

public sealed record DailySalesDto(DateOnly Date, int Orders, decimal Revenue);
