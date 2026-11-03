using StockSync.Application.Abstractions;
using StockSync.Domain.Users;

namespace StockSync.Application.Features.Auth;

public sealed class AuthSessionFactory(IAppDbContext db, ITokenService tokens, TimeProvider clock)
{
    public static UserDto ToDto(User user, string storeName) =>
        new(user.Id, user.Email, user.DisplayName, user.Role.ToString(), user.TenantId, storeName);

    public AuthSession Create(User user, string storeName, RefreshToken? rotating = null)
    {
        var now = clock.GetUtcNow();
        var rawRefreshToken = tokens.CreateRefreshToken();
        var tokenHash = tokens.HashRefreshToken(rawRefreshToken);
        var refreshToken = rotating is null
            ? RefreshToken.Issue(user.Id, tokenHash, now, tokens.RefreshTokenLifetime, tokens.SessionLifetime)
            : rotating.Successor(tokenHash, now, tokens.RefreshTokenLifetime, tokens.SessionLifetime);
        db.RefreshTokens.Add(refreshToken);

        return new AuthSession(
            tokens.CreateAccessToken(user),
            rawRefreshToken,
            refreshToken.Id,
            refreshToken.ExpiresAt,
            ToDto(user, storeName));
    }
}
