using MauiDay.Core.Services;

namespace MauiDay.App.Tests;

public sealed class EventTimeServiceTests
{
    private readonly EventTimeService _service = new();

    [Fact]
    public void OffsetlessSummerTimestampIsInterpretedAsSkopjeWallClock()
    {
        var result = _service.ParseSessionizeTimestamp(
            "2026-09-17T10:00:00",
            "Europe/Skopje");

        Assert.Equal(new DateTimeOffset(2026, 9, 17, 10, 0, 0, TimeSpan.FromHours(2)), result);
    }

    [Fact]
    public void OffsetlessWinterTimestampUsesStandardOffset()
    {
        var result = _service.ParseSessionizeTimestamp(
            "2026-12-23T09:00:00",
            "Europe/Skopje");

        Assert.Equal(TimeSpan.FromHours(1), result.Offset);
    }

    [Fact]
    public void ExplicitOffsetTimestampIsConvertedIntoEventTime()
    {
        var result = _service.ParseSessionizeTimestamp(
            "2026-09-17T08:00:00Z",
            "Europe/Skopje");

        Assert.Equal(10, result.Hour);
        Assert.Equal(TimeSpan.FromHours(2), result.Offset);
    }

    [Fact]
    public void InvalidDaylightSavingTimestampIsRejected()
    {
        Assert.Throws<FormatException>(
            () => _service.ParseSessionizeTimestamp(
                "2026-03-29T02:30:00",
                "Europe/Skopje"));
    }

    [Fact]
    public void DescribeTimeZoneReportsCityAndSummerOffsetOnEventDay()
    {
        var label = _service.DescribeTimeZone(
            "Europe/Skopje",
            new DateOnly(2026, 9, 17),
            "Skopje");

        Assert.Contains("Skopje", label);
        Assert.Contains("UTC+02:00", label);
    }
}
