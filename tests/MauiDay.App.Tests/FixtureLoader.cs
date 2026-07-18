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
        var config = Load<EventConfiguration>("config/events/cologne-2026.json");
        ConfigurationValidator.Validate(config, "cologne-2026");
        return config;
    }

    public static SessionizeAllDto LoadSessionizeData() =>
        Load<SessionizeAllDto>("config/data/cologne-2026-sessionize-all.json");

    public static T Load<T>(string relativePath)
    {
        var path = Path.Combine(AppContext.BaseDirectory, relativePath);
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<T>(json, MauiDayJson.Options)
            ?? throw new InvalidDataException($"Fixture '{relativePath}' was empty.");
    }
}
