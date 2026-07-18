using MauiDay.Core.Services;

namespace MauiDay.App.Tests;

public sealed class ConferenceScenarioFactoryTests
{
    private static readonly EventTimeService TimeService = new();

    [Fact]
    public void EmptyScenarioRetainsRoomMetadata()
    {
        var source = LoadConference();

        var result = ConferenceScenarioFactory.Apply(source, ConferenceScenario.Empty);

        Assert.Empty(result.Sessions);
        Assert.Empty(result.Speakers);
        Assert.Equal(source.Rooms, result.Rooms);
    }

    [Fact]
    public void MissingContentScenarioExercisesIntentionalFallbacks()
    {
        var result = ConferenceScenarioFactory.Apply(
            LoadConference(),
            ConferenceScenario.MissingContent);

        Assert.Null(result.Sessions[0].Description);
        Assert.Null(result.Speakers[0].Bio);
        Assert.Null(result.Speakers[0].TagLine);
        Assert.Null(result.Speakers[0].ProfilePicture);
    }

    [Fact]
    public void MultipleSpeakerScenarioKeepsRelationshipsBidirectional()
    {
        var result = ConferenceScenarioFactory.Apply(
            LoadConference(),
            ConferenceScenario.MultipleSpeakers);
        var session = result.Sessions[0];

        Assert.Equal(result.Speakers.Count, session.SpeakerIds.Count);
        Assert.All(result.Speakers, speaker => Assert.Contains(session.Id, speaker.SessionIds));
    }

    [Fact]
    public void ServiceScenarioAddsNonSpeakerProgramItem()
    {
        var result = ConferenceScenarioFactory.Apply(
            LoadConference(),
            ConferenceScenario.ServiceSessions);
        var serviceSession = Assert.Single(
            result.Sessions,
            session => session.Id == "debug-service-session");

        Assert.True(serviceSession.IsServiceSession);
        Assert.Empty(serviceSession.SpeakerIds);
        Assert.Equal(TimeSpan.FromMinutes(30), serviceSession.Duration);
    }

    [Fact]
    public void CancelledScenarioAddsNonColorStatusCopy()
    {
        var result = ConferenceScenarioFactory.Apply(
            LoadConference(),
            ConferenceScenario.CancelledSession);

        Assert.Equal(
            MauiDay.Core.Configuration.SessionDisplayStatus.Cancelled,
            result.Sessions[0].DisplayStatus);
        Assert.False(string.IsNullOrWhiteSpace(result.Sessions[0].StatusNote));
    }

    private static MauiDay.Core.Models.ConferenceData LoadConference() =>
        new SessionizeMapper(TimeService).Map(
            FixtureLoader.LoadSessionizeData(),
            FixtureLoader.LoadEventConfiguration());
}
