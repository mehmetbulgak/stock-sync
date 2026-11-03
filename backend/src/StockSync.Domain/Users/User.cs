using StockSync.Domain.Common;

namespace StockSync.Domain.Users;

public sealed class User : Entity, ITenantOwned
{
    public const int EmailMaxLength = 254;
    public const int DisplayNameMaxLength = 100;

    private User()
    {
    }

    public Guid TenantId { get; private set; }

    public string Email { get; private set; } = null!;

    public string DisplayName { get; private set; } = null!;

    public string PasswordHash { get; private set; } = null!;

    public UserRole Role { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? LastLoginAt { get; private set; }

    public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    public static Result<User> Create(Guid tenantId, string email, string displayName, UserRole role, DateTimeOffset now)
    {
        email = NormalizeEmail(email);
        displayName = displayName.Trim();

        if (email.Length is 0 or > EmailMaxLength || !email.Contains('@', StringComparison.Ordinal))
        {
            return Error.Validation("User.InvalidEmail", "A valid email address is required.");
        }

        if (displayName.Length is 0 or > DisplayNameMaxLength)
        {
            return Error.Validation("User.InvalidDisplayName", $"Name must be 1-{DisplayNameMaxLength} characters.");
        }

        return new User
        {
            TenantId = tenantId,
            Email = email,
            DisplayName = displayName,
            Role = role,
            CreatedAt = now,
        };
    }

    public void SetPasswordHash(string passwordHash) => PasswordHash = passwordHash;

    public void RecordLogin(DateTimeOffset now) => LastLoginAt = now;
}
