using FluentValidation;
using Microsoft.EntityFrameworkCore;
using StockSync.Application.Abstractions;
using StockSync.Domain.Channels;
using StockSync.Domain.Common;
using StockSync.Domain.Tenants;
using StockSync.Domain.Users;

namespace StockSync.Application.Features.Auth;

public static class Register
{
    public sealed record Command(string StoreName, string DisplayName, string Email, string Password);

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.StoreName).NotEmpty().MaximumLength(Tenant.NameMaxLength);
            RuleFor(c => c.DisplayName).NotEmpty().MaximumLength(User.DisplayNameMaxLength);
            RuleFor(c => c.Email).NotEmpty().EmailAddress().MaximumLength(User.EmailMaxLength);
            RuleFor(c => c.Password).ValidPassword();
        }
    }

    public sealed class Handler(IAppDbContext db, IPasswordHasher hasher, AuthSessionFactory sessions, TimeProvider clock)
    {
        public async Task<Result<AuthSession>> HandleAsync(Command command, CancellationToken ct)
        {
            var email = User.NormalizeEmail(command.Email);
            if (await db.Users.IgnoreQueryFilters().AnyAsync(u => u.Email == email, ct))
            {
                return AuthErrors.EmailTaken;
            }

            var now = clock.GetUtcNow();

            var tenant = Tenant.Create(command.StoreName, now);
            if (tenant.IsFailure)
            {
                return tenant.Error!;
            }

            var user = User.Create(tenant.Value.Id, email, command.DisplayName, UserRole.Owner, now);
            if (user.IsFailure)
            {
                return user.Error!;
            }

            user.Value.SetPasswordHash(hasher.Hash(command.Password));
            user.Value.RecordLogin(now);

            db.Tenants.Add(tenant.Value);
            db.Users.Add(user.Value);
            db.SalesChannels.AddRange(SalesChannel.CreateDefaults(tenant.Value.Id, now));

            var session = sessions.Create(user.Value, tenant.Value.Name);
            await db.SaveChangesAsync(ct);
            return session;
        }
    }
}
