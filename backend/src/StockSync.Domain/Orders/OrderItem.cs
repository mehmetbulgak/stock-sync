using StockSync.Domain.Products;

namespace StockSync.Domain.Orders;

public sealed record OrderItem(Product Product, int Quantity, decimal? UnitPrice = null);
