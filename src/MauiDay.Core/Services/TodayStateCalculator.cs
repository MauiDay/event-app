using MauiDay.Core.Configuration;
using MauiDay.Core.Models;

namespace MauiDay.Core.Services;

public enum TodayPhase
{
    PreEvent,
    EventDayBeforeStart,
    Live,
    BetweenSessions,
    EventDayUnscheduled,
    PostEvent,
}

public sealed record TodayState(
    TodayPhase Phase,
    EventSession? CurrentSession,
    EventSession? NextSession,
    TimeSpan? TimeUntilEvent);

public sealed class TodayStateCalculator(IEventTimeService eventTimeService)
{
    public TodayState Calculate(
        EventConfiguration eventConfiguration,
        ConferenceData conference,
        DateTimeOffset now)
    {
        var eventNow = eventTimeService.ToEventTime(now, eventConfiguration.TimeZone);
        var localDate = DateOnly.FromDateTime(eventNow.DateTime);
        var sessions = conference.Sessions
            .Where(session => session.DisplayStatus != SessionDisplayStatus.Cancelled)
            .OrderBy(session => session.StartsAt)
            .ToArray();

        if (localDate < eventConfiguration.Date)
        {
            var eventMidnight = eventTimeService.ParseSessionizeTimestamp(
                $"{eventConfiguration.Date:yyyy-MM-dd}T00:00:00",
                eventConfiguration.TimeZone);
            return new TodayState(
                TodayPhase.PreEvent,
                null,
                sessions.FirstOrDefault(),
                eventMidnight - eventNow);
        }

        if (localDate > eventConfiguration.Date)
        {
            return new TodayState(TodayPhase.PostEvent, null, null, null);
        }

        if (sessions.Length == 0)
        {
            return new TodayState(TodayPhase.EventDayUnscheduled, null, null, null);
        }

        var current = sessions.FirstOrDefault(
            session => session.StartsAt <= eventNow && eventNow < session.EndsAt);
        var next = sessions.FirstOrDefault(session => session.StartsAt > eventNow);

        if (current is not null)
        {
            return new TodayState(TodayPhase.Live, current, next, null);
        }

        if (eventNow < sessions[0].StartsAt)
        {
            return new TodayState(
                TodayPhase.EventDayBeforeStart,
                null,
                sessions[0],
                sessions[0].StartsAt - eventNow);
        }

        if (next is not null)
        {
            return new TodayState(TodayPhase.BetweenSessions, null, next, null);
        }

        return new TodayState(TodayPhase.PostEvent, null, null, null);
    }
}
