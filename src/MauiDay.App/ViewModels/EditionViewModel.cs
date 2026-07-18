using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiDay.App.Services;
using MauiDay.Core.Configuration;

namespace MauiDay.App.ViewModels;

public sealed partial class EditionViewModel(
    IAppDataService dataService,
    IExternalLauncher launcher) : BaseViewModel
{
    private VenueConfiguration? _venue;
    private Uri? _ticketsUrl;

    [ObservableProperty]
    private string _editionLabel = "Cologne 2026";

    [ObservableProperty]
    private string _eventName = "MAUI Day";

    [ObservableProperty]
    private string _dateText = string.Empty;

    [ObservableProperty]
    private string _locationText = string.Empty;

    [ObservableProperty]
    private string _venueName = string.Empty;

    [ObservableProperty]
    private string _venueAddress = string.Empty;

    public async Task LoadAsync()
    {
        var snapshot = await GetSnapshotAsync(dataService);
        if (snapshot is null)
        {
            return;
        }

        var @event = snapshot.Event;
        _venue = @event.Venue;
        _ticketsUrl = @event.Links.Tickets;
        EditionLabel = @event.EditionLabel;
        EventName = @event.Name;
        DateText = @event.Date.ToString("dddd, d MMMM yyyy", CultureInfo.CurrentCulture);
        LocationText = $"{@event.City}, {@event.Country}";
        VenueName = _venue.Name;
        VenueAddress = _venue.FormattedAddress;
    }

    [RelayCommand]
    private Task OpenTicketsAsync() =>
        _ticketsUrl is null
            ? Task.CompletedTask
            : RunExternalActionAsync(() => launcher.OpenBrowserAsync(_ticketsUrl));

    [RelayCommand]
    private Task OpenDirectionsAsync() =>
        _venue is null
            ? Task.CompletedTask
            : RunExternalActionAsync(() => launcher.OpenMapAsync(_venue));
}
