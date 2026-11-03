using System.ComponentModel.DataAnnotations;

namespace StockSync.Api.Configuration;

internal sealed class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    [Range(1, 100_000)]
    public int AuthPermitLimit { get; set; } = 10;

    [Range(1, 100_000)]
    public int SessionPermitLimit { get; set; } = 60;

    [Range(1, 100_000)]
    public int ApiPermitLimit { get; set; } = 300;

    [Range(1, 3600)]
    public int WindowSeconds { get; set; } = 60;
}
