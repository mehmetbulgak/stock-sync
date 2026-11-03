using Microsoft.EntityFrameworkCore;
using StockSync.Application.Abstractions;

namespace StockSync.Application.Features.Channels;

public static class ListChannels
{
    public sealed class Handler(IAppDbContext db)
    {
        public async Task<IReadOnlyList<ChannelDto>> HandleAsync(CancellationToken ct) =>
            await db.SalesChannels.AsNoTracking()
                .OrderBy(c => c.Type)
                .Select(c => new ChannelDto(c.Id, c.Name, c.Type.ToString(), c.IsActive))
                .ToListAsync(ct);
    }
}
