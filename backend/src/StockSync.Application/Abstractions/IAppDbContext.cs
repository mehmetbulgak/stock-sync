using Microsoft.EntityFrameworkCore;
using StockSync.Domain.Channels;
using StockSync.Domain.Orders;
using StockSync.Domain.Products;
using StockSync.Domain.Tenants;
using StockSync.Domain.Users;

namespace StockSync.Application.Abstractions;

public interface IAppDbContext
{
    DbSet<Tenant> Tenants { get; }

    DbSet<User> Users { get; }

    DbSet<RefreshToken> RefreshTokens { get; }

    DbSet<SalesChannel> SalesChannels { get; }

    DbSet<Product> Products { get; }

    DbSet<StockMovement> StockMovements { get; }

    DbSet<Order> Orders { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
