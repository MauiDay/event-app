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
    }
}
