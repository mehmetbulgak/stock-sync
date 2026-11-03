using StockSync.Application.Abstractions;

namespace StockSync.Application.Common;

public static class CurrentUserExtensions
{
    public static Guid GetRequiredTenantId(this ICurrentUser user) =>
        user.TenantId ?? throw new UnauthorizedAccessException("No tenant in the current context.");

    public static Guid GetRequiredUserId(this ICurrentUser user) =>
        user.UserId ?? throw new UnauthorizedAccessException("No user in the current context.");
}
