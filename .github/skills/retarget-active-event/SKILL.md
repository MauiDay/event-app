---
name: retarget-active-event
description: Retarget the MAUI Day companion app to a different event edition without leaving stale city, venue, date, timezone, Sessionize, asset, fixture, screenshot, or documentation references. Use when switching the active event, replacing an edition, adding a MAUI Day city, or reviewing an event migration.
---

# Retarget the active event

Treat an edition change as a repository-wide migration, not only a bootstrap update.

## Establish the migration

1. Read `.github/copilot-instructions.md`, `config/bootstrap.json`, the old event
   configuration, and the new event configuration.
2. Decide from the request whether the old edition must remain available. When replacing
   it, remove its bootstrap descriptor, event configuration, Sessionize fallback, badge,
   screenshots, and tests instead of retaining it as an inactive event.
3. Build an old-edition fingerprint from both configurations and the migration diff:
   city names and localized spellings, event slug, venue and address, country, event date,
   timezone, Sessionize event ID, event URLs, and badge filename.

## Update every surface

1. Set `activeEventId` and descriptors in `config/bootstrap.json`.
2. Validate the new event facts and all referenced files:
   `sessionize.bundledDataAsset` and `brand.bundledEditionBadge`.
3. Keep UI identity configuration-driven. Bind city labels, edition labels, dates, venue
   details, and badge sources from `AppDataSnapshot.Event`; do not hardcode edition values
   in XAML or view-model defaults.
4. Make shared test fixtures resolve the active event and bundled Sessionize path through
   `config/bootstrap.json` and the active event configuration.
5. Update contract expectations, event-time tests, scenario timestamps, session IDs, and
   duration assertions to the new bundled payload.
6. Inspect binary assets and documentation screenshots visually. Text search cannot detect
   an old city name or landmark rendered inside a PNG.
7. Update README and contributor instructions when their active-event examples change.

## Prove the old edition is gone

Search case-insensitively for every old-edition fingerprint, including filenames:

```shell
git grep -inE 'OLD_CITY|LOCALIZED_CITY|OLD_SLUG|OLD_TIME_ZONE|OLD_SESSIONIZE_ID|OLD_BADGE|OLD_VENUE|OLD_EVENT_DATE'
find . -type f | sort | grep -Ei 'OLD_CITY|OLD_SLUG|OLD_BADGE'
```

Classify every match. Do not dismiss test fixtures, inactive descriptors, bundled data,
images, or screenshots as harmless when the request is a replacement.

Then run:

```shell
dotnet test tests/MauiDay.App.Tests/MauiDay.App.Tests.csproj
dotnet build src/MauiDay.App/MauiDay.App.csproj -c Debug -f net11.0-android
```

Finally inspect the changed-file list and diff together. Confirm the visible Today and
edition screens use the configured city and badge, and that deleted edition assets are no
longer packaged by wildcard project includes.
