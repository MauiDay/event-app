# AGENTS.md

The canonical contributor/agent guide for this repo lives in
**[`.github/copilot-instructions.md`](.github/copilot-instructions.md)** — read it in
full before making changes. It's kept there so it applies to every GitHub Copilot
surface (Chat, code review, IDE, coding agent) as well as AGENTS.md-aware tools.

A few constraints worth having up front (the full detail is in the canonical file):

- **SDK is pinned** in `global.json` to .NET 11 Preview 6 (`11.0.100-preview.6.26359.118`,
  `rollForward: disable`); iOS builds require **Xcode 26.6**. Run `dotnet workload restore`.
- **Shared-code first.** Put testable logic in `MauiDay.Core` (no MAUI dependency, unit
  tested); keep `MauiDay.App` thin. Only add `Platforms/<X>/` code when truly required.
- **Event data is live from Sessionize via `config/`** (bootstrap → event config) — never
  hardcode schedule/speaker data in the app.
- **XAML C# expressions over value converters**; icons via `IconFont.Maui.FluentIcons`.
- **Out of scope unless asked:** favorites, calendar, localization, embedded maps,
  multi-track/multi-day, in-app search.
- Every commit must include the trailer:
  `Co-authored-by: Copilot App <223556219+Copilot@users.noreply.github.com>`

See the canonical file for architecture, build/test commands, CI/publishing, the DevFlow
iOS limitation (dotnet/maui-labs#394), and conventions.
