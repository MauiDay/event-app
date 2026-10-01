using MauiDay.Core.Services;

namespace MauiDay.App.Tests;

public sealed class EventTimeServiceTests
{
    private readonly EventTimeService _service = new();

    [Fact]
    public void OffsetlessSummerTimestampIsInterpretedAsCologneWallClock()
    {
        var result = _service.ParseSessionizeTimestamp(
            "2026-10-23T10:00:00",
            "Europe/Berlin");

        Assert.Equal(new DateTimeOffset(2026, 10, 23, 10, 0, 0, TimeSpan.FromHours(2)), result);
    }

    [Fact]
    public void OffsetlessWinterTimestampUsesStandardOffset()
    {
        var result = _service.ParseSessionizeTimestamp(
            "2026-12-23T09:00:00",
            "Europe/Berlin");

        Assert.Equal(TimeSpan.FromHours(1), result.Offset);
    }

    [Fact]
    public void ExplicitOffsetTimestampIsConvertedIntoEventTime()
    {
        var result = _service.ParseSessionizeTimestamp(
            "2026-10-23T08:00:00Z",
            "Europe/Berlin");

        Assert.Equal(10, result.Hour);
        Assert.Equal(TimeSpan.FromHours(2), result.Offset);
    }

    [Fact]
    public void InvalidDaylightSavingTimestampIsRejected()
    {
        Assert.Throws<FormatException>(
            () => _service.ParseSessionizeTimestamp(
                "2026-03-29T02:30:00",
                "Europe/Berlin"));
    }

    [Fact]
    public void DescribeTimeZoneReportsCityAndSummerOffsetOnEventDay()
    {
        var label = _service.DescribeTimeZone(
            "Europe/Berlin",
            new DateOnly(2026, 10, 23),
            "Cologne");

        Assert.Contains("Cologne", label);
        Assert.Contains("UTC+02:00", label);
    }
}
