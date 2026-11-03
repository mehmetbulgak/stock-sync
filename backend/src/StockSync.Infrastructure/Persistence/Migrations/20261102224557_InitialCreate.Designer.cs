using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using StockSync.Infrastructure.Persistence;

#nullable disable

namespace StockSync.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20261102224557_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "10.0.14")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("StockSync.Domain.Channels.SalesChannel", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<bool>("IsActive")
                        .HasColumnType("boolean")
                        .HasColumnName("is_active");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(60)
                        .HasColumnType("character varying(60)")
                        .HasColumnName("name");

                    b.Property<Guid>("TenantId")
                        .HasColumnType("uuid")
                        .HasColumnName("tenant_id");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)")
                        .HasColumnName("type");

                    b.HasKey("Id")
                        .HasName("pk_sales_channels");

                    b.HasIndex("TenantId", "Name")
                        .IsUnique()
                        .HasDatabaseName("ix_sales_channels_tenant_id_name");

                    b.ToTable("sales_channels", (string)null);
                });

            modelBuilder.Entity("StockSync.Domain.Orders.Order", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("CancellationReason")
                        .HasMaxLength(300)
                        .HasColumnType("character varying(300)")
                        .HasColumnName("cancellation_reason");

                    b.Property<DateTimeOffset?>("CancelledAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("cancelled_at");

                    b.Property<Guid>("ChannelId")
                        .HasColumnType("uuid")
                        .HasColumnName("channel_id");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<string>("CustomerName")
                        .IsRequired()
                        .HasMaxLength(150)
                        .HasColumnType("character varying(150)")
                        .HasColumnName("customer_name");

                    b.Property<string>("Number")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)")
                        .HasColumnName("number");

                    b.Property<DateTimeOffset?>("ShippedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("shipped_at");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)")
                        .HasColumnName("status");

                    b.Property<Guid>("TenantId")
                        .HasColumnType("uuid")
                        .HasColumnName("tenant_id");

                    b.Property<decimal>("Total")
                        .HasPrecision(14, 2)
                        .HasColumnType("numeric(14,2)")
                        .HasColumnName("total");

                    b.Property<uint>("Version")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("xid")
                        .HasColumnName("xmin");

                    b.HasKey("Id")
                        .HasName("pk_orders");

                    b.HasIndex("ChannelId", "Number")
                        .IsUnique()
                        .HasDatabaseName("ix_orders_channel_id_number");

                    b.HasIndex("TenantId", "CreatedAt")
                        .HasDatabaseName("ix_orders_tenant_id_created_at");

                    b.HasIndex("TenantId", "Status")
                        .HasDatabaseName("ix_orders_tenant_id_status");

                    b.ToTable("orders", null, t =>
                        {
                            t.HasCheckConstraint("ck_orders_total", "total >= 0");
                        });
                });

            modelBuilder.Entity("StockSync.Domain.Orders.OrderLine", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<decimal>("LineTotal")
                        .HasPrecision(14, 2)
                        .HasColumnType("numeric(14,2)")
                        .HasColumnName("line_total");

                    b.Property<Guid>("OrderId")
                        .HasColumnType("uuid")
                        .HasColumnName("order_id");

                    b.Property<Guid>("ProductId")
                        .HasColumnType("uuid")
                        .HasColumnName("product_id");

                    b.Property<string>("ProductName")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)")
                        .HasColumnName("product_name");

                    b.Property<int>("Quantity")
                        .HasColumnType("integer")
                        .HasColumnName("quantity");

                    b.Property<string>("Sku")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)")
                        .HasColumnName("sku");

                    b.Property<decimal>("UnitPrice")
                        .HasPrecision(12, 2)
                        .HasColumnType("numeric(12,2)")
                        .HasColumnName("unit_price");

                    b.HasKey("Id")
                        .HasName("pk_order_lines");

                    b.HasIndex("OrderId")
                        .HasDatabaseName("ix_order_lines_order_id");

                    b.HasIndex("ProductId")
                        .HasDatabaseName("ix_order_lines_product_id");

                    b.ToTable("order_lines", null, t =>
                        {
                            t.HasCheckConstraint("ck_order_lines_quantity", "quantity > 0");
                        });
                });

            modelBuilder.Entity("StockSync.Domain.Products.Product", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<string>("Description")
                        .HasMaxLength(2000)
                        .HasColumnType("character varying(2000)")
                        .HasColumnName("description");

                    b.Property<bool>("IsActive")
                        .HasColumnType("boolean")
                        .HasColumnName("is_active");

                    b.Property<int>("LowStockThreshold")
                        .HasColumnType("integer")
                        .HasColumnName("low_stock_threshold");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)")
                        .HasColumnName("name");

                    b.Property<int>("OnHand")
                        .HasColumnType("integer")
                        .HasColumnName("on_hand");

                    b.Property<decimal>("Price")
                        .HasPrecision(12, 2)
                        .HasColumnType("numeric(12,2)")
                        .HasColumnName("price");

                    b.Property<int>("Reserved")
                        .HasColumnType("integer")
                        .HasColumnName("reserved");

                    b.Property<string>("Sku")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)")
                        .HasColumnName("sku");

                    b.Property<Guid>("TenantId")
                        .HasColumnType("uuid")
                        .HasColumnName("tenant_id");

                    b.Property<DateTimeOffset>("UpdatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("updated_at");

                    b.Property<uint>("Version")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("xid")
                        .HasColumnName("xmin");

                    b.HasKey("Id")
                        .HasName("pk_products");

                    b.HasIndex("TenantId", "Sku")
                        .IsUnique()
                        .HasDatabaseName("ix_products_tenant_id_sku");

                    b.ToTable("products", null, t =>
                        {
                            t.HasCheckConstraint("ck_products_price", "price >= 0");

                            t.HasCheckConstraint("ck_products_stock", "reserved >= 0 AND on_hand >= reserved");
                        });
                });

            modelBuilder.Entity("StockSync.Domain.Products.StockMovement", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<string>("Note")
                        .HasMaxLength(300)
                        .HasColumnType("character varying(300)")
                        .HasColumnName("note");

                    b.Property<int>("OnHandAfter")
                        .HasColumnType("integer")
                        .HasColumnName("on_hand_after");

                    b.Property<Guid?>("OrderId")
                        .HasColumnType("uuid")
                        .HasColumnName("order_id");

                    b.Property<Guid>("ProductId")
                        .HasColumnType("uuid")
                        .HasColumnName("product_id");

                    b.Property<int>("Quantity")
                        .HasColumnType("integer")
                        .HasColumnName("quantity");

                    b.Property<int>("ReservedAfter")
                        .HasColumnType("integer")
                        .HasColumnName("reserved_after");

                    b.Property<Guid>("TenantId")
                        .HasColumnType("uuid")
                        .HasColumnName("tenant_id");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)")
                        .HasColumnName("type");

                    b.HasKey("Id")
                        .HasName("pk_stock_movements");

                    b.HasIndex("OrderId")
                        .HasDatabaseName("ix_stock_movements_order_id");

                    b.HasIndex("ProductId", "CreatedAt")
                        .HasDatabaseName("ix_stock_movements_product_id_created_at");

                    b.ToTable("stock_movements", (string)null);
                });

            modelBuilder.Entity("StockSync.Domain.Tenants.Tenant", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(120)
                        .HasColumnType("character varying(120)")
                        .HasColumnName("name");

                    b.HasKey("Id")
                        .HasName("pk_tenants");

                    b.ToTable("tenants", (string)null);
                });

            modelBuilder.Entity("StockSync.Domain.Users.RefreshToken", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<DateTimeOffset>("ExpiresAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("expires_at");

                    b.Property<Guid?>("ReplacedByTokenId")
                        .HasColumnType("uuid")
                        .HasColumnName("replaced_by_token_id");

                    b.Property<DateTimeOffset?>("RevokedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("revoked_at");

                    b.Property<string>("TokenHash")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)")
                        .HasColumnName("token_hash");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid")
                        .HasColumnName("user_id");

                    b.Property<uint>("Version")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("xid")
                        .HasColumnName("xmin");

                    b.HasKey("Id")
                        .HasName("pk_refresh_tokens");

                    b.HasIndex("TokenHash")
                        .IsUnique()
                        .HasDatabaseName("ix_refresh_tokens_token_hash");

                    b.HasIndex("UserId")
                        .HasDatabaseName("ix_refresh_tokens_user_id");

                    b.ToTable("refresh_tokens", (string)null);
                });

            modelBuilder.Entity("StockSync.Domain.Users.User", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<string>("DisplayName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("display_name");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(254)
                        .HasColumnType("character varying(254)")
                        .HasColumnName("email");

                    b.Property<DateTimeOffset?>("LastLoginAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("last_login_at");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasMaxLength(512)
                        .HasColumnType("character varying(512)")
                        .HasColumnName("password_hash");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)")
                        .HasColumnName("role");

                    b.Property<Guid>("TenantId")
                        .HasColumnType("uuid")
                        .HasColumnName("tenant_id");

                    b.HasKey("Id")
                        .HasName("pk_users");

                    b.HasIndex("Email")
                        .IsUnique()
                        .HasDatabaseName("ix_users_email");

                    b.HasIndex("TenantId")
                        .HasDatabaseName("ix_users_tenant_id");

                    b.ToTable("users", (string)null);
                });

            modelBuilder.Entity("StockSync.Domain.Channels.SalesChannel", b =>
                {
                    b.HasOne("StockSync.Domain.Tenants.Tenant", null)
                        .WithMany()
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_sales_channels_tenants_tenant_id");
                });

            modelBuilder.Entity("StockSync.Domain.Orders.Order", b =>
                {
                    b.HasOne("StockSync.Domain.Channels.SalesChannel", "Channel")
                        .WithMany()
                        .HasForeignKey("ChannelId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("fk_orders_sales_channels_channel_id");

                    b.HasOne("StockSync.Domain.Tenants.Tenant", null)
                        .WithMany()
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_orders_tenants_tenant_id");

                    b.Navigation("Channel");
                });

            modelBuilder.Entity("StockSync.Domain.Orders.OrderLine", b =>
                {
                    b.HasOne("StockSync.Domain.Orders.Order", null)
                        .WithMany("Lines")
                        .HasForeignKey("OrderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_order_lines_orders_order_id");
                });

            modelBuilder.Entity("StockSync.Domain.Products.Product", b =>
                {
                    b.HasOne("StockSync.Domain.Tenants.Tenant", null)
                        .WithMany()
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_products_tenants_tenant_id");
                });

            modelBuilder.Entity("StockSync.Domain.Products.StockMovement", b =>
                {
                    b.HasOne("StockSync.Domain.Products.Product", null)
                        .WithMany("Movements")
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_stock_movements_products_product_id");
                });

            modelBuilder.Entity("StockSync.Domain.Users.RefreshToken", b =>
                {
                    b.HasOne("StockSync.Domain.Users.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_refresh_tokens_users_user_id");

                    b.Navigation("User");
                });

            modelBuilder.Entity("StockSync.Domain.Users.User", b =>
                {
                    b.HasOne("StockSync.Domain.Tenants.Tenant", null)
                        .WithMany()
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_users_tenants_tenant_id");
                });

            modelBuilder.Entity("StockSync.Domain.Orders.Order", b =>
                {
                    b.Navigation("Lines");
                });

            modelBuilder.Entity("StockSync.Domain.Products.Product", b =>
                {
                    b.Navigation("Movements");
                });
#pragma warning restore 612, 618
        }
    }
}
