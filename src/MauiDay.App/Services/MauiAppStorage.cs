using System.Text.Json;
using MauiDay.Core.Serialization;

namespace MauiDay.App.Services;

public sealed class MauiAppStorage : IAppStorage
{
    private readonly string _cacheDirectory =
        Path.Combine(FileSystem.AppDataDirectory, "content-cache");

    public async Task<string> ReadPackagedTextAsync(
        string assetPath,
        CancellationToken cancellationToken = default)
    {
        await using var stream = await FileSystem.OpenAppPackageFileAsync(assetPath)
            .ConfigureAwait(false);
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<CacheEnvelope?> ReadCacheAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        var path = GetCachePath(key);
        if (!File.Exists(path))
        {
            return null;
        }

        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<CacheEnvelope>(
            stream,
            MauiDayJson.Options,
            cancellationToken).ConfigureAwait(false);
    }

    public async Task WriteCacheAsync(
        string key,
        CacheEnvelope envelope,
        CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_cacheDirectory);
        var path = GetCachePath(key);
        var temporaryPath = $"{path}.{Guid.NewGuid():N}.tmp";

        try
        {
            await using (var stream = File.Create(temporaryPath))
            {
                await JsonSerializer.SerializeAsync(
                    stream,
                    envelope,
                    MauiDayJson.Options,
                    cancellationToken).ConfigureAwait(false);
                await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
            }

            File.Move(temporaryPath, path, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }

    private string GetCachePath(string key)
    {
        var safeName = string.Concat(
            key.Select(character => char.IsLetterOrDigit(character) ? character : '-'));
        return Path.Combine(_cacheDirectory, $"{safeName}.json");
    }
}
