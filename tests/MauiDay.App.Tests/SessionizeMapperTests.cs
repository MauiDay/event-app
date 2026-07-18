using MauiDay.Core.Services;

namespace MauiDay.App.Tests;

public sealed class SessionizeMapperTests
{
    [Fact]
    public void BundledPayloadNormalizesSessionsSpeakersAndRooms()
    {
        var mapper = new SessionizeMapper(new EventTimeService());

        var result = mapper.Map(
            FixtureLoader.LoadSessionizeData(),
            FixtureLoader.LoadEventConfiguration());

        Assert.Equal(3, result.Sessions.Count);
        Assert.Equal(3, result.Speakers.Count);
        Assert.Single(result.Rooms);
        Assert.All(result.Sessions, session => Assert.Equal(TimeSpan.FromHours(1), session.Duration));
        Assert.All(result.Sessions, session => Assert.Equal(TimeSpan.FromHours(2), session.StartsAt.Offset));
    }

    [Fact]
    public void SessionAndSpeakerRelationshipsAreBidirectional()
    {
        var mapper = new SessionizeMapper(new EventTimeService());
        var result = mapper.Map(
            FixtureLoader.LoadSessionizeData(),
            FixtureLoader.LoadEventConfiguration());

        foreach (var session in result.Sessions)
        {
            foreach (var speakerId in session.SpeakerIds)
            {
                var speaker = Assert.Single(result.Speakers, item => item.Id == speakerId);
                Assert.Contains(session.Id, speaker.SessionIds);
            }
        }
    }
}
