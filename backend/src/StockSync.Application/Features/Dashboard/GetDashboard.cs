using Microsoft.EntityFrameworkCore;
using StockSync.Application.Abstractions;
using StockSync.Application.Common;
using StockSync.Application.Features.Orders;
using StockSync.Application.Features.Products;
using StockSync.Domain.Channels;
using StockSync.Domain.Orders;

namespace StockSync.Application.Features.Dashboard;

public static class GetDashboard
{
    public const int Days = 7;
    public const int ListSize = 5;

    public sealed class Handler(IAppDbContext db, TimeProvider clock, ListOrders.Handler listOrders)
    {
        public async Task<DashboardDto> HandleAsync(CancellationToken ct)
        {
            var today = BusinessTimeZone.Today(clock.GetUtcNow());
            var firstDay = today.AddDays(-(Days - 1));

            var products = db.Products.AsNoTracking().Where(p => p.IsActive);
            var activeProducts = await products.CountAsync(ct);
            var lowStockProducts = await products.CountAsync(p => p.OnHand - p.Reserved <= p.LowStockThreshold, ct);
            var pendingOrders = await db.Orders.CountAsync(o => o.Status == OrderStatus.Pending, ct);
            var sales = await LoadSalesAsync(BusinessTimeZone.StartOfDayUtc(firstDay), ct);
            var dailySales = SummarizeByDay(sales, firstDay);

            var lowStock = await products
                .Where(p => p.OnHand - p.Reserved <= p.LowStockThreshold)
                .OrderBy(p => p.OnHand - p.Reserved)
                .ThenBy(p => p.Name)
                .Take(ListSize)
                .Select(ProductDto.Projection)
                .ToListAsync(ct);

            var recentOrders = await listOrders.HandleAsync(new ListOrders.Query(PageSize: ListSize), ct);

            return new DashboardDto(
                activeProducts,
                lowStockProducts,
                pendingOrders,
                dailySales[^1].Orders,
                dailySales.Sum(d => d.Revenue),
                dailySales,
                SummarizeByChannel(sales),
                lowStock,
                recentOrders.Items);
        }

        private static List<DailySalesDto> SummarizeByDay(IReadOnlyList<Sale> sales, DateOnly firstDay)
        {
            var byDay = sales
                .GroupBy(s => BusinessTimeZone.ToLocalDate(s.CreatedAt))
                .ToDictionary(g => g.Key, g => (Orders: g.Count(), Revenue: g.Sum(s => s.Total)));

            return [.. Enumerable.Range(0, Days)
                .Select(firstDay.AddDays)
                .Select(day => byDay.TryGetValue(day, out var s)
                    ? new DailySalesDto(day, s.Orders, s.Revenue)
                    : new DailySalesDto(day, 0, 0))];
        }

        private static List<ChannelSalesDto> SummarizeByChannel(IReadOnlyList<Sale> sales) =>
            [.. sales
                .GroupBy(s => new { s.ChannelName, s.ChannelType })
                .Select(g => new ChannelSalesDto(g.Key.ChannelName, g.Key.ChannelType.ToString(), g.Count(), g.Sum(s => s.Total)))
                .OrderByDescending(c => c.Revenue)];

        private async Task<List<Sale>> LoadSalesAsync(DateTimeOffset since, CancellationToken ct) =>
            await db.Orders.AsNoTracking()
                .Where(o => o.CreatedAt >= since && o.Status != OrderStatus.Cancelled)
                .Select(o => new Sale(o.CreatedAt, o.Total, o.Channel.Name, o.Channel.Type))
                .ToListAsync(ct);
    }

    private sealed record Sale(DateTimeOffset CreatedAt, decimal Total, string ChannelName, ChannelType ChannelType);
}
