using StockSync.Application.Features.Orders;
using StockSync.Application.Features.Products;

namespace StockSync.Application.Features.Dashboard;

public sealed record DashboardDto(
    int ActiveProducts,
    int LowStockProducts,
    int PendingOrders,
    int OrdersToday,
    decimal RevenueLast7Days,
    IReadOnlyList<DailySalesDto> DailySales,
    IReadOnlyList<ChannelSalesDto> SalesByChannel,
    IReadOnlyList<ProductDto> LowStock,
    IReadOnlyList<OrderSummaryDto> RecentOrders);
