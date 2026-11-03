using FluentValidation;
using Microsoft.EntityFrameworkCore;
using StockSync.Application.Abstractions;
using StockSync.Domain.Common;
using StockSync.Domain.Users;

namespace StockSync.Application.Features.Auth;

public static class Login
{
    public sealed record Command(string Email, string Password);

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.Email).NotEmpty().MaximumLength(User.EmailMaxLength);
            RuleFor(c => c.Password).NotEmpty().MaximumLength(PasswordRules.MaxLength);
        }
    }

    public sealed class Handler(
        IAppDbContext db,
        IPasswordHasher hasher,
        ILoginThrottle throttle,
        AuthSessionFactory sessions,
        TimeProvider clock)
    {
        public async Task<Result<AuthSession>> HandleAsync(Command command, CancellationToken ct)
        {
            var email = User.NormalizeEmail(command.Email);
            if (throttle.IsBlocked(email))
            {
                return AuthErrors.TooManyFailedSignIns;
            }

            var match = await db.Users.IgnoreQueryFilters()
                .Where(u => u.Email == email)
                .Join(db.Tenants, u => u.TenantId, t => t.Id, (u, t) => new { User = u, StoreName = t.Name })
                .SingleOrDefaultAsync(ct);

            var check = hasher.Verify(match?.User.PasswordHash, command.Password);
            if (match is null || check == PasswordCheck.Failed)
            {
                throttle.RecordFailure(email);
                return AuthErrors.InvalidCredentials;
            }

            if (check == PasswordCheck.SuccessRehashNeeded)
            {
                match.User.SetPasswordHash(hasher.Hash(command.Password));
            }

            match.User.RecordLogin(clock.GetUtcNow());
            var session = sessions.Create(match.User, match.StoreName);
            await db.SaveChangesAsync(ct);
            return session;
        }
    }
}
