using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;
using IPNetwork = System.Net.IPNetwork;

namespace StockSync.Api.Configuration;

internal static class ReverseProxySetup
{
    public static IServiceCollection AddReverseProxySupport(this IServiceCollection services)
    {
        services.AddOptions<ReverseProxyOptions>()
            .BindConfiguration(ReverseProxyOptions.SectionName)
            .Validate(
                proxy => proxy.KnownNetworks.All(network => IPNetwork.TryParse(network, out _)),
                "ReverseProxy:KnownNetworks must contain CIDR ranges such as 172.16.0.0/12.")
            .ValidateOnStart();

        services.AddOptions<ForwardedHeadersOptions>()
            .Configure<IOptions<ReverseProxyOptions>>((options, proxy) =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.ForwardLimit = 1;
                foreach (var network in proxy.Value.KnownNetworks)
                {
                    options.KnownIPNetworks.Add(IPNetwork.Parse(network));
                }
            });

        return services;
    }
}
