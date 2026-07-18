using System.Globalization;

namespace MauiDay.App.Services;

public sealed class AppTimeProvider : TimeProvider
{
    public const string DebugNowPreference = "mauiday.debug.now";

    public override DateTimeOffset GetUtcNow()
    {
#if DEBUG
        var overrideValue = Preferences.Default.Get(DebugNowPreference, string.Empty);
        if (DateTimeOffset.TryParse(
                overrideValue,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind,
                out var overrideNow))
        {
            return overrideNow.ToUniversalTime();
        }
#endif

        return base.GetUtcNow();
    }
}
