# AGENTS.md — MAUI Day companion app

Guidance for AI agents (and humans) working in this repo. Read this first, then dive
into the specific files it points at. Keep it up to date when architecture or
conventions change.

## What this is

A **.NET MAUI** companion app for **MAUI Day**, a single-day / single-track technical
conference about .NET MAUI. The event data is powered by **Sessionize**. The app is
branded after mauiday.net and currently targets the **Cologne 2026** instance, but is
built to be re-pointed at other MAUI Day instances via config (see *Data flow*).

Core features: Today/overview, full Schedule, Speakers, session ⇄ speaker
cross-navigation with detail screens, venue info, partners/sponsors, and event info
(code of conduct, about). **Deliberately shared-code first** — minimize platform-specific
code.

## Tech stack & versions

- **.NET 11 Preview 6**, SDK pinned in `global.json` to `11.0.100-preview.6.26359.118`
  (`rollForward: disable`). You must have this exact SDK + the matching MAUI workload.
- **TFMs:** `net11.0-android;net11.0-ios` (app project). iOS build requires **Xcode 26.6**
  (the P6 iOS SDK hard-requires it).
- **UI:** MAUI Shell + MVVM (`CommunityToolkit.Mvvm` 8.4.2).
- **Icons:** `IconFont.Maui.FluentIcons` (registered via `.UseFluentIcons()`), used in XAML
  through `x:Static` glyphs, e.g. `Glyph="{x:Static icons:FluentIconsRegular.Home24}"`.
  **Do not** reintroduce a different icon library or icon converters.
- **XAML:** compiled bindings everywhere (`x:DataType`). `<EnablePreviewFeatures>true</EnablePreviewFeatures>`
  is on so we use **source-generated XAML / C# expressions instead of value converters**.
  Prefer `{Binding}` + XAML C# expressions (e.g. `{!HasSessions}`) over writing new
  `IValueConverter`s.
- **DevFlow:** `Microsoft.Maui.DevFlow.Agent` is referenced **Debug-only** for runtime
  inspection (see *Verifying on device*).

## Project structure

```
src/MauiDay.App/     # MAUI head: Pages/, ViewModels/, Controls/, Services/ (platform-facing), Resources/
src/MauiDay.Core/    # Shared, platform-agnostic logic — no MAUI UI dependency
  Configuration/     # EventConfiguration, AppBootstrap (bootstrap + event config models)
  Models/            # ConferenceData, AppDataSnapshot (domain models the UI binds to)
  Serialization/     # MauiDayJson (global STJ options), StrictJsonStringEnumConverter
  Services/          # SessionizeMapper, EventTimeService, TodayStateCalculator, RefreshNoticeBuilder
  Sessionize/        # SessionizeDtos (raw Sessionize payload DTOs)
  Validation/        # ConfigurationValidator
tests/MauiDay.App.Tests/   # xUnit tests over Core logic (fixtures via FixtureLoader)
config/              # Event config + bundled data served from raw.githubusercontent (see Data flow)
.github/workflows/   # ci.yml (build+test); publishing workflows land via a separate PR
```

**Rule of thumb:** put testable logic in `MauiDay.Core` (no MAUI dependency, covered by
unit tests); keep `MauiDay.App` thin (XAML, view models, DI wiring, platform services).

## Architecture

- **DI + composition** is in `src/MauiDay.App/MauiProgram.cs`. Note the lifetimes:
  - Content pages/VMs (`Today`, `Schedule`, `Speakers`, `Info`) are **singletons** and
    subscribe to data updates.
  - Detail/sub pages/VMs (`SessionDetail`, `SpeakerDetail`, `Venue`, `Partners`,
    `CodeOfConduct`, `About`) are **transient** and reload fresh in `OnAppearing`.
    Only singletons subscribe to update messages — keep it that way to avoid leaks.
- **Navigation:** Shell `TabBar` in `AppShell.xaml` (Today/Schedule/Speakers/Info).
  Detail navigation is via routes (e.g. `SessionDetailPage?sessionId=...`). Session ⇄
  speaker cross-links carry a source id so "back" pops to the originating entity.
- **Data service:** `AppDataService` (App) implements `IAppDataService`, uses
  `IHttpClientFactory` (named client `"mauiday"`), does its own per-request timeout +
  retry, caches via `IAppStorage`, and falls back to bundled assets when offline. It
  publishes an immutable `AppDataSnapshot` the view models observe.
- **Time:** all event times go through `IEventTimeService` (`EventTimeService`) so the
  app shows **event-local time** (Cologne, Europe/Berlin) regardless of device zone.
  Don't format Sessionize times directly — normalize through this service.
- **Mapping:** `SessionizeMapper` turns raw Sessionize DTOs into domain models; it filters
  hidden/dropped sessions out of speaker session lists. Enum config values use
  `StrictJsonStringEnumConverter` (rejects integers/unknown strings).

## Data flow (live, not baked-in)

The schedule/speakers come **live from Sessionize** — no app redeploy needed when the
program changes. Chain:

1. `AppDataService` fetches **`config/bootstrap.json`** (from
   `raw.githubusercontent.com/MauiDay/event-app/main/config/bootstrap.json`) → picks the
   `activeEventId` (currently `cologne-2026`).
2. That points at the event config **`config/events/cologne-2026.json`**, which contains
   the Sessionize `AllDataUrl` (live endpoint) plus a `BundledDataAsset` fallback and
   partner/venue metadata.
3. Live Sessionize data is fetched, mapped, and cached; on failure it falls back to cache,
   then to the bundled asset.

To change what the app shows, edit the JSON under `config/` on `main` (validated by the
schemas in `config/schema/`) — **do not** hardcode event data in the app. Sessionize
identifier for the source event is `o0aj9rpg`.

## Build, test, run

```bash
# Restore workloads (once, matches global.json)
dotnet workload restore

# Unit tests (fast, no device needed)
dotnet test tests/MauiDay.App.Tests/MauiDay.App.Tests.csproj

# iOS (simulator) debug build
dotnet build src/MauiDay.App/MauiDay.App.csproj -c Debug -f net11.0-ios -p:RuntimeIdentifier=iossimulator-arm64

# Android debug build
dotnet build src/MauiDay.App/MauiDay.App.csproj -c Debug -f net11.0-android
```

**Release-build gotchas** (CI hit these — keep them in mind):
- The app needs an explicit `Microsoft.Extensions.Http` reference. In Debug it leaks in
  transitively via the Debug-only DevFlow package; Release excludes DevFlow, so the
  reference must be explicit.
- To build a single TFM without breaking `MauiDay.Core`, use the `CIBuildTfm` MSBuild
  property (passing `-f`/`-p:TargetFrameworks` on the CLI propagates globally into Core).
  See `.github/workflows/ci.yml` for the exact pattern.

## CI / publishing

- **CI** (`.github/workflows/ci.yml`, on `main` + PRs): Android build + unit tests on
  ubuntu; unsigned iOS build on `macos-26` (explicitly `xcode-select`s Xcode 26.6). Keep it
  green.
- **Publishing** workflows (signed iOS→TestFlight, Android→Google Play) are wired entirely
  through GitHub **secrets/variables** — see `docs/publishing.md` for the required list.
  Never hardcode signing material or account identifiers.

## Verifying on device (DevFlow) — important limitation

DevFlow (`maui devflow ...`) can inspect and drive the running app, but on **iOS + .NET 11
P6 there is a known bug: it cannot switch Shell tabs** — `ui tap` on a `Tab` and
`ui navigate "//route"` both return `success: true` but the visible tab never changes
(filed as **dotnet/maui-labs#394**). Practical implications for self-verification:

- `ui navigate "SessionDetailPage?sessionId=..."` (pushed routes) **does** flip the screen.
- To view a non-landing tab, temporarily reorder `AppShell` so it's the first tab,
  rebuild/redeploy, then capture the live screen with
  `xcrun simctl io <UDID> screenshot out.png` (this faithfully captures what's on screen;
  DevFlow's own screenshot/tree can lag or be incomplete). Revert the reorder afterward.
- The `ui tree` traverses the full visual tree (including into CollectionView/BindableLayout).

## Conventions

- **Shared code first** — only add `Platforms/<X>/` code when a feature genuinely needs it.
- Accessibility matters: set `SemanticProperties.Description`/`Hint` on interactive
  elements (cards, tiles). Follow the existing patterns in `Controls/` and `Pages/`.
- Keep the mauiday.net branding (colors in `Resources/Styles/Colors.xaml`, fonts Inter +
  Poppins).
- Add/adjust **unit tests in `MauiDay.Core`** for any logic change; keep the suite green.
- **Every commit** must include these trailers:
  ```
  Co-authored-by: Copilot App <223556219+Copilot@users.noreply.github.com>
  ```

## Out of scope (do NOT build unless explicitly asked)

These were intentionally deferred: favorites/personal agenda, calendar integration,
localization, embedded maps, multi-track/multi-day support, and in-app search. Don't add
them speculatively — the app is intentionally focused on single-day / single-track.

## How to iterate

Prefer one focused change per session/PR (one branch ≈ one PR). Good candidate tasks:
polishing a specific screen's UI/UX, adding a new Info sub-page, enriching partner/venue
data via `config/`, hardening the data/refresh path, or improving accessibility. Start a
new session per task, point it at this file, and give it the concrete goal + definition of
done.
