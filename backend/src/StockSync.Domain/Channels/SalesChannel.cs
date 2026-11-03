using StockSync.Domain.Common;

namespace StockSync.Domain.Channels;

public sealed class SalesChannel : Entity, ITenantOwned
{
    public const int NameMaxLength = 60;

    private SalesChannel()
    {
    }

    public Guid TenantId { get; private set; }

    public string Name { get; private set; } = null!;

    public ChannelType Type { get; private set; }

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public static SalesChannel Create(Guid tenantId, string name, ChannelType type, DateTimeOffset now) => new()
    {
        TenantId = tenantId,
        Name = name.Trim(),
        Type = type,
        IsActive = true,
        CreatedAt = now,
    };

    public static IReadOnlyList<SalesChannel> CreateDefaults(Guid tenantId, DateTimeOffset now) =>
    [
        Create(tenantId, "Trendyol", ChannelType.Trendyol, now),
        Create(tenantId, "Hepsiburada", ChannelType.Hepsiburada, now),
        Create(tenantId, "Web Store", ChannelType.WebStore, now),
        Create(tenantId, "Manual", ChannelType.Manual, now),
    ];

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;
}
