using StockSync.Domain.Users;

namespace StockSync.Application.Abstractions;

public interface ITokenService
{
    TimeSpan RefreshTokenLifetime { get; }

    TimeSpan SessionLifetime { get; }

    AccessToken CreateAccessToken(User user);

    string CreateRefreshToken();

    string HashRefreshToken(string token);
}
