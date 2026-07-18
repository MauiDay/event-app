using System.Globalization;

namespace MauiDay.Core.Services;

public interface IEventTimeService
{
    TimeZoneInfo GetTimeZone(string timeZoneId);

    DateTimeOffset ParseSessionizeTimestamp(string value, string timeZoneId);

    DateTimeOffset ToEventTime(DateTimeOffset value, string timeZoneId);

    string DescribeTimeZone(string timeZoneId, DateOnly onDate, string city);
}

public sealed class EventTimeService : IEventTimeService
{
    public TimeZoneInfo GetTimeZone(string timeZoneId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(timeZoneId);
        return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
    }

    public DateTimeOffset ParseSessionizeTimestamp(string value, string timeZoneId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var timeZone = GetTimeZone(timeZoneId);
        if (HasExplicitOffset(value))
        {
            if (!DateTimeOffset.TryParse(
                    value,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.RoundtripKind,
                    out var timestamp))
            {
                throw new FormatException($"'{value}' is not a valid Sessionize timestamp.");
            }

            return TimeZoneInfo.ConvertTime(timestamp, timeZone);
        }

        if (!DateTime.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces,
                out var localClock))
        {
            throw new FormatException($"'{value}' is not a valid Sessionize local timestamp.");
        }

        localClock = DateTime.SpecifyKind(localClock, DateTimeKind.Unspecified);
        if (timeZone.IsInvalidTime(localClock))
        {
            throw new FormatException($"'{value}' falls inside a daylight-saving time gap.");
        }

        var offset = timeZone.IsAmbiguousTime(localClock)
            ? timeZone.GetAmbiguousTimeOffsets(localClock).Max()
            : timeZone.GetUtcOffset(localClock);

        return new DateTimeOffset(localClock, offset);
    }

    public DateTimeOffset ToEventTime(DateTimeOffset value, string timeZoneId) =>
        TimeZoneInfo.ConvertTime(value, GetTimeZone(timeZoneId));

    public string DescribeTimeZone(string timeZoneId, DateOnly onDate, string city)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(city);

        var timeZone = GetTimeZone(timeZoneId);
        var reference = new DateTime(onDate.Year, onDate.Month, onDate.Day, 12, 0, 0, DateTimeKind.Unspecified);
        var offset = timeZone.GetUtcOffset(reference);
        var sign = offset < TimeSpan.Zero ? "-" : "+";
        var offsetText = $"UTC{sign}{Math.Abs(offset.Hours):00}:{Math.Abs(offset.Minutes):00}";
        return $"All times shown in {city} local time ({offsetText}).";
    }

    private static bool HasExplicitOffset(string value)
    {
        var trimmed = value.Trim();
        if (trimmed.EndsWith('Z', StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (trimmed.Length < 6 || trimmed[^3] != ':')
        {
            return false;
        }

        return trimmed[^6] is '+' or '-';
    }
}
