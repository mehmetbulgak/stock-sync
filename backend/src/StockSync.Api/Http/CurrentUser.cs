using System.Security.Claims;
using StockSync.Application.Abstractions;
using StockSync.Infrastructure.Auth;

namespace StockSync.Api.Http;

internal sealed class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => accessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    public Guid? TenantId => ReadGuid(AppClaims.TenantId);

    public Guid? UserId => ReadGuid(AppClaims.UserId);

    private Guid? ReadGuid(string claimType) =>
        IsAuthenticated && Guid.TryParse(Principal!.FindFirstValue(claimType), out var id) ? id : null;
}
