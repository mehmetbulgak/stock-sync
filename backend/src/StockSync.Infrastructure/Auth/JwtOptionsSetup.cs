using System.Security.Cryptography;
using Microsoft.Extensions.Options;

namespace StockSync.Infrastructure.Auth;

internal sealed class JwtOptionsSetup : IPostConfigureOptions<JwtOptions>
{
    private const int EphemeralKeyBytes = 64;

    public void PostConfigure(string? name, JwtOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.SigningKey))
        {
            return;
        }

        options.SigningKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(EphemeralKeyBytes));
        options.UsesEphemeralSigningKey = true;
    }
}
