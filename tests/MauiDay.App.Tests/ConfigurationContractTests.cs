using System.Text.Json;
using MauiDay.Core.Configuration;
using MauiDay.Core.Serialization;
using MauiDay.Core.Sessionize;
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
        var descriptor = Assert.Single(bootstrap.Events);
        Assert.Equal(bootstrap.ActiveEventId, descriptor.Id);
    }

    [Fact]
    public void BundledCologneConfigContainsExpectedSourceFacts()
    {
        var config = FixtureLoader.LoadEventConfiguration();
        var sessionize = FixtureLoader.LoadSessionizeData();

        Assert.Equal("Cologne", config.City);
        Assert.Equal("Cologne 2026", config.EditionLabel);
        Assert.Equal("Germany", config.Country);
        Assert.Equal(new DateOnly(2026, 10, 23), config.Date);
        Assert.Equal("Europe/Berlin", config.TimeZone);
        Assert.Equal("o0aj9rpg", config.Sessionize.EventId);
        Assert.Equal(new Uri("https://sessionize.com/api/v2/o0aj9rpg/view/All"), config.Sessionize.AllDataUrl);
        Assert.Equal(ScheduleStatus.Published, config.ScheduleStatus);
        Assert.Equal("Microsoft, Cologne Office", config.Venue.Name);
        Assert.Equal("Holzmarkt 2", config.Venue.AddressLine1);
        Assert.Equal("50676", config.Venue.PostalCode);
        Assert.Equal(config.City, config.Venue.City);
        Assert.Equal(config.Country, config.Venue.Country);
        Assert.Equal(100, config.Venue.Capacity);
        Assert.Equal(2, config.Partners.Count(partner => partner.Tier == PartnerTier.Sponsor));
        Assert.Equal(4, config.Partners.Count(partner => partner.Tier == PartnerTier.Supporter));
        Assert.NotEmpty(sessionize.Sessions);
        Assert.NotEmpty(sessionize.Speakers);
    }

    [Fact]
    public void ActiveEditionBadgeIsBundled()
    {
        var config = FixtureLoader.LoadEventConfiguration();

        Assert.Equal("cologne26.png", config.Brand.BundledEditionBadge);
        Assert.Equal(config.Brand.BundledEditionBadge, Path.GetFileName(config.Brand.EditionBadgeUrl.LocalPath));
        Assert.True(File.Exists(Path.Combine(
            AppContext.BaseDirectory, "images", config.Brand.BundledEditionBadge)));
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
