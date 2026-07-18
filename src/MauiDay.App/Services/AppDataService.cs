using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using MauiDay.Core.Configuration;
using MauiDay.Core.Models;
using MauiDay.Core.Serialization;
using MauiDay.Core.Services;
using MauiDay.Core.Sessionize;
using MauiDay.Core.Validation;
using Microsoft.Extensions.Logging;

namespace MauiDay.App.Services;

public sealed class AppDataService(
    IHttpClientFactory httpClientFactory,
    IAppStorage storage,
    SessionizeMapper sessionizeMapper,
    IDataScenarioProvider scenarioProvider,
    TimeProvider timeProvider,
    ILogger<AppDataService> logger) : IAppDataService
{
    public const string HttpClientName = "mauiday";

    private const string BootstrapAsset = "config/bootstrap.json";
    private const string BootstrapCacheKey = "bootstrap-v1";

    private static readonly Uri BootstrapUrl =
        new("https://raw.githubusercontent.com/MauiDay/event-app/main/config/bootstrap.json");

    private readonly SemaphoreSlim _initializationGate = new(1, 1);
    private readonly SemaphoreSlim _refreshGate = new(1, 1);
    private AppDataSnapshot? _productionCurrent;

    public AppDataSnapshot? Current { get; private set; }

    public bool IsRefreshing { get; private set; }

    public event EventHandler<AppDataSnapshot>? SnapshotChanged;

    public async Task EnsureInitializedAsync(CancellationToken cancellationToken = default)
    {
        if (Current is not null)
        {
            return;
        }

        await _initializationGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (Current is not null)
            {
                return;
            }

            var bootstrap = Deserialize<AppBootstrap>(
                await storage.ReadPackagedTextAsync(BootstrapAsset, cancellationToken)
                    .ConfigureAwait(false));
            ConfigurationValidator.Validate(bootstrap);

            var descriptor = GetActiveDescriptor(bootstrap);
            var eventConfiguration = Deserialize<EventConfiguration>(
                await storage.ReadPackagedTextAsync(
                    descriptor.BundledConfigAsset,
                    cancellationToken).ConfigureAwait(false));
            ConfigurationValidator.Validate(eventConfiguration, descriptor.Id);
            var configurationSource = ContentSource.Bundled;

            var cachedEvent = await TryReadCachedAsync<EventConfiguration>(
                GetEventConfigCacheKey(descriptor.Id),
                value => ConfigurationValidator.Validate(value, descriptor.Id),
                cancellationToken).ConfigureAwait(false);
            if (cachedEvent is not null)
            {
                eventConfiguration = cachedEvent.Value;
                configurationSource = ContentSource.Cache;
            }

            var sessionizePayload = Deserialize<SessionizeAllDto>(
                await storage.ReadPackagedTextAsync(
                    eventConfiguration.Sessionize.BundledDataAsset,
                    cancellationToken).ConfigureAwait(false));
            var conference = sessionizeMapper.Map(sessionizePayload, eventConfiguration);
            var conferenceSource = ContentSource.Bundled;

            var cachedConference = await TryReadCachedAsync<SessionizeAllDto>(
                GetConferenceCacheKey(descriptor.Id),
                value => _ = sessionizeMapper.Map(value, eventConfiguration),
                cancellationToken).ConfigureAwait(false);
            if (cachedConference is not null)
            {
                conference = sessionizeMapper.Map(cachedConference.Value, eventConfiguration);
                conferenceSource = ContentSource.Cache;
            }

            SetCurrent(new AppDataSnapshot(
                eventConfiguration,
                conference,
                configurationSource,
                conferenceSource,
                timeProvider.GetUtcNow(),
                conferenceSource == ContentSource.Cache
                    ? "Saved schedule loaded while updates are checked."
                    : "Schedule preview loaded. Checking for updates."));
        }
        finally
        {
            _initializationGate.Release();
        }

        _ = RunBackgroundRefreshAsync();
    }

    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        if (Current is null)
        {
            await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);
            return;
        }

        await _refreshGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        IsRefreshing = true;
        try
        {
            var baseline = _productionCurrent
                ?? Current
                ?? throw new InvalidOperationException("Event data was not initialized.");
            var bootstrap = await TryLoadRemoteAsync(
                BootstrapUrl,
                BootstrapCacheKey,
                DeserializeAndValidateBootstrap,
                cancellationToken).ConfigureAwait(false);

            var descriptor = bootstrap is null
                ? new EventDescriptor
                {
                    Id = baseline.Event.Id,
                    Name = baseline.Event.Name,
                    ConfigUrl = new Uri(
                        $"https://raw.githubusercontent.com/MauiDay/event-app/main/config/events/{baseline.Event.Id}.json"),
                    BundledConfigAsset = $"config/events/{baseline.Event.Id}.json",
                }
                : GetActiveDescriptor(bootstrap.Value);

            var remoteEvent = await TryLoadRemoteAsync(
                descriptor.ConfigUrl,
                GetEventConfigCacheKey(descriptor.Id),
                content =>
                {
                    var value = Deserialize<EventConfiguration>(content);
                    ConfigurationValidator.Validate(value, descriptor.Id);
                    return value;
                },
                cancellationToken).ConfigureAwait(false);

            var eventConfiguration = remoteEvent?.Value ?? baseline.Event;
            var configurationSource = remoteEvent is null
                ? baseline.ConfigurationSource
                : ContentSource.Remote;
            var configurationRefreshed = remoteEvent is not null;

            var remoteConference = await TryLoadRemoteAsync(
                eventConfiguration.Sessionize.AllDataUrl,
                GetConferenceCacheKey(eventConfiguration.Id),
                content =>
                {
                    var value = Deserialize<SessionizeAllDto>(content);
                    _ = sessionizeMapper.Map(value, eventConfiguration);
                    return value;
                },
                cancellationToken).ConfigureAwait(false);

            var conference = remoteConference is null
                ? baseline.Conference
                : sessionizeMapper.Map(remoteConference.Value, eventConfiguration);
            var conferenceSource = remoteConference is null
                ? baseline.ConferenceSource
                : ContentSource.Remote;
            var conferenceRefreshed = remoteConference is not null;

            // The schedule and the configuration must describe the same event. If we adopted a
            // configuration for a different event but could not load its schedule, publishing the
            // pair would show the previous event's sessions under the new event's identity. Keep
            // the whole baseline instead so the two never drift apart during an event rollover.
            if (remoteConference is null &&
                !string.Equals(eventConfiguration.Id, baseline.Event.Id, StringComparison.Ordinal))
            {
                eventConfiguration = baseline.Event;
                configurationSource = baseline.ConfigurationSource;
                conference = baseline.Conference;
                conferenceSource = baseline.ConferenceSource;
                configurationRefreshed = false;
                conferenceRefreshed = false;
            }

            SetCurrent(new AppDataSnapshot(
                eventConfiguration,
                conference,
                configurationSource,
                conferenceSource,
                timeProvider.GetUtcNow(),
                RefreshNoticeBuilder.Build(
                    conferenceSource,
                    configurationRefreshed,
                    conferenceRefreshed)));
        }
        finally
        {
            IsRefreshing = false;
            _refreshGate.Release();
        }
    }

    private async Task RunBackgroundRefreshAsync()
    {
        try
        {
            await RefreshAsync().ConfigureAwait(false);
        }
        catch (IOException exception)
        {
            logger.LogWarning(exception, "Could not access the content cache during refresh.");
        }
        catch (UnauthorizedAccessException exception)
        {
            logger.LogWarning(exception, "The content cache was not accessible during refresh.");
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "The background content refresh failed unexpectedly; keeping the current data.");
        }
    }

    private async Task<LoadedRemote<T>?> TryLoadRemoteAsync<T>(
        Uri uri,
        string cacheKey,
        Func<string, T> deserializeAndValidate,
        CancellationToken cancellationToken)
    {
        CacheEnvelope? cachedEnvelope = null;
        try
        {
            cachedEnvelope = await storage.ReadCacheAsync(cacheKey, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (JsonException exception)
        {
            logger.LogWarning(exception, "Ignoring malformed cache entry {CacheKey}.", cacheKey);
        }
        catch (IOException exception)
        {
            logger.LogWarning(exception, "Could not read cache entry {CacheKey}.", cacheKey);
        }

        var httpClient = httpClientFactory.CreateClient(HttpClientName);

        for (var attempt = 1; attempt <= 2; attempt++)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, uri);

            if (EntityTagHeaderValue.TryParse(cachedEnvelope?.ETag, out var etag))
            {
                request.Headers.IfNoneMatch.Add(etag);
            }

            if (cachedEnvelope?.LastModified is not null)
            {
                request.Headers.IfModifiedSince = cachedEnvelope.LastModified;
            }

            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(10));

            try
            {
                using var response = await httpClient.SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead,
                    timeout.Token).ConfigureAwait(false);

                if (response.StatusCode == HttpStatusCode.NotModified &&
                    cachedEnvelope is not null)
                {
                    var cachedValue = deserializeAndValidate(cachedEnvelope.Content);
                    return new LoadedRemote<T>(cachedValue, cachedEnvelope);
                }

                if (!response.IsSuccessStatusCode)
                {
                    if (attempt == 1 && IsTransient(response.StatusCode))
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(350), cancellationToken)
                            .ConfigureAwait(false);
                        continue;
                    }

                    logger.LogWarning(
                        "Remote content request to {Uri} returned {StatusCode}.",
                        uri,
                        response.StatusCode);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync(timeout.Token)
                    .ConfigureAwait(false);
                var value = deserializeAndValidate(content);
                var envelope = new CacheEnvelope
                {
                    Content = content,
                    SavedAt = timeProvider.GetUtcNow(),
                    ETag = response.Headers.ETag?.ToString(),
                    LastModified = response.Content.Headers.LastModified,
                };

                try
                {
                    await storage.WriteCacheAsync(cacheKey, envelope, cancellationToken)
                        .ConfigureAwait(false);
                }
                catch (Exception exception)
                    when (exception is IOException or UnauthorizedAccessException)
                {
                    logger.LogWarning(
                        exception,
                        "Could not persist remote content from {Uri}; using the fresh data anyway.",
                        uri);
                }

                return new LoadedRemote<T>(value, envelope);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                if (attempt == 1)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(350), cancellationToken)
                        .ConfigureAwait(false);
                    continue;
                }

                logger.LogWarning("Remote content request to {Uri} timed out.", uri);
                return null;
            }
            catch (HttpRequestException exception) when (attempt == 1)
            {
                logger.LogWarning(
                    exception,
                    "Transient remote content request failure for {Uri}.",
                    uri);
                await Task.Delay(TimeSpan.FromMilliseconds(350), cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (HttpRequestException exception)
            {
                logger.LogWarning(
                    exception,
                    "Remote content request failed for {Uri}.",
                    uri);
                return null;
            }
            catch (JsonException exception)
            {
                logger.LogWarning(exception, "Remote content from {Uri} was malformed.", uri);
                return null;
            }
            catch (ConfigurationValidationException exception)
            {
                logger.LogWarning(exception, "Remote configuration from {Uri} was invalid.", uri);
                return null;
            }
            catch (InvalidDataException exception)
            {
                logger.LogWarning(exception, "Remote event data from {Uri} was invalid.", uri);
                return null;
            }
            catch (FormatException exception)
            {
                logger.LogWarning(exception, "Remote event data from {Uri} was invalid.", uri);
                return null;
            }
            catch (IOException exception)
            {
                logger.LogWarning(exception, "Could not persist remote content from {Uri}.", uri);
                return null;
            }
            catch (UnauthorizedAccessException exception)
            {
                logger.LogWarning(exception, "Could not access the content cache for {Uri}.", uri);
                return null;
            }
        }

        return null;
    }

    private async Task<LoadedCache<T>?> TryReadCachedAsync<T>(
        string cacheKey,
        Action<T> validate,
        CancellationToken cancellationToken)
    {
        try
        {
            var envelope = await storage.ReadCacheAsync(cacheKey, cancellationToken)
                .ConfigureAwait(false);
            if (envelope is null)
            {
                return null;
            }

            var value = Deserialize<T>(envelope.Content);
            validate(value);
            return new LoadedCache<T>(value, envelope);
        }
        catch (JsonException exception)
        {
            logger.LogWarning(exception, "Ignoring malformed cache entry {CacheKey}.", cacheKey);
        }
        catch (ConfigurationValidationException exception)
        {
            logger.LogWarning(exception, "Ignoring invalid cache entry {CacheKey}.", cacheKey);
        }
        catch (InvalidDataException exception)
        {
            logger.LogWarning(exception, "Ignoring invalid event data cache {CacheKey}.", cacheKey);
        }
        catch (FormatException exception)
        {
            logger.LogWarning(exception, "Ignoring invalid event data cache {CacheKey}.", cacheKey);
        }
        catch (IOException exception)
        {
            logger.LogWarning(exception, "Could not read cache entry {CacheKey}.", cacheKey);
        }

        return null;
    }

    private void SetCurrent(AppDataSnapshot snapshot)
    {
        _productionCurrent = snapshot;
        var publishedSnapshot = scenarioProvider.Apply(snapshot);
        Current = publishedSnapshot;
        if (MainThread.IsMainThread)
        {
            SnapshotChanged?.Invoke(this, publishedSnapshot);
            return;
        }

        MainThread.BeginInvokeOnMainThread(
            () => SnapshotChanged?.Invoke(this, publishedSnapshot));
    }

    private static AppBootstrap DeserializeAndValidateBootstrap(string content)
    {
        var value = Deserialize<AppBootstrap>(content);
        ConfigurationValidator.Validate(value);
        return value;
    }

    private static T Deserialize<T>(string content) =>
        JsonSerializer.Deserialize<T>(content, MauiDayJson.Options)
        ?? throw new JsonException($"The {typeof(T).Name} document was empty.");

    private static EventDescriptor GetActiveDescriptor(AppBootstrap bootstrap) =>
        bootstrap.Events.First(
            descriptor => descriptor.Id.Equals(
                bootstrap.ActiveEventId,
                StringComparison.Ordinal));

    private static bool IsTransient(HttpStatusCode statusCode) =>
        statusCode is HttpStatusCode.RequestTimeout or HttpStatusCode.TooManyRequests ||
        (int)statusCode >= 500;

    private static string GetEventConfigCacheKey(string eventId) =>
        $"event-config-v1-{eventId}";

    private static string GetConferenceCacheKey(string eventId) =>
        $"sessionize-v1-{eventId}";

    private sealed record LoadedRemote<T>(T Value, CacheEnvelope Envelope);

    private sealed record LoadedCache<T>(T Value, CacheEnvelope Envelope);
}
