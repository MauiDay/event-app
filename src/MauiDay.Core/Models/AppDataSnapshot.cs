using MauiDay.Core.Configuration;

namespace MauiDay.Core.Models;

public enum ContentSource
{
    Bundled,
    Cache,
    Remote,
}

public sealed record AppDataSnapshot(
    EventConfiguration Event,
    ConferenceData Conference,
    ContentSource ConfigurationSource,
    ContentSource ConferenceSource,
    DateTimeOffset LoadedAt,
    string? Notice)
{
    public bool IsOfflineFallback =>
        ConfigurationSource != ContentSource.Remote ||
        ConferenceSource != ContentSource.Remote;
}
