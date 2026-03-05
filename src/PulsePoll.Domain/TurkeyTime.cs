public static class TurkeyTime
{
    private static readonly TimeZoneInfo TurkeyTimeZone = ResolveTurkeyTimeZone();

    public static DateTime Now
    {
        get
        {
            var value = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TurkeyTimeZone);
            return DateTime.SpecifyKind(value, DateTimeKind.Unspecified);
        }
    }

    public static DateTime Today => Now.Date;

    public static DateTimeOffset OffsetNow
    {
        get
        {
            var now = Now;
            var offset = TurkeyTimeZone.GetUtcOffset(now);
            return new DateTimeOffset(now, offset);
        }
    }

    public static string TimeZoneId => TurkeyTimeZone.Id;

    public static DateTime FromUtc(DateTime utcDateTime)
    {
        var utc = utcDateTime.Kind == DateTimeKind.Utc
            ? utcDateTime
            : DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);

        var value = TimeZoneInfo.ConvertTimeFromUtc(utc, TurkeyTimeZone);
        return DateTime.SpecifyKind(value, DateTimeKind.Unspecified);
    }

    private static TimeZoneInfo ResolveTurkeyTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
        }
    }
}
