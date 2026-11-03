using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockSync.Domain.Orders;
using StockSync.Domain.Products;

namespace StockSync.Infrastructure.Persistence.Configurations;

internal sealed class OrderLineConfiguration : IEntityTypeConfiguration<OrderLine>
{
    public void Configure(EntityTypeBuilder<OrderLine> builder)
    {
        builder.ToTable("order_lines", t => t.HasCheckConstraint("ck_order_lines_quantity", "quantity > 0"));

        builder.Property(l => l.Sku).HasMaxLength(Product.SkuMaxLength);
        builder.Property(l => l.ProductName).HasMaxLength(Product.NameMaxLength);
        builder.Property(l => l.LineTotal).HasPrecision(14, 2);
        builder.HasIndex(l => l.ProductId);
    }
}
