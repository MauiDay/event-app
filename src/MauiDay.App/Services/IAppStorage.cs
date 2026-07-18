namespace MauiDay.App.Services;

public interface IAppStorage
{
    Task<string> ReadPackagedTextAsync(
        string assetPath,
        CancellationToken cancellationToken = default);

    Task<CacheEnvelope?> ReadCacheAsync(
        string key,
        CancellationToken cancellationToken = default);

    Task WriteCacheAsync(
        string key,
        CacheEnvelope envelope,
        CancellationToken cancellationToken = default);
}

public sealed class CacheEnvelope
{
    public required string Content { get; init; }

    public required DateTimeOffset SavedAt { get; init; }

    public string? ETag { get; init; }

    public DateTimeOffset? LastModified { get; init; }
}
