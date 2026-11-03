using System.Threading.RateLimiting;
using Microsoft.Extensions.Options;
using StockSync.Application.Abstractions;

namespace StockSync.Infrastructure.Auth;

internal sealed class LoginThrottle : ILoginThrottle, IDisposable
{
    private readonly PartitionedRateLimiter<string> _failures;

    public LoginThrottle(IOptions<LoginThrottleOptions> options)
    {
        var limits = options.Value;
        _failures = PartitionedRateLimiter.Create<string, string>(email =>
            RateLimitPartition.GetFixedWindowLimiter(email, _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = limits.MaxFailuresPerAccount,
                Window = TimeSpan.FromMinutes(limits.WindowMinutes),
                QueueLimit = 0,
            }));
    }

    public bool IsBlocked(string normalizedEmail) =>
        _failures.GetStatistics(normalizedEmail) is { CurrentAvailablePermits: <= 0 };

    public void RecordFailure(string normalizedEmail)
    {
        using var lease = _failures.AttemptAcquire(normalizedEmail);
    }

    public void Dispose() => _failures.Dispose();
}
