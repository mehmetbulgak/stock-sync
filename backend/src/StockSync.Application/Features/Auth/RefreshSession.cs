using Microsoft.EntityFrameworkCore;
using StockSync.Application.Abstractions;
using StockSync.Domain.Common;

namespace StockSync.Application.Features.Auth;

public static class RefreshSession
{
    public static readonly TimeSpan ConcurrentRotationGracePeriod = TimeSpan.FromSeconds(30);

    public sealed record Command(string? RefreshToken);

    public sealed class Handler(IAppDbContext db, ITokenService tokens, AuthSessionFactory sessions, TimeProvider clock)
    {
        public async Task<Result<AuthSession>> HandleAsync(Command command, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(command.RefreshToken))
            {
                return AuthErrors.InvalidRefreshToken;
            }

            var hash = tokens.HashRefreshToken(command.RefreshToken);
            var stored = await db.RefreshTokens
                .IgnoreQueryFilters()
                .Include(t => t.User)
                .SingleOrDefaultAsync(t => t.TokenHash == hash, ct);

            if (stored is null)
            {
                return AuthErrors.InvalidRefreshToken;
            }

            var now = clock.GetUtcNow();

            if (stored.IsRevoked)
            {
                if (stored.WasRotatedWithin(ConcurrentRotationGracePeriod, now))
                {
                    return AuthErrors.RefreshTokenAlreadyRotated;
                }

                await RevokeAllSessionsAsync(stored.UserId, now, ct);
                return AuthErrors.InvalidRefreshToken;
            }

            if (!stored.IsActive(now))
            {
                return AuthErrors.InvalidRefreshToken;
            }

            var storeName = await db.Tenants
                .Where(t => t.Id == stored.User.TenantId)
                .Select(t => t.Name)
                .SingleAsync(ct);

            var session = sessions.Create(stored.User, storeName, rotating: stored);
            stored.Revoke(now, replacedByTokenId: session.RefreshTokenId);

            try
            {
                await db.SaveChangesAsync(ct);
            }
            catch (DbUpdateConcurrencyException)
            {
                return AuthErrors.RefreshTokenAlreadyRotated;
            }

            return session;
        }

        private async Task RevokeAllSessionsAsync(Guid userId, DateTimeOffset now, CancellationToken ct) =>
            await db.RefreshTokens
                .Where(t => t.UserId == userId && t.RevokedAt == null)
                .ExecuteUpdateAsync(setters => setters.SetProperty(t => t.RevokedAt, now), ct);
    }
}
