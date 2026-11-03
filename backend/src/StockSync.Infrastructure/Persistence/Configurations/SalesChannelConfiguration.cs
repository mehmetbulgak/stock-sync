using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockSync.Domain.Channels;
using StockSync.Domain.Tenants;

namespace StockSync.Infrastructure.Persistence.Configurations;

internal sealed class SalesChannelConfiguration : IEntityTypeConfiguration<SalesChannel>
{
    public void Configure(EntityTypeBuilder<SalesChannel> builder)
    {
        builder.ToTable("sales_channels");
        builder.Property(c => c.Name).HasMaxLength(SalesChannel.NameMaxLength);
        builder.HasIndex(c => new { c.TenantId, c.Name }).IsUnique();
        builder.HasOne<Tenant>().WithMany().HasForeignKey(c => c.TenantId).OnDelete(DeleteBehavior.Cascade);
    }
}
