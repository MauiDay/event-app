using System.Text.Json;
using System.Text.Json.Nodes;
using MauiDay.Core.Configuration;
using MauiDay.Core.Serialization;
using MauiDay.Core.Services;
using MauiDay.Core.Sessionize;
using MauiDay.Core.Validation;

namespace MauiDay.App.Tests;

public sealed class ResilienceHardeningTests
{
    [Fact]
    public void MapperTreatsNullCollectionsAsEmpty()
    {
        var mapper = new SessionizeMapper(new EventTimeService());
        var payload = new SessionizeAllDto
        {
            Sessions = null!,
            Speakers = null!,
            Rooms = null!,
        };

        var result = mapper.Map(payload, FixtureLoader.LoadEventConfiguration());

        Assert.Empty(result.Sessions);
        Assert.Empty(result.Speakers);
        Assert.Empty(result.Rooms);
    }

    [Fact]
    public void MapperSkipsNullCollectionElements()
    {
        var mapper = new SessionizeMapper(new EventTimeService());
        var payload = new SessionizeAllDto
        {
            Sessions = [null!],
            Speakers = [null!],
            Rooms = [null!],
        };

        var result = mapper.Map(payload, FixtureLoader.LoadEventConfiguration());

        Assert.Empty(result.Sessions);
        Assert.Empty(result.Speakers);
        Assert.Empty(result.Rooms);
    }

    [Fact]
    public void MapperToleratesNullSpeakerLinksAndSessions()
    {
        var mapper = new SessionizeMapper(new EventTimeService());
        var payload = new SessionizeAllDto
        {
            Speakers =
            [
                new SessionizeSpeakerDto
                {
                    Id = "speaker-1",
                    FullName = "Test Speaker",
                    Links = null!,
                    Sessions = null!,
                },
            ],
        };

        var result = mapper.Map(payload, FixtureLoader.LoadEventConfiguration());

        var speaker = Assert.Single(result.Speakers);
        Assert.Empty(speaker.Links);
        Assert.Empty(speaker.SessionIds);
    }

    [Fact]
    public void BootstrapWithNullEventsIsRejectedGracefully()
    {
        var bootstrap = new AppBootstrap
        {
            SchemaVersion = AppBootstrap.CurrentSchemaVersion,
            ActiveEventId = "cologne-2026",
            Events = null!,
        };

        Assert.Throws<ConfigurationValidationException>(
            () => ConfigurationValidator.Validate(bootstrap));
    }

    [Theory]
    [InlineData("sessionize")]
    [InlineData("venue")]
    [InlineData("links")]
    [InlineData("organizers")]
    [InlineData("socialLinks")]
    [InlineData("partners")]
    [InlineData("brand")]
    [InlineData("sessionOverrides")]
    public void EventConfigWithNullRequiredSectionIsRejectedGracefully(string property)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "config/events/cologne-2026.json");
        var node = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        node[property] = null;

        var config = JsonSerializer.Deserialize<EventConfiguration>(
            node.ToJsonString(),
            MauiDayJson.Options)!;

        Assert.Throws<ConfigurationValidationException>(
            () => ConfigurationValidator.Validate(config, "cologne-2026"));
    }
}
