using MauiDay.Core.Configuration;
using MauiDay.Core.Models;
using MauiDay.Core.Sessionize;

namespace MauiDay.Core.Services;

public sealed class SessionizeMapper(IEventTimeService eventTimeService)
{
    private static readonly IReadOnlyDictionary<string, SessionOverride> EmptyOverrides =
        new Dictionary<string, SessionOverride>();

    public ConferenceData Map(SessionizeAllDto payload, EventConfiguration eventConfiguration)
    {
        ArgumentNullException.ThrowIfNull(payload);
        ArgumentNullException.ThrowIfNull(eventConfiguration);

        var rooms = (payload.Rooms ?? [])
            .Where(room => room is not null)
            .Select(room => new EventRoom(room.Id, RequireValue(room.Name, "room name"), room.Sort))
            .OrderBy(room => room.Sort)
            .ThenBy(room => room.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var sessions = (payload.Sessions ?? [])
            .Where(session => session is not null)
            .Select(session => MapSession(session, eventConfiguration))
            .Where(session => session is not null)
            .Cast<EventSession>()
            .OrderBy(session => session.StartsAt)
            .ThenBy(session => session.Title, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var linkedSessionIdsBySpeaker = sessions
            .SelectMany(session => session.SpeakerIds.Select(speakerId => (speakerId, session.Id)))
            .GroupBy(link => link.speakerId, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<string>)group.Select(link => link.Id).Distinct().ToArray(),
                StringComparer.Ordinal);

        var speakers = (payload.Speakers ?? [])
            .Where(speaker => speaker is not null)
            .Select(speaker => MapSpeaker(speaker, linkedSessionIdsBySpeaker))
            .OrderBy(speaker => speaker.FullName, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return new ConferenceData(sessions, speakers, rooms);
    }

    private EventSession? MapSession(
        SessionizeSessionDto session,
        EventConfiguration eventConfiguration)
    {
        var id = RequireValue(session.Id, "session id");
        (eventConfiguration.SessionOverrides ?? EmptyOverrides).TryGetValue(id, out var sessionOverride);
        if (sessionOverride?.Hidden == true)
        {
            return null;
        }

        var startsAtValue = sessionOverride?.StartsAt ?? session.StartsAt;
        var endsAtValue = sessionOverride?.EndsAt ?? session.EndsAt;
        var startsAt = eventTimeService.ParseSessionizeTimestamp(
            RequireValue(startsAtValue, $"start time for session {id}"),
            eventConfiguration.TimeZone);
        var endsAt = eventTimeService.ParseSessionizeTimestamp(
            RequireValue(endsAtValue, $"end time for session {id}"),
            eventConfiguration.TimeZone);

        if (endsAt <= startsAt)
        {
            throw new InvalidDataException($"Session '{id}' ends before it starts.");
        }

        return new EventSession(
            id,
            RequireValue(sessionOverride?.Title ?? session.Title, $"title for session {id}"),
            CleanOptional(sessionOverride?.Description ?? session.Description),
            startsAt,
            endsAt,
            session.IsServiceSession,
            (session.Speakers ?? []).Where(value => !string.IsNullOrWhiteSpace(value)).Distinct().ToArray(),
            session.RoomId,
            sessionOverride?.Status ?? SessionDisplayStatus.Scheduled,
            CleanOptional(sessionOverride?.StatusNote));
    }

    private static EventSpeaker MapSpeaker(
        SessionizeSpeakerDto speaker,
        IReadOnlyDictionary<string, IReadOnlyList<string>> linkedSessionIdsBySpeaker)
    {
        var id = RequireValue(speaker.Id, "speaker id");
        var firstName = CleanOptional(speaker.FirstName) ?? string.Empty;
        var lastName = CleanOptional(speaker.LastName) ?? string.Empty;
        var fullName = CleanOptional(speaker.FullName)
            ?? CleanOptional($"{firstName} {lastName}")
            ?? "Speaker to be announced";

        var links = (speaker.Links ?? [])
            .Where(link => link is not null)
            .Select(link =>
            {
                if (!Uri.TryCreate(link.Url, UriKind.Absolute, out var uri) ||
                    uri.Scheme is not ("http" or "https"))
                {
                    return null;
                }

                return new SpeakerLink(CleanOptional(link.Title) ?? uri.Host, uri);
            })
            .Where(link => link is not null)
            .Cast<SpeakerLink>()
            .ToArray();

        Uri? profilePicture = null;
        if (Uri.TryCreate(speaker.ProfilePicture, UriKind.Absolute, out var parsedProfilePicture) &&
            parsedProfilePicture.Scheme is "http" or "https")
        {
            profilePicture = parsedProfilePicture;
        }

        linkedSessionIdsBySpeaker.TryGetValue(id, out var linkedSessionIds);
        var sessionIds = (linkedSessionIds ?? [])
            .Concat((speaker.Sessions ?? []).Select(sessionId => sessionId.ToString()))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return new EventSpeaker(
            id,
            firstName,
            lastName,
            fullName,
            CleanOptional(speaker.Bio),
            CleanOptional(speaker.TagLine),
            profilePicture,
            links,
            sessionIds);
    }

    private static string RequireValue(string? value, string fieldName) =>
        CleanOptional(value)
        ?? throw new InvalidDataException($"Sessionize did not provide a valid {fieldName}.");

    private static string? CleanOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
