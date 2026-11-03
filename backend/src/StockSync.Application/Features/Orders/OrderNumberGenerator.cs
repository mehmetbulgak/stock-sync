using System.Security.Cryptography;
using StockSync.Application.Common;
using StockSync.Domain.Channels;

namespace StockSync.Application.Features.Orders;

public static class OrderNumberGenerator
{
    public static string Prefix(ChannelType type) => type switch
    {
        ChannelType.Trendyol => "TY",
        ChannelType.Hepsiburada => "HB",
        ChannelType.WebStore => "WEB",
        _ => "MAN",
    };

    public static string Generate(ChannelType type, DateTimeOffset now) =>
        $"{Prefix(type)}-{BusinessTimeZone.ToLocalDate(now):yyMMdd}-{RandomNumberGenerator.GetHexString(6)}";
}
