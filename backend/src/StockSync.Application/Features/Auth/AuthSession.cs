using StockSync.Application.Abstractions;

namespace StockSync.Application.Features.Auth;

public sealed record AuthSession(
    AccessToken AccessToken,
    string RefreshToken,
    Guid RefreshTokenId,
    DateTimeOffset RefreshTokenExpiresAt,
    UserDto User);
