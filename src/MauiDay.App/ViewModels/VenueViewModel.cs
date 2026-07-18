using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiDay.App.Services;
using MauiDay.Core.Configuration;

namespace MauiDay.App.ViewModels;

public sealed partial class VenueViewModel(
    IAppDataService dataService,
    IExternalLauncher launcher) : BaseViewModel
{
    private VenueConfiguration? _venue;

    [ObservableProperty]
    private string _name = "Venue";

    [ObservableProperty]
    private string _address = string.Empty;

    [ObservableProperty]
    private string? _capacityText;

    [ObservableProperty]
    private string? _transit;

    [ObservableProperty]
    private string? _parking;

    [ObservableProperty]
    private string? _arrivalNotes;

    public bool HasCapacity => !string.IsNullOrWhiteSpace(CapacityText);

    public bool HasTransit => !string.IsNullOrWhiteSpace(Transit);

    public bool HasParking => !string.IsNullOrWhiteSpace(Parking);

    public bool HasArrivalNotes => !string.IsNullOrWhiteSpace(ArrivalNotes);

    public async Task LoadAsync()
    {
        var snapshot = await GetSnapshotAsync(dataService);
        if (snapshot is null)
        {
            return;
        }

        _venue = snapshot.Event.Venue;
        Name = _venue.Name;
        Address = _venue.FormattedAddress;
        CapacityText = _venue.Capacity is null ? null : $"{_venue.Capacity} seats";
        Transit = _venue.Transit;
        Parking = _venue.Parking;
        ArrivalNotes = _venue.ArrivalNotes;
        OnPropertyChanged(nameof(HasCapacity));
        OnPropertyChanged(nameof(HasTransit));
        OnPropertyChanged(nameof(HasParking));
        OnPropertyChanged(nameof(HasArrivalNotes));
    }

    [RelayCommand]
    private Task OpenDirectionsAsync() =>
        _venue is null
            ? Task.CompletedTask
            : RunExternalActionAsync(() => launcher.OpenMapAsync(_venue));
}
