using MauiDay.Core.Configuration;

namespace MauiDay.Core.Validation;

public sealed class ConfigurationValidationException(string message) : Exception(message);

public static class ConfigurationValidator
{
    public static void Validate(AppBootstrap bootstrap)
    {
        ArgumentNullException.ThrowIfNull(bootstrap);

        if (bootstrap.SchemaVersion != AppBootstrap.CurrentSchemaVersion)
        {
            throw new ConfigurationValidationException(
                $"Unsupported bootstrap schema version {bootstrap.SchemaVersion}.");
        }

        RequireText(bootstrap.ActiveEventId, "active event id");
        if (bootstrap.Events is null || bootstrap.Events.Count == 0)
        {
            throw new ConfigurationValidationException("Bootstrap must contain at least one event.");
        }

        foreach (var descriptor in bootstrap.Events)
        {
            RequireObject(descriptor, "event descriptor");
            RequireText(descriptor.Id, "event id");
            RequireText(descriptor.Name, $"name for event '{descriptor.Id}'");
            RequireWebUri(descriptor.ConfigUrl, $"config URL for event '{descriptor.Id}'");
            RequireText(
                descriptor.BundledConfigAsset,
                $"bundled config asset for event '{descriptor.Id}'");
        }

        if (!bootstrap.Events.Any(
                descriptor => descriptor.Id.Equals(
                    bootstrap.ActiveEventId,
                    StringComparison.Ordinal)))
        {
            throw new ConfigurationValidationException(
                $"Active event '{bootstrap.ActiveEventId}' is not present in the bootstrap.");
        }
    }

    public static void Validate(EventConfiguration eventConfiguration, string expectedEventId)
    {
        ArgumentNullException.ThrowIfNull(eventConfiguration);

        if (eventConfiguration.SchemaVersion != EventConfiguration.CurrentSchemaVersion)
        {
            throw new ConfigurationValidationException(
                $"Unsupported event schema version {eventConfiguration.SchemaVersion}.");
        }

        if (!string.Equals(eventConfiguration.Id, expectedEventId, StringComparison.Ordinal))
        {
            throw new ConfigurationValidationException(
                $"Event config id '{eventConfiguration.Id}' does not match '{expectedEventId}'.");
        }

        RequireObject(eventConfiguration.Sessionize, "Sessionize configuration");
        RequireObject(eventConfiguration.Venue, "venue configuration");
        RequireObject(eventConfiguration.Links, "event links");
        RequireObject(eventConfiguration.SocialLinks, "social links");
        RequireObject(eventConfiguration.Partners, "partners collection");
        RequireObject(eventConfiguration.Brand, "brand configuration");
        RequireObject(eventConfiguration.SessionOverrides, "session overrides");

        RequireText(eventConfiguration.Name, "event name");
        RequireText(eventConfiguration.EditionLabel, "event edition label");
        RequireText(eventConfiguration.City, "event city");
        RequireText(eventConfiguration.Country, "event country");
        RequireText(eventConfiguration.TimeZone, "event time zone");
        RequireText(eventConfiguration.Sessionize.EventId, "Sessionize event id");
        RequireWebUri(eventConfiguration.Sessionize.AllDataUrl, "Sessionize endpoint");
        RequireText(eventConfiguration.Sessionize.BundledDataAsset, "bundled Sessionize asset");
        RequireText(eventConfiguration.Venue.Name, "venue name");
        RequireText(eventConfiguration.Venue.AddressLine1, "venue address");
        RequireWebUri(eventConfiguration.Venue.MapsUrl, "venue maps URL");
        RequireWebUri(eventConfiguration.Links.Website, "event website");
        RequireWebUri(eventConfiguration.Links.Tickets, "event ticket URL");
        RequireWebUri(eventConfiguration.Links.CodeOfConduct, "Code of Conduct URL");
        RequireWebUri(eventConfiguration.Links.Privacy, "privacy URL");

        if (eventConfiguration.Organizers is null || eventConfiguration.Organizers.Count == 0)
        {
            throw new ConfigurationValidationException(
                "Event config must contain at least one organizer.");
        }

        foreach (var organizer in eventConfiguration.Organizers)
        {
            RequireObject(organizer, "organizer");
            RequireText(organizer.Name, "organizer name");
            if (organizer.Email is null ||
                !organizer.Email.Contains('@', StringComparison.Ordinal))
            {
                throw new ConfigurationValidationException(
                    $"Organizer '{organizer.Name}' has an invalid email address.");
            }
        }

        foreach (var partner in eventConfiguration.Partners)
        {
            RequireObject(partner, "partner");
            RequireText(partner.Name, "partner name");
            RequireWebUri(partner.LogoUrl, $"logo URL for '{partner.Name}'");
            RequireWebUri(partner.WebsiteUrl, $"website URL for '{partner.Name}'");
        }

        foreach (var socialLink in eventConfiguration.SocialLinks)
        {
            RequireObject(socialLink, "social link");
            RequireText(socialLink.Title, "social link title");
            RequireWebUri(socialLink.Url, $"URL for social link '{socialLink.Title}'");
        }

        RequireWebUri(eventConfiguration.Brand.EditionBadgeUrl, "edition badge URL");
        RequireText(eventConfiguration.Brand.BundledEditionBadge, "bundled edition badge");
        RequireText(eventConfiguration.Brand.PrimaryColor, "brand primary color");
        RequireText(eventConfiguration.Brand.AccentColor, "brand accent color");

        try
        {
            _ = TimeZoneInfo.FindSystemTimeZoneById(eventConfiguration.TimeZone);
        }
        catch (TimeZoneNotFoundException exception)
        {
            throw new ConfigurationValidationException(
                $"Unknown event time zone '{eventConfiguration.TimeZone}': {exception.Message}");
        }
        catch (InvalidTimeZoneException exception)
        {
            throw new ConfigurationValidationException(
                $"Invalid event time zone '{eventConfiguration.TimeZone}': {exception.Message}");
        }
    }

    private static void RequireText(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ConfigurationValidationException($"Missing {fieldName}.");
        }
    }

    private static void RequireObject(object? value, string fieldName)
    {
        if (value is null)
        {
            throw new ConfigurationValidationException($"Missing {fieldName}.");
        }
    }

    private static void RequireWebUri(Uri? value, string fieldName)
    {
        if (value is null ||
            !value.IsAbsoluteUri ||
            value.Scheme is not ("http" or "https"))
        {
            throw new ConfigurationValidationException($"Invalid {fieldName}.");
        }
    }
}
