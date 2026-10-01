using System.Text.Json;
using MauiDay.Core.Configuration;
using MauiDay.Core.Serialization;
using MauiDay.Core.Sessionize;
using MauiDay.Core.Validation;

namespace MauiDay.App.Tests;

internal static class FixtureLoader
{
    public static EventConfiguration LoadEventConfiguration()
    {
        var bootstrap = Load<AppBootstrap>("config/bootstrap.json");
        return LoadEventConfiguration(bootstrap.ActiveEventId);
    }

    public static EventConfiguration LoadEventConfiguration(string eventId)
    {
        var bootstrap = Load<AppBootstrap>("config/bootstrap.json");
        ConfigurationValidator.Validate(bootstrap);
        var descriptor = bootstrap.Events.Single(item => item.Id == eventId);
        var config = Load<EventConfiguration>(descriptor.BundledConfigAsset);
        ConfigurationValidator.Validate(config, eventId);
        return config;
    }

    public static SessionizeAllDto LoadSessionizeData()
    {
        var config = LoadEventConfiguration();
        return Load<SessionizeAllDto>(config.Sessionize.BundledDataAsset);
    }

    public static T Load<T>(string relativePath)
    {
        var path = Path.Combine(AppContext.BaseDirectory, relativePath);
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<T>(json, MauiDayJson.Options)
            ?? throw new InvalidDataException($"Fixture '{relativePath}' was empty.");
    }
}
