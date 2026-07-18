using MauiDay.Core.Services;
using MauiDay.Core.Sessionize;

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

    [Fact]
    public void SpeakerSessionsExcludeHiddenOrDroppedSessionIds()
    {
        var config = FixtureLoader.LoadEventConfiguration();
        var mapper = new SessionizeMapper(new EventTimeService());

        var payload = new SessionizeAllDto
        {
            Rooms = [new SessionizeRoomDto { Id = 1, Name = "Main hall", Sort = 0 }],
            Sessions =
            [
                new SessionizeSessionDto
                {
                    Id = "1",
                    Title = "Linked via session",
                    StartsAt = "2026-10-23T09:00:00",
                    EndsAt = "2026-10-23T10:00:00",
                    Speakers = ["S1"],
                    RoomId = 1,
                },
                new SessionizeSessionDto
                {
                    Id = "2",
                    Title = "Linked only via speaker",
                    StartsAt = "2026-10-23T11:00:00",
                    EndsAt = "2026-10-23T12:00:00",
                    Speakers = [],
                    RoomId = 1,
                },
            ],
            Speakers =
            [
                new SessionizeSpeakerDto
                {
                    Id = "S1",
                    FirstName = "Ada",
                    LastName = "Lovelace",
                    // 2 is a real visible session; 999 is a phantom/dropped id.
                    Sessions = [2, 999],
                },
            ],
        };

        var result = mapper.Map(payload, config);
        var speaker = Assert.Single(result.Speakers);

        Assert.Equal(["1", "2"], speaker.SessionIds.OrderBy(id => id));
        Assert.DoesNotContain("999", speaker.SessionIds);
    }
}
