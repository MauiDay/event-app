using System.Text.Json;
using MauiDay.Core.Configuration;
using MauiDay.Core.Serialization;
using MauiDay.Core.Sessionize;
using MauiDay.Core.Validation;

namespace MauiDay.App.Tests;

public sealed class ConfigurationContractTests
{
    [Fact]
    public void BundledBootstrapSelectsSkopje()
    {
        var bootstrap = FixtureLoader.Load<AppBootstrap>("config/bootstrap.json");

        ConfigurationValidator.Validate(bootstrap);

        Assert.Equal("skopje-2026", bootstrap.ActiveEventId);
        Assert.Contains(bootstrap.Events, descriptor => descriptor.Id == "skopje-2026");
        Assert.Contains(bootstrap.Events, descriptor => descriptor.Id == "cologne-2026");
    }

    [Fact]
    public void BundledSkopjeConfigContainsExpectedSourceFacts()
    {
        var config = FixtureLoader.LoadEventConfiguration("skopje-2026");
        var sessionize = FixtureLoader.Load<SessionizeAllDto>(
            "config/data/skopje-2026-sessionize-all.json");

        Assert.Equal(new DateOnly(2026, 9, 17), config.Date);
        Assert.Equal("Europe/Skopje", config.TimeZone);
        Assert.Equal("q9b8reb9", config.Sessionize.EventId);
        Assert.Equal(ScheduleStatus.Preview, config.ScheduleStatus);
        Assert.Equal("Faculty of Computer Science & Engineering", config.Venue.Name);
        Assert.Equal(70, config.Venue.Capacity);
        Assert.Equal(2, config.Partners.Count(partner => partner.Tier == PartnerTier.Sponsor));
        Assert.Equal(6, config.Partners.Count(partner => partner.Tier == PartnerTier.Supporter));
        Assert.NotEmpty(sessionize.Sessions);
        Assert.NotEmpty(sessionize.Speakers);
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
