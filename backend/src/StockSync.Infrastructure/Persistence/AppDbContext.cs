using Microsoft.EntityFrameworkCore;
using StockSync.Application.Abstractions;
using StockSync.Domain.Channels;
using StockSync.Domain.Common;
using StockSync.Domain.Orders;
using StockSync.Domain.Products;
using StockSync.Domain.Tenants;
using StockSync.Domain.Users;

namespace StockSync.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUser currentUser)
    : DbContext(options), IAppDbContext
{
    public DbSet<Tenant> Tenants => Set<Tenant>();

    public DbSet<User> Users => Set<User>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<SalesChannel> SalesChannels => Set<SalesChannel>();

    public DbSet<Product> Products => Set<Product>();

    public DbSet<StockMovement> StockMovements => Set<StockMovement>();

    public DbSet<Order> Orders => Set<Order>();

    private Guid? CurrentTenantId => currentUser.TenantId;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        foreach (var entity in modelBuilder.Model.GetEntityTypes().Where(e => typeof(Entity).IsAssignableFrom(e.ClrType)))
        {
            modelBuilder.Entity(entity.ClrType).Property(nameof(Entity.Id)).ValueGeneratedNever();
        }

        modelBuilder.Entity<User>().HasQueryFilter(e => e.TenantId == CurrentTenantId);
        modelBuilder.Entity<SalesChannel>().HasQueryFilter(e => e.TenantId == CurrentTenantId);
        modelBuilder.Entity<Product>().HasQueryFilter(e => e.TenantId == CurrentTenantId);
        modelBuilder.Entity<StockMovement>().HasQueryFilter(e => e.TenantId == CurrentTenantId);
        modelBuilder.Entity<Order>().HasQueryFilter(e => e.TenantId == CurrentTenantId);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<Enum>().HaveConversion<string>().HaveMaxLength(32);
        configurationBuilder.Properties<decimal>().HavePrecision(12, 2);
    }
}
