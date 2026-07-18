# MAUI Day companion

A .NET MAUI companion for MAUI Day events. The first edition targets
[MAUI Day Cologne 2026](https://mauiday.net/cologne) and provides a time-aware Today
view, a single-track schedule, speaker and session details, venue guidance, partners,
and the Code of Conduct.

The app is built from shared MAUI XAML and C# for iOS and Android. It starts from
bundled, validated event data, refreshes from Sessionize in the background, and keeps
the last valid response for offline use.

## Requirements

- .NET SDK `11.0.100-preview.6.26359.118` (pinned by `global.json`)
- .NET MAUI workload for .NET 11 Preview 6
- Xcode 26.6 for iOS builds
- Android SDK API 37 for Android builds

## Build and test

```shell
dotnet test tests/MauiDay.App.Tests/MauiDay.App.Tests.csproj
dotnet build src/MauiDay.App/MauiDay.App.csproj -f net11.0-ios
dotnet build src/MauiDay.App/MauiDay.App.csproj -f net11.0-android
```

Debug builds include the MAUI DevFlow agent. Release builds do not.

## Event data

`config/bootstrap.json` selects the active event. Each descriptor points to an event
file under `config/events/`; that file defines the date, timezone, Sessionize endpoint,
venue, links, organizers, partners, brand assets, schedule state, and optional session
overrides. The same checked-in files are bundled into the app as offline fallbacks.

To update Cologne:

1. Update `config/events/cologne-2026.json`.
2. Replace `config/data/cologne-2026-sessionize-all.json` with a validated snapshot from
   `https://sessionize.com/api/v2/o0aj9rpg/view/All` when the bundled fallback should
   change.
3. Keep `schemaVersion` compatible with `config/schema/` and run the tests.

To add an event:

1. Add its event configuration and bundled Sessionize snapshot.
2. Add the event descriptor to `config/bootstrap.json`.
3. Set `activeEventId` when the new event should open by default.
4. Add or bundle any event-specific brand asset referenced by the configuration.

Remote documents are accepted only after validation. A failed, partial, or incompatible
refresh never replaces valid bundled or cached data.

## Debug scenarios

Deterministic UI states are available only in Debug builds through the preferences
`mauiday.debug.scenario` and `mauiday.debug.now`. Supported scenarios are `Production`,
`Empty`, `MissingContent`, `LongContent`, `MultipleSpeakers`, `ServiceSessions`, and
`CancelledSession`. `mauiday.debug.now` accepts an ISO 8601 timestamp for testing
pre-event, live, boundary, and post-event states.

## Structure

- `src/MauiDay.App` — MAUI UI, navigation, platform launchers, and data orchestration
- `src/MauiDay.Core` — configuration, Sessionize mapping, event time, and domain logic
- `tests/MauiDay.App.Tests` — deterministic domain and contract tests
- `config` — versioned bootstrap, event configuration, schemas, and bundled data

Event content comes from [MAUI Day](https://mauiday.net) and
[Sessionize](https://sessionize.com). Poppins and Inter use the SIL Open Font License;
Fluent icons are provided by MauiIcons.