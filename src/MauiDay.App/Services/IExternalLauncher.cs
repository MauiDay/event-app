using MauiDay.Core.Configuration;

namespace MauiDay.App.Services;

public interface IExternalLauncher
{
    Task OpenBrowserAsync(Uri uri);

    Task OpenMapAsync(VenueConfiguration venue);

    Task ComposeEmailAsync(string emailAddress, string subject);
}
