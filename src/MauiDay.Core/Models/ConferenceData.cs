using MauiDay.Core.Configuration;

namespace MauiDay.Core.Models;

public sealed record ConferenceData(
    IReadOnlyList<EventSession> Sessions,
    IReadOnlyList<EventSpeaker> Speakers,
    IReadOnlyList<EventRoom> Rooms)
{
    public EventSession? FindSession(string id) =>
        Sessions.FirstOrDefault(session => session.Id == id);

    public EventSpeaker? FindSpeaker(string id) =>
        Speakers.FirstOrDefault(speaker => speaker.Id == id);

    public EventRoom? FindRoom(int? id) =>
        id is null ? null : Rooms.FirstOrDefault(room => room.Id == id.Value);
}

public sealed record EventSession(
    string Id,
    string Title,
    string? Description,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    bool IsServiceSession,
    IReadOnlyList<string> SpeakerIds,
    int? RoomId,
    SessionDisplayStatus DisplayStatus,
    string? StatusNote)
{
    public TimeSpan Duration => EndsAt - StartsAt;
}

public sealed record EventSpeaker(
    string Id,
    string FirstName,
    string LastName,
    string FullName,
    string? Bio,
    string? TagLine,
    Uri? ProfilePicture,
    IReadOnlyList<SpeakerLink> Links,
    IReadOnlyList<string> SessionIds)
{
    public string Initials
    {
        get
        {
            var first = string.IsNullOrWhiteSpace(FirstName) ? string.Empty : FirstName[..1];
            var last = string.IsNullOrWhiteSpace(LastName) ? string.Empty : LastName[..1];
            return $"{first}{last}".ToUpperInvariant();
        }
    }
}

public sealed record SpeakerLink(string Title, Uri Url);

public sealed record EventRoom(int Id, string Name, int Sort);
