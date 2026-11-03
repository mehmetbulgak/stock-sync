namespace StockSync.Api.Configuration;

internal sealed class ReverseProxyOptions
{
    public const string SectionName = "ReverseProxy";

    public bool Enabled { get; set; }

    public string[] KnownNetworks { get; set; } = [];
}
