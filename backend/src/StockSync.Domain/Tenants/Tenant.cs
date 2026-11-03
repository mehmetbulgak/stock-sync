using StockSync.Domain.Common;

namespace StockSync.Domain.Tenants;

public sealed class Tenant : Entity
{
    public const int NameMaxLength = 120;

    private Tenant()
    {
    }

    public string Name { get; private set; } = null!;

    public DateTimeOffset CreatedAt { get; private set; }

    public static Result<Tenant> Create(string name, DateTimeOffset now)
    {
        name = name.Trim();
        if (name.Length is 0 or > NameMaxLength)
        {
            return Error.Validation("Tenant.InvalidName", $"Store name must be 1-{NameMaxLength} characters.");
        }

        return new Tenant { Name = name, CreatedAt = now };
    }
}
