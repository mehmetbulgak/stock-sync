using FluentValidation;
using Microsoft.EntityFrameworkCore;
using StockSync.Application.Abstractions;
using StockSync.Application.Common;
using StockSync.Domain.Orders;

namespace StockSync.Application.Features.Orders;

public static class ListOrders
{
    public sealed record Query(
        OrderStatus? Status = null,
        Guid? ChannelId = null,
        string? Search = null,
        int? Page = null,
        int? PageSize = null);

    public sealed class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(q => q.Status).IsInEnum();
            RuleFor(q => q.Search).ValidSearch();
            RuleFor(q => q.Page).ValidPage();
            RuleFor(q => q.PageSize).ValidPageSize();
        }
    }

    public sealed class Handler(IAppDbContext db)
    {
        public Task<PagedResult<OrderSummaryDto>> HandleAsync(Query query, CancellationToken ct)
        {
            var orders = db.Orders.AsNoTracking();

            if (query.Status is { } status)
            {
                orders = orders.Where(o => o.Status == status);
            }

            if (query.ChannelId is { } channelId)
            {
                orders = orders.Where(o => o.ChannelId == channelId);
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var term = query.Search.Trim().ToLowerInvariant();
                orders = orders.Where(o => o.Number.ToLower().Contains(term) || o.CustomerName.ToLower().Contains(term));
            }

            return orders
                .OrderByDescending(o => o.CreatedAt)
                .ThenByDescending(o => o.Id)
                .Select(o => new OrderSummaryDto(
                    o.Id,
                    o.Number,
                    o.ChannelId,
                    o.Channel.Name,
                    o.Channel.Type.ToString(),
                    o.CustomerName,
                    o.Status.ToString(),
                    o.Total,
                    o.Lines.Sum(l => l.Quantity),
                    o.CreatedAt))
                .ToPagedResultAsync(query.Page, query.PageSize, ct);
        }
    }
}
