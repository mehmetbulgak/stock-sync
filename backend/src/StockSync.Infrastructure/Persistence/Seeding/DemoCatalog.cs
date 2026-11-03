namespace StockSync.Infrastructure.Persistence.Seeding;

internal static class DemoCatalog
{
    public const string StoreName = "Demo Accessories";
    public const string OwnerName = "Demo Seller";
    public const string Email = "demo@stocksync.dev";
    public const string Password = "Demo1234!";

    public static readonly IReadOnlyList<Item> Products =
    [
        new("CASE-IP15-CLR", "iPhone 15 Clear Case", 249.90m, 120, 15),
        new("CASE-IP15-MAG", "iPhone 15 MagSafe Silicone Case", 449.90m, 60, 10),
        new("CASE-S24-BLK", "Galaxy S24 Rugged Case, Black", 329.90m, 45, 10),
        new("GLASS-IP15", "iPhone 15 Tempered Glass (2-pack)", 179.90m, 200, 25),
        new("GLASS-S24", "Galaxy S24 Tempered Glass", 159.90m, 90, 20),
        new("CHG-20W-USBC", "20W USB-C Fast Charger", 389.90m, 80, 15),
        new("CHG-65W-GAN", "65W GaN 3-Port Charger", 1099.90m, 25, 5),
        new("CBL-C2C-1M", "USB-C to USB-C Cable 1m", 149.90m, 150, 20),
        new("CBL-C2L-1M", "USB-C to Lightning Cable 1m", 199.90m, 70, 15),
        new("CBL-C2C-2M-BR", "Braided USB-C Cable 2m", 229.90m, 30, 10),
        new("PB-10K", "Power Bank 10,000 mAh", 799.90m, 40, 8),
        new("PB-20K-PD", "Power Bank 20,000 mAh PD", 1299.90m, 26, 6),
        new("EAR-TWS-PRO", "Wireless Earbuds Pro ANC", 1899.90m, 30, 5),
        new("EAR-TWS-LITE", "Wireless Earbuds Lite", 899.90m, 55, 10),
        new("CAR-MAG-VENT", "Magnetic Car Vent Holder", 279.90m, 65, 10),
        new("CAR-CHG-DUAL", "Dual USB-C Car Charger 45W", 349.90m, 24, 8),
        new("BAND-W45-SPT", "Smartwatch Sport Band 45mm", 199.90m, 85, 10),
        new("STAND-ALU", "Aluminium Phone Stand", 299.90m, 20, 5),
    ];

    public static readonly IReadOnlySet<string> NeverRestocked = new HashSet<string>(StringComparer.Ordinal)
    {
        "CBL-C2C-2M-BR",
        "PB-20K-PD",
        "CAR-CHG-DUAL",
        "STAND-ALU",
    };

    public static readonly IReadOnlyList<string> Customers =
    [
        "Ayşe Yılmaz", "Mehmet Kaya", "Elif Şahin", "Can Demir", "Zeynep Çelik", "Burak Arslan",
        "Selin Aydın", "Emre Koç", "Deniz Öztürk", "Ece Kurt", "Kerem Aksoy", "İrem Polat",
    ];

    internal sealed record Item(string Sku, string Name, decimal Price, int Stock, int Threshold);
}
