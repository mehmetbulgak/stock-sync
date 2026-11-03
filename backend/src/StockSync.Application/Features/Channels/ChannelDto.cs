namespace StockSync.Application.Features.Channels;

public sealed record ChannelDto(Guid Id, string Name, string Type, bool IsActive);
