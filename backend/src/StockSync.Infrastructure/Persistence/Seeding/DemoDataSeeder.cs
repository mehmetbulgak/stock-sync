using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StockSync.Application.Abstractions;
using StockSync.Application.Features.Orders;
using StockSync.Domain.Channels;
using StockSync.Domain.Common;
using StockSync.Domain.Orders;
using StockSync.Domain.Products;
using StockSync.Domain.Tenants;
using StockSync.Domain.Users;

namespace StockSync.Infrastructure.Persistence.Seeding;

public sealed partial class DemoDataSeeder(AppDbContext db, IPasswordHasher hasher, TimeProvider clock, ILogger<DemoDataSeeder> logger)
{
    private const int SimulatedDays = 21;
    private const int RestockEveryDays = 4;
    private const int MaxBasketAttempts = 5;
    private const double CancellationRate = 0.08;

    private readonly Random _random = new(2026);
    private readonly List<(DateTimeOffset At, Order Order, bool Ship)> _scheduled = [];
    private readonly Dictionary<ChannelType, int> _orderCounters = [];
    private Dictionary<Guid, Product> _productsById = [];
    private int _deliveryNumber = 4400;

    public async Task SeedAsync(CancellationToken ct = default)
    {
        if (await db.Users.IgnoreQueryFilters().AnyAsync(u => u.Email == DemoCatalog.Email, ct))
        {
            return;
        }

        var now = clock.GetUtcNow();
        var start = now.AddDays(-30);

        var tenant = Tenant.Create(DemoCatalog.StoreName, start).Value;
        var user = User.Create(tenant.Id, DemoCatalog.Email, DemoCatalog.OwnerName, UserRole.Owner, start).Value;
        user.SetPasswordHash(hasher.Hash(DemoCatalog.Password));

        var channels = SalesChannel.CreateDefaults(tenant.Id, start);
        var products = DemoCatalog.Products
            .Select(c => Product.Create(tenant.Id, c.Sku, c.Name, null, c.Price, c.Stock, c.Threshold, start).Value)
            .ToList();
        _productsById = products.ToDictionary(p => p.Id);

        var orders = SimulateOrders(channels, products, now);

        db.Tenants.Add(tenant);
        db.Users.Add(user);
        db.SalesChannels.AddRange(channels);
        db.Products.AddRange(products);
        db.Orders.AddRange(orders);
        await db.SaveChangesAsync(ct);

        LogSeeded(logger, products.Count, orders.Count, DemoCatalog.Email);
    }

    private List<Order> SimulateOrders(IReadOnlyList<SalesChannel> channels, List<Product> products, DateTimeOffset now)
    {
        var orders = new List<Order>();

        for (var day = SimulatedDays; day >= 0; day--)
        {
            var windowEnd = now.AddDays(-day);
            var windowStart = windowEnd.AddDays(-1);

            RunScheduledUntil(windowStart);
            if (day % RestockEveryDays == 0)
            {
                Restock(products, windowStart);
            }

            var placementTimes = Enumerable.Range(0, _random.Next(4, 10))
                .Select(_ => windowStart.AddMinutes(_random.Next(1, 24 * 60)))
                .Order();

            foreach (var placedAt in placementTimes)
            {
                RunScheduledUntil(placedAt);
                var order = TryPlaceOrder(channels, products, placedAt);
                if (order is null)
                {
                    continue;
                }

                orders.Add(order);
                _scheduled.Add(_random.NextDouble() < CancellationRate
                    ? (placedAt.AddHours(_random.Next(1, 10)), order, false)
                    : (placedAt.AddHours(_random.Next(6, 40)), order, true));
            }
        }

        RunScheduledUntil(now);
        return orders;
    }

    private Order? TryPlaceOrder(IReadOnlyList<SalesChannel> channels, List<Product> products, DateTimeOffset placedAt)
    {
        var channel = PickChannel(channels);
        _orderCounters[channel.Type] = _orderCounters.GetValueOrDefault(channel.Type, 1000) + _random.Next(1, 40);
        var number = $"{OrderNumberGenerator.Prefix(channel.Type)}-{_orderCounters[channel.Type]}";

        for (var attempt = 0; attempt < MaxBasketAttempts; attempt++)
        {
            var items = Enumerable.Range(0, _random.Next(1, 4))
                .Select(_ => new OrderItem(products[_random.Next(products.Count)], _random.Next(1, 3)))
                .ToList();

            var customer = DemoCatalog.Customers[_random.Next(DemoCatalog.Customers.Count)];
            Result<Order> placed = Order.Place(channel, number, customer, items, placedAt);
            if (placed.IsSuccess)
            {
                return placed.Value;
            }
        }

        return null;
    }

    private void Restock(IEnumerable<Product> products, DateTimeOffset at)
    {
        foreach (var product in products.Where(p => p.IsLowStock && !DemoCatalog.NeverRestocked.Contains(p.Sku)))
        {
            product.AdjustStock(_random.Next(30, 80), $"Supplier delivery #{++_deliveryNumber}", at);
        }
    }

    private void RunScheduledUntil(DateTimeOffset until)
    {
        foreach (var due in _scheduled.Where(s => s.At <= until).OrderBy(s => s.At).ToList())
        {
            if (due.Ship)
            {
                due.Order.Ship(_productsById, due.At);
            }
            else
            {
                due.Order.Cancel(_productsById, "Customer cancelled", due.At);
            }

            _scheduled.Remove(due);
        }
    }

    private SalesChannel PickChannel(IReadOnlyList<SalesChannel> channels)
    {
        var type = _random.NextDouble() switch
        {
            < 0.45 => ChannelType.Trendyol,
            < 0.75 => ChannelType.Hepsiburada,
            < 0.95 => ChannelType.WebStore,
            _ => ChannelType.Manual,
        };

        return channels.First(c => c.Type == type);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Seeded demo store with {Products} products and {Orders} orders for {Email}")]
    private static partial void LogSeeded(ILogger logger, int products, int orders, string email);
}
