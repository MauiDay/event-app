using MauiDay.Core.Configuration;
using MauiDay.Core.Models;

namespace MauiDay.Core.Services;

public enum ConferenceScenario
{
    Production,
    Empty,
    MissingContent,
    LongContent,
    MultipleSpeakers,
    ServiceSessions,
    CancelledSession,
}

public static class ConferenceScenarioFactory
{
    public static ConferenceData Apply(
        ConferenceData source,
        ConferenceScenario scenario)
    {
        ArgumentNullException.ThrowIfNull(source);

        return scenario switch
        {
            ConferenceScenario.Production => source,
            ConferenceScenario.Empty => new ConferenceData([], [], source.Rooms),
            ConferenceScenario.MissingContent => ApplyMissingContent(source),
            ConferenceScenario.LongContent => ApplyLongContent(source),
            ConferenceScenario.MultipleSpeakers => ApplyMultipleSpeakers(source),
            ConferenceScenario.ServiceSessions => ApplyServiceSessions(source),
            ConferenceScenario.CancelledSession => ApplyCancelledSession(source),
            _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, null),
        };
    }

    private static ConferenceData ApplyMissingContent(ConferenceData source)
    {
        var sessions = source.Sessions.ToArray();
        var speakers = source.Speakers.ToArray();
        if (sessions.Length > 0)
        {
            sessions[0] = sessions[0] with { Description = null };
        }

        if (speakers.Length > 0)
        {
            speakers[0] = speakers[0] with
            {
                Bio = null,
                TagLine = null,
                ProfilePicture = null,
            };
        }

        return new ConferenceData(sessions, speakers, source.Rooms);
    }

    private static ConferenceData ApplyLongContent(ConferenceData source)
    {
        var sessions = source.Sessions.ToArray();
        var speakers = source.Speakers.ToArray();
        if (sessions.Length > 0)
        {
            sessions[0] = sessions[0] with
            {
                Title =
                    "Building thoughtful, accessible, resilient, production-ready " +
                    "cross-platform experiences with .NET MAUI and modern AI",
                Description = string.Join(
                    "\n\n",
                    Enumerable.Repeat(
                        sessions[0].Description ?? "Detailed session information.",
                        4)),
            };
        }

        if (speakers.Length > 0)
        {
            speakers[0] = speakers[0] with
            {
                Bio = string.Join(
                    "\n\n",
                    Enumerable.Repeat(
                        speakers[0].Bio ?? "Detailed speaker information.",
                        4)),
            };
        }

        return new ConferenceData(sessions, speakers, source.Rooms);
    }

    private static ConferenceData ApplyMultipleSpeakers(ConferenceData source)
    {
        if (source.Sessions.Count == 0 || source.Speakers.Count < 2)
        {
            return source;
        }

        var firstSession = source.Sessions[0] with
        {
            SpeakerIds = source.Speakers.Select(speaker => speaker.Id).ToArray(),
        };
        var sessions = source.Sessions.ToArray();
        sessions[0] = firstSession;
        var speakers = source.Speakers
            .Select(speaker => speaker with
            {
                SessionIds = speaker.SessionIds
                    .Append(firstSession.Id)
                    .Distinct(StringComparer.Ordinal)
                    .ToArray(),
            })
            .ToArray();

        return new ConferenceData(sessions, speakers, source.Rooms);
    }

    private static ConferenceData ApplyServiceSessions(ConferenceData source)
    {
        if (source.Sessions.Count == 0)
        {
            return source;
        }

        var first = source.Sessions[0];
        var serviceStart = first.EndsAt;
        var serviceSession = new EventSession(
            "debug-service-session",
            "Coffee, hallway track, and community time",
            "Take a break, meet another attendee, and recharge.",
            serviceStart,
            serviceStart.AddMinutes(30),
            true,
            [],
            first.RoomId,
            SessionDisplayStatus.Scheduled,
            null);

        return source with
        {
            Sessions = source.Sessions
                .Append(serviceSession)
                .OrderBy(session => session.StartsAt)
                .ToArray(),
        };
    }

    private static ConferenceData ApplyCancelledSession(ConferenceData source)
    {
        if (source.Sessions.Count == 0)
        {
            return source;
        }

        var sessions = source.Sessions.ToArray();
        sessions[0] = sessions[0] with
        {
            DisplayStatus = SessionDisplayStatus.Cancelled,
            StatusNote = "This session is not taking place.",
        };
        return source with { Sessions = sessions };
    }
}
