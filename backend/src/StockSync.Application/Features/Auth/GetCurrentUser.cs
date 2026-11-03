using Microsoft.EntityFrameworkCore;
using StockSync.Application.Abstractions;
using StockSync.Application.Common;
using StockSync.Domain.Common;

namespace StockSync.Application.Features.Auth;

public static class GetCurrentUser
{
    public sealed class Handler(IAppDbContext db, ICurrentUser currentUser)
    {
        public async Task<Result<UserDto>> HandleAsync(CancellationToken ct)
        {
            var userId = currentUser.GetRequiredUserId();
            var match = await db.Users
                .Where(u => u.Id == userId)
                .Join(db.Tenants, u => u.TenantId, t => t.Id, (u, t) => new { User = u, StoreName = t.Name })
                .SingleOrDefaultAsync(ct);

            return match is null
                ? AuthErrors.UserNotFound
                : AuthSessionFactory.ToDto(match.User, match.StoreName);
        }
    }
}
