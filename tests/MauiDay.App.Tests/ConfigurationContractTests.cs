using System.Text.Json;
using MauiDay.Core.Configuration;
using MauiDay.Core.Serialization;
using MauiDay.Core.Validation;

namespace MauiDay.App.Tests;

public sealed class ConfigurationContractTests
{
    [Fact]
    public void BundledBootstrapSelectsCologne()
    {
        var bootstrap = FixtureLoader.Load<AppBootstrap>("config/bootstrap.json");

        ConfigurationValidator.Validate(bootstrap);

        Assert.Equal("cologne-2026", bootstrap.ActiveEventId);
        Assert.Contains(bootstrap.Events, descriptor => descriptor.Id == "cologne-2026");
    }

    [Fact]
    public void BundledCologneConfigContainsExpectedSourceFacts()
    {
        var config = FixtureLoader.LoadEventConfiguration();

        Assert.Equal(new DateOnly(2026, 10, 23), config.Date);
        Assert.Equal("Europe/Berlin", config.TimeZone);
        Assert.Equal("o0aj9rpg", config.Sessionize.EventId);
        Assert.Equal(ScheduleStatus.Preview, config.ScheduleStatus);
        Assert.Equal("Microsoft Cologne Office", config.Venue.Name);
        Assert.Equal(160, config.Venue.Capacity);
        Assert.Single(config.Partners, partner => partner.Tier == PartnerTier.Sponsor);
        Assert.Equal(4, config.Partners.Count(partner => partner.Tier == PartnerTier.Supporter));
    }

    [Fact]
    public void UnsupportedSchemaIsRejected()
    {
        var bootstrap = new AppBootstrap
        {
            SchemaVersion = 99,
            ActiveEventId = "event",
            Events = [],
        };

        Assert.Throws<ConfigurationValidationException>(
            () => ConfigurationValidator.Validate(bootstrap));
    }

    [Fact]
    public void EnumConfigValuesRejectIntegersAndUnknownStrings()
    {
        Assert.Equal(
            ScheduleStatus.Published,
            JsonSerializer.Deserialize<ScheduleStatus>("\"published\"", MauiDayJson.Options));

        // Numeric enum values must not silently map to an undefined status.
        Assert.ThrowsAny<JsonException>(
            () => JsonSerializer.Deserialize<ScheduleStatus>("1", MauiDayJson.Options));

        // Unknown status strings must fail loudly rather than defaulting.
        Assert.ThrowsAny<JsonException>(
            () => JsonSerializer.Deserialize<SessionDisplayStatus>("\"postponed\"", MauiDayJson.Options));
    }
}
