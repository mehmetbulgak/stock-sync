using Microsoft.IdentityModel.JsonWebTokens;

namespace StockSync.Infrastructure.Auth;

public static class AppClaims
{
    public const string UserId = JwtRegisteredClaimNames.Sub;
    public const string TenantId = "tenant_id";
    public const string Role = "role";
    public const string TokenId = JwtRegisteredClaimNames.Jti;
}
