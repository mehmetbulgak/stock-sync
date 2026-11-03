namespace StockSync.Application.Common;

public static class BusinessTimeZone
{
    private static readonly Lazy<TimeZoneInfo> Instance = new(Resolve);

    public static TimeZoneInfo Get() => Instance.Value;

    public static DateOnly Today(DateTimeOffset now) => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(now, Get()).Date);

    public static DateOnly ToLocalDate(DateTimeOffset value) => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(value, Get()).Date);

    public static DateTimeOffset StartOfDayUtc(DateOnly day)
    {
        var local = day.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
        return new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(local, Get()), TimeSpan.Zero);
    }

    private static TimeZoneInfo Resolve()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.CreateCustomTimeZone("Istanbul", TimeSpan.FromHours(3), "Istanbul", "Istanbul");
        }
    }
}
