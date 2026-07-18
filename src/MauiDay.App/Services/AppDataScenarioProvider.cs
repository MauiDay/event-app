using MauiDay.Core.Models;
using MauiDay.Core.Services;
using Microsoft.Extensions.Logging;

namespace MauiDay.App.Services;

// In Release builds the debug scenario branch is compiled out, so 'logger'
// is intentionally unused there. Suppress CS9113 rather than dropping the
// dependency, which keeps the Debug diagnostics wiring intact.
#pragma warning disable CS9113
public sealed class AppDataScenarioProvider(
    ILogger<AppDataScenarioProvider> logger) : IDataScenarioProvider
#pragma warning restore CS9113
{
    public const string DebugScenarioPreference = "mauiday.debug.scenario";

    public AppDataSnapshot Apply(AppDataSnapshot snapshot)
    {
#if DEBUG
        var configuredScenario = Preferences.Default.Get(
            DebugScenarioPreference,
            nameof(ConferenceScenario.Production));
        if (!Enum.TryParse<ConferenceScenario>(
                configuredScenario,
                ignoreCase: true,
                out var scenario))
        {
            logger.LogWarning(
                "Ignoring unknown debug scenario {Scenario}.",
                configuredScenario);
            return snapshot;
        }

        if (scenario != ConferenceScenario.Production)
        {
            logger.LogInformation("Applying debug data scenario {Scenario}.", scenario);
        }

        return snapshot with
        {
            Conference = ConferenceScenarioFactory.Apply(snapshot.Conference, scenario),
        };
#else
        return snapshot;
#endif
    }
}
