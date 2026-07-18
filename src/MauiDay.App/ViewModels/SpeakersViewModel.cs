using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiDay.App.Services;
using MauiDay.Core.Models;

namespace MauiDay.App.ViewModels;

public sealed partial class SpeakersViewModel : DataViewModel
{
    private readonly IAppNavigator _navigator;

    [ObservableProperty]
    private IReadOnlyList<SpeakerCardModel> _speakers = [];

    public bool HasSpeakers => Speakers.Count > 0;

    public SpeakersViewModel(
        IAppDataService dataService,
        IAppNavigator navigator)
        : base(dataService)
    {
        _navigator = navigator;
        SubscribeToUpdates();
    }

    protected override void ApplySnapshot(AppDataSnapshot snapshot)
    {
        Speakers = snapshot.Conference.Speakers
            .Select(SpeakerCardModel.Create)
            .ToArray();
        OnPropertyChanged(nameof(HasSpeakers));
    }

    [RelayCommand]
    private Task OpenSpeakerAsync(SpeakerCardModel? speaker) =>
        speaker is null
            ? Task.CompletedTask
            : _navigator.OpenSpeakerAsync(speaker.Id);
}
