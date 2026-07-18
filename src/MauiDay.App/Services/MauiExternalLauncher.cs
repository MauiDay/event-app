using MauiDay.Core.Configuration;

namespace MauiDay.App.Services;

public sealed class MauiExternalLauncher : IExternalLauncher
{
    public Task OpenBrowserAsync(Uri uri) =>
        Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);

    public async Task OpenMapAsync(VenueConfiguration venue)
    {
        var placemark = new Placemark
        {
            Thoroughfare = venue.AddressLine1,
            PostalCode = venue.PostalCode,
            Locality = venue.City,
            CountryName = venue.Country,
        };

        await Map.Default.OpenAsync(
            placemark,
            new MapLaunchOptions { Name = venue.Name });
    }

    public async Task ComposeEmailAsync(string emailAddress, string subject)
    {
        var message = new EmailMessage
        {
            Subject = subject,
            To = [emailAddress],
        };

        await Email.Default.ComposeAsync(message);
    }
}
