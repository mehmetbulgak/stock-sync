using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockSync.Domain.Orders;
using StockSync.Domain.Tenants;

namespace StockSync.Infrastructure.Persistence.Configurations;

internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders", t => t.HasCheckConstraint("ck_orders_total", "total >= 0"));

        builder.Property(o => o.Number).HasMaxLength(Order.NumberMaxLength);
        builder.Property(o => o.CustomerName).HasMaxLength(Order.CustomerNameMaxLength);
        builder.Property(o => o.CancellationReason).HasMaxLength(Order.CancellationReasonMaxLength);
        builder.Property(o => o.Total).HasPrecision(14, 2);
        builder.Property(o => o.Version).IsRowVersion();

        builder.HasIndex(o => new { o.ChannelId, o.Number }).IsUnique();
        builder.HasIndex(o => new { o.TenantId, o.CreatedAt });
        builder.HasIndex(o => new { o.TenantId, o.Status });

        builder.HasOne(o => o.Channel).WithMany().HasForeignKey(o => o.ChannelId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Tenant>().WithMany().HasForeignKey(o => o.TenantId).OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.Lines).WithOne().HasForeignKey(l => l.OrderId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(o => o.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
