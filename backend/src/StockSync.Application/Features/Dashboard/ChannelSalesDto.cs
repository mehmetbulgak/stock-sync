namespace StockSync.Application.Features.Dashboard;

public sealed record ChannelSalesDto(string ChannelName, string ChannelType, int Orders, decimal Revenue);
