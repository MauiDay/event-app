using System.Text.Json.Serialization;

namespace MauiDay.Core.Configuration;

[JsonConverter(typeof(JsonStringEnumConverter<ScheduleStatus>))]
public enum ScheduleStatus
{
    Preview,
    Published,
    Archived,
}

[JsonConverter(typeof(JsonStringEnumConverter<PartnerTier>))]
public enum PartnerTier
{
    Sponsor,
    Supporter,
}

[JsonConverter(typeof(JsonStringEnumConverter<SessionDisplayStatus>))]
public enum SessionDisplayStatus
{
    Scheduled,
    Cancelled,
    Rescheduled,
}

public sealed class EventConfiguration
{
    public const int CurrentSchemaVersion = 1;

    public required int SchemaVersion { get; init; }

    public required string Id { get; init; }

    public required string Name { get; init; }

    public required string EditionLabel { get; init; }

    public required string City { get; init; }

    public required string Country { get; init; }

    public required DateOnly Date { get; init; }

    public required string TimeZone { get; init; }

    public required ScheduleStatus ScheduleStatus { get; init; }

    public required SessionizeConfiguration Sessionize { get; init; }

    public required VenueConfiguration Venue { get; init; }

    public required EventLinks Links { get; init; }

    public required IReadOnlyList<OrganizerConfiguration> Organizers { get; init; }

    public required IReadOnlyList<ExternalLink> SocialLinks { get; init; }

    public required IReadOnlyList<PartnerConfiguration> Partners { get; init; }

    public required BrandConfiguration Brand { get; init; }

    public required IReadOnlyDictionary<string, SessionOverride> SessionOverrides { get; init; }
}

public sealed class SessionizeConfiguration
{
    public required string EventId { get; init; }

    public required Uri AllDataUrl { get; init; }

    public required string BundledDataAsset { get; init; }
}

public sealed class VenueConfiguration
{
    public required string Name { get; init; }

    public required string AddressLine1 { get; init; }

    public required string PostalCode { get; init; }

    public required string City { get; init; }

    public required string Country { get; init; }

    public int? Capacity { get; init; }

    public string? ArrivalNotes { get; init; }

    public string? Transit { get; init; }

    public string? Parking { get; init; }

    public required Uri MapsUrl { get; init; }

    public string FormattedAddress =>
        $"{AddressLine1}, {PostalCode} {City}, {Country}";
}

public sealed class EventLinks
{
    public required Uri Website { get; init; }

    public required Uri Tickets { get; init; }

    public required Uri CodeOfConduct { get; init; }

    public required Uri Privacy { get; init; }
}

public sealed class OrganizerConfiguration
{
    public required string Name { get; init; }

    public required string Email { get; init; }
}

public sealed class ExternalLink
{
    public required string Title { get; init; }

    public required Uri Url { get; init; }
}

public sealed class PartnerConfiguration
{
    public required string Name { get; init; }

    public required PartnerTier Tier { get; init; }

    public required Uri LogoUrl { get; init; }

    public required Uri WebsiteUrl { get; init; }
}

public sealed class BrandConfiguration
{
    public required Uri EditionBadgeUrl { get; init; }

    public required string BundledEditionBadge { get; init; }

    public required string PrimaryColor { get; init; }

    public required string AccentColor { get; init; }
}

public sealed class SessionOverride
{
    public SessionDisplayStatus Status { get; init; } = SessionDisplayStatus.Scheduled;

    public string? StatusNote { get; init; }

    public string? Title { get; init; }

    public string? Description { get; init; }

    public string? StartsAt { get; init; }

    public string? EndsAt { get; init; }

    public bool Hidden { get; init; }
}
