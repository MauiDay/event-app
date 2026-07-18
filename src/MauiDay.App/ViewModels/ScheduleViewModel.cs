using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiDay.App.Services;
using MauiDay.Core.Configuration;
using MauiDay.Core.Models;

namespace MauiDay.App.ViewModels;

public sealed partial class ScheduleViewModel : DataViewModel
{
    private readonly IAppNavigator _navigator;
    private readonly TimeProvider _timeProvider;

    [ObservableProperty]
    private IReadOnlyList<SessionCardModel> _sessions = [];

    [ObservableProperty]
    private bool _isPreview;

    [ObservableProperty]
    private string _dateText = string.Empty;

    public bool HasSessions => Sessions.Count > 0;

    public ScheduleViewModel(
        IAppDataService dataService,
        IAppNavigator navigator,
        TimeProvider timeProvider)
        : base(dataService)
    {
        _navigator = navigator;
        _timeProvider = timeProvider;
        SubscribeToUpdates();
    }

    protected override void ApplySnapshot(AppDataSnapshot snapshot)
    {
        Sessions = snapshot.Conference.Sessions
            .Select(session => SessionCardModel.Create(
                snapshot,
                session,
                _timeProvider.GetUtcNow()))
            .ToArray();
        IsPreview = snapshot.Event.ScheduleStatus == ScheduleStatus.Preview;
        DateText = snapshot.Event.Date.ToString("dddd, d MMMM yyyy");
        StatusMessage = snapshot.Notice;
        OnPropertyChanged(nameof(HasSessions));
    }

    [RelayCommand]
    private Task OpenSessionAsync(SessionCardModel? session) =>
        session is null || !session.IsNavigable
            ? Task.CompletedTask
            : _navigator.OpenSessionAsync(session.Id);
}
