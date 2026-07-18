using System.Globalization;
using MauiDay.Core.Configuration;
using MauiDay.Core.Models;

namespace MauiDay.App.ViewModels;

public sealed record SessionCardModel(
    string Id,
    string TimeText,
    string DurationText,
    string Title,
    string SpeakerText,
    string? RoomText,
    string? StatusText,
    bool IsServiceSession,
    bool IsLive,
    bool IsNavigable,
    string AccessibilityDescription)
{
    public bool HasRoom => !string.IsNullOrWhiteSpace(RoomText);

    public bool HasStatus => !string.IsNullOrWhiteSpace(StatusText);

    public static SessionCardModel Create(
        AppDataSnapshot snapshot,
        EventSession session,
        DateTimeOffset now)
    {
        var speakers = session.SpeakerIds
            .Select(snapshot.Conference.FindSpeaker)
            .Where(speaker => speaker is not null)
            .Select(speaker => speaker!.FullName)
            .ToArray();
        var speakerText = speakers.Length == 0
            ? session.IsServiceSession ? "MAUI Day" : "Speaker to be announced"
            : string.Join(", ", speakers);
        var eventNow = TimeZoneInfo.ConvertTime(
            now,
            TimeZoneInfo.FindSystemTimeZoneById(snapshot.Event.TimeZone));
        var isLive = session.DisplayStatus != SessionDisplayStatus.Cancelled
            && session.StartsAt <= eventNow && eventNow < session.EndsAt;
        var showRoom = snapshot.Conference.Rooms.Count > 1;
        var roomText = showRoom
            ? snapshot.Conference.FindRoom(session.RoomId)?.Name
            : null;
        var statusText = session.DisplayStatus switch
        {
            SessionDisplayStatus.Cancelled => "Cancelled",
            SessionDisplayStatus.Rescheduled => "Rescheduled",
            _ => null,
        };
        var durationMinutes = Math.Max(1, (int)Math.Round(session.Duration.TotalMinutes));
        var accessibilityDescription =
            $"{session.StartsAt:HH:mm}, {session.Title}, {speakerText}, {durationMinutes} minutes";

        return new SessionCardModel(
            session.Id,
            session.StartsAt.ToString("HH:mm", CultureInfo.InvariantCulture),
            $"{durationMinutes} min",
            session.Title,
            speakerText,
            roomText,
            statusText,
            session.IsServiceSession,
            isLive,
            !session.IsServiceSession || !string.IsNullOrWhiteSpace(session.Description),
            accessibilityDescription);
    }
}

public sealed record SpeakerCardModel(
    string Id,
    string FullName,
    string? TagLine,
    Uri? ProfilePicture,
    string Initials,
    int SessionCount,
    string AccessibilityDescription)
{
    public bool HasProfilePicture => ProfilePicture is not null;

    public static SpeakerCardModel Create(EventSpeaker speaker) =>
        new(
            speaker.Id,
            speaker.FullName,
            speaker.TagLine,
            speaker.ProfilePicture,
            speaker.Initials,
            speaker.SessionIds.Count,
            $"{speaker.FullName}, {speaker.TagLine ?? "MAUI Day speaker"}");
}

public sealed record PartnerCardModel(
    string Name,
    PartnerTier Tier,
    Uri LogoUrl,
    Uri WebsiteUrl)
{
    public static PartnerCardModel Create(PartnerConfiguration partner)
    {
        return new PartnerCardModel(
            partner.Name,
            partner.Tier,
            partner.LogoUrl,
            partner.WebsiteUrl);
    }
}
