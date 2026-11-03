namespace StockSync.Application.Abstractions;

public interface ICurrentUser
{
    bool IsAuthenticated { get; }

    Guid? TenantId { get; }

    Guid? UserId { get; }
}
