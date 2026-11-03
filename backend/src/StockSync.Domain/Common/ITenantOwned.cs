namespace StockSync.Domain.Common;

public interface ITenantOwned
{
    Guid TenantId { get; }
}
