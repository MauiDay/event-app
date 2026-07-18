using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiDay.App.Services;
using MauiDay.Core.Configuration;
using MauiDay.Core.Models;

namespace MauiDay.App.ViewModels;

public sealed partial class InfoViewModel : DataViewModel
{
    private readonly IAppNavigator _navigator;
    private readonly IExternalLauncher _launcher;

    [ObservableProperty]
    private string _venueName = string.Empty;

    [ObservableProperty]
    private string _venueAddress = string.Empty;

    [ObservableProperty]
    private string _eventName = string.Empty;

    [ObservableProperty]
    private string _partnerSummary = string.Empty;

    private AppDataSnapshot? _snapshot;

    public InfoViewModel(
        IAppDataService dataService,
        IAppNavigator navigator,
        IExternalLauncher launcher)
        : base(dataService)
    {
        _navigator = navigator;
        _launcher = launcher;
        SubscribeToUpdates();
    }

    protected override void ApplySnapshot(AppDataSnapshot snapshot)
    {
        _snapshot = snapshot;
        EventName = snapshot.Event.Name;
        VenueName = snapshot.Event.Venue.Name;
        VenueAddress = snapshot.Event.Venue.FormattedAddress;
        var sponsors = snapshot.Event.Partners.Count(
            partner => partner.Tier == PartnerTier.Sponsor);
        var supporters = snapshot.Event.Partners.Count(
            partner => partner.Tier == PartnerTier.Supporter);
        PartnerSummary = $"{sponsors} sponsor · {supporters} supporters";
        StatusMessage = snapshot.Notice;
    }

    [RelayCommand]
    private Task OpenVenueAsync() => _navigator.OpenVenueAsync();

    [RelayCommand]
    private Task OpenPartnersAsync() => _navigator.OpenPartnersAsync();

    [RelayCommand]
    private Task OpenCodeOfConductAsync() => _navigator.OpenCodeOfConductAsync();

    [RelayCommand]
    private Task OpenAboutAsync() => _navigator.OpenAboutAsync();

    [RelayCommand]
    private Task OpenTicketsAsync() =>
        _snapshot is null
            ? Task.CompletedTask
            : RunExternalActionAsync(
                () => _launcher.OpenBrowserAsync(_snapshot.Event.Links.Tickets));

    [RelayCommand]
    private Task OpenWebsiteAsync() =>
        _snapshot is null
            ? Task.CompletedTask
            : RunExternalActionAsync(
                () => _launcher.OpenBrowserAsync(_snapshot.Event.Links.Website));
}
