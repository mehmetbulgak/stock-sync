using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockSync.Domain.Products;
using StockSync.Domain.Tenants;

namespace StockSync.Infrastructure.Persistence.Configurations;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products", t =>
        {
            t.HasCheckConstraint("ck_products_stock", "reserved >= 0 AND on_hand >= reserved");
            t.HasCheckConstraint("ck_products_price", "price >= 0");
        });

        builder.Property(p => p.Sku).HasMaxLength(Product.SkuMaxLength);
        builder.Property(p => p.Name).HasMaxLength(Product.NameMaxLength);
        builder.Property(p => p.Description).HasMaxLength(Product.DescriptionMaxLength);
        builder.Property(p => p.Version).IsRowVersion();
        builder.Ignore(p => p.Available);
        builder.Ignore(p => p.IsLowStock);

        builder.HasIndex(p => new { p.TenantId, p.Sku }).IsUnique();
        builder.HasOne<Tenant>().WithMany().HasForeignKey(p => p.TenantId).OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Movements).WithOne().HasForeignKey(m => m.ProductId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(p => p.Movements).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
