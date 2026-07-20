using MauiDay.Core.Configuration;
using MauiDay.Core.Models;
using MauiDay.Core.Services;

namespace MauiDay.App.Tests;

public sealed class TodayStateCalculatorTests
{
    private readonly EventTimeService _timeService = new();

    [Fact]
    public void ExactStartMinuteIsLiveAndExactEndMinuteIsNot()
    {
        var configuration = FixtureLoader.LoadEventConfiguration();
        var conference = new SessionizeMapper(_timeService).Map(
            FixtureLoader.LoadSessionizeData(),
            configuration);
        var calculator = new TodayStateCalculator(_timeService);

        var atStart = calculator.Calculate(
            configuration,
            conference,
            new DateTimeOffset(2026, 10, 23, 7, 0, 0, TimeSpan.Zero));
        var atEnd = calculator.Calculate(
            configuration,
            conference,
            new DateTimeOffset(2026, 10, 23, 8, 0, 0, TimeSpan.Zero));

        Assert.Equal(TodayPhase.Live, atStart.Phase);
        Assert.Equal("1272305", atStart.CurrentSession?.Id);
        Assert.Equal(TodayPhase.BetweenSessions, atEnd.Phase);
        Assert.Null(atEnd.CurrentSession);
        Assert.Equal("1272319", atEnd.NextSession?.Id);
    }

    [Fact]
    public void PreEventCountdownNeverBecomesNegative()
    {
        var configuration = FixtureLoader.LoadEventConfiguration();
        var conference = new SessionizeMapper(_timeService).Map(
            FixtureLoader.LoadSessionizeData(),
            configuration);

        var state = new TodayStateCalculator(_timeService).Calculate(
            configuration,
            conference,
            new DateTimeOffset(2026, 10, 22, 21, 0, 0, TimeSpan.Zero));

        Assert.Equal(TodayPhase.PreEvent, state.Phase);
        Assert.True(state.TimeUntilEvent >= TimeSpan.Zero);
    }

    [Fact]
    public void AfterLastSessionIsPostEvent()
    {
        var configuration = FixtureLoader.LoadEventConfiguration();
        var conference = new SessionizeMapper(_timeService).Map(
            FixtureLoader.LoadSessionizeData(),
            configuration);

        var state = new TodayStateCalculator(_timeService).Calculate(
            configuration,
            conference,
            new DateTimeOffset(2026, 10, 23, 15, 0, 0, TimeSpan.Zero));

        Assert.Equal(TodayPhase.PostEvent, state.Phase);
        Assert.Null(state.NextSession);
        Assert.Null(state.TimeUntilEvent);
    }

    [Fact]
    public void CancelledSessionIsNeverTreatedAsLive()
    {
        var configuration = FixtureLoader.LoadEventConfiguration();
        var start = _timeService.ParseSessionizeTimestamp(
            "2026-10-23T09:00:00", configuration.TimeZone);
        var end = _timeService.ParseSessionizeTimestamp(
            "2026-10-23T10:00:00", configuration.TimeZone);
        var conference = new ConferenceData(
            [
                new EventSession(
                    "cancelled-1", "Cancelled talk", "Off", start, end,
                    IsServiceSession: false, SpeakerIds: [], RoomId: null,
                    SessionDisplayStatus.Cancelled, "This session has been cancelled."),
            ],
            [],
            []);

        var now = _timeService.ParseSessionizeTimestamp(
            "2026-10-23T09:30:00", configuration.TimeZone);
        var state = new TodayStateCalculator(_timeService).Calculate(
            configuration, conference, now);

        Assert.NotEqual(TodayPhase.Live, state.Phase);
        Assert.Null(state.CurrentSession);
    }
}
