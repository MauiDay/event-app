using MauiDay.Core.Models;

namespace MauiDay.Core.Services;

/// <summary>
/// Builds the freshness notice shown after a refresh. The message reflects whether content was
/// actually fetched during this refresh cycle, not merely where the content originated, so a
/// failed schedule fetch is never presented as if it just updated.
/// </summary>
public static class RefreshNoticeBuilder
{
    public static string Build(
        ContentSource conferenceSource,
        bool configurationRefreshed,
        bool conferenceRefreshed)
    {
        // The schedule is the primary content attendees rely on, so its freshness drives the
        // message. Only claim an update when the schedule was genuinely fetched this cycle.
        if (conferenceRefreshed)
        {
            return configurationRefreshed
                ? "App information updated just now."
                : "Schedule updated from Sessionize.";
        }

        return conferenceSource switch
        {
            ContentSource.Remote => "Couldn't reach Sessionize. Showing the latest schedule you have.",
            ContentSource.Cache => "Using saved schedule. Pull to retry.",
            _ => "Using bundled schedule. Pull to retry.",
        };
    }
}
