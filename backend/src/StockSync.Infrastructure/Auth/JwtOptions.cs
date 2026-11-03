using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace StockSync.Infrastructure.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    public const int MinSigningKeyLength = 32;

    [Required]
    public string Issuer { get; set; } = "stocksync";

    [Required]
    public string Audience { get; set; } = "stocksync-web";

    [MinLength(MinSigningKeyLength)]
    public string? SigningKey { get; set; }

    [Range(1, 60)]
    public int AccessTokenMinutes { get; set; } = 15;

    [Range(1, 90)]
    public int RefreshTokenDays { get; set; } = 7;

    [Range(1, 365)]
    public int SessionLifetimeDays { get; set; } = 30;

    public bool UsesEphemeralSigningKey { get; internal set; }

    public SymmetricSecurityKey CreateSecurityKey() =>
        new(Encoding.UTF8.GetBytes(SigningKey ?? throw new InvalidOperationException("JWT signing key is not configured.")));
}
