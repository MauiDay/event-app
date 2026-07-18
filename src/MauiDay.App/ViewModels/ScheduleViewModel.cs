using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiDay.App.Services;
using MauiDay.Core.Configuration;
using MauiDay.Core.Models;
using MauiDay.Core.Services;

namespace MauiDay.App.ViewModels;

public sealed partial class ScheduleViewModel : DataViewModel
{
    private readonly IAppNavigator _navigator;
    private readonly TimeProvider _timeProvider;
    private readonly IEventTimeService _eventTimeService;
    private AppDataSnapshot? _snapshot;

    [ObservableProperty]
    private IReadOnlyList<SessionCardModel> _sessions = [];

    [ObservableProperty]
    private string _dateText = string.Empty;

    [ObservableProperty]
    private string _timeZoneLabel = string.Empty;

    public bool HasSessions => Sessions.Count > 0;

    public ScheduleViewModel(
        IAppDataService dataService,
        IAppNavigator navigator,
        TimeProvider timeProvider,
        IEventTimeService eventTimeService)
        : base(dataService)
    {
        _navigator = navigator;
        _timeProvider = timeProvider;
        _eventTimeService = eventTimeService;
        SubscribeToUpdates();
    }

    public void UpdateTimeSensitiveState()
    {
        if (_snapshot is not null)
        {
            ApplySnapshot(_snapshot);
        }
    }

    protected override void ApplySnapshot(AppDataSnapshot snapshot)
    {
        _snapshot = snapshot;
        Sessions = snapshot.Conference.Sessions
            .Select(session => SessionCardModel.Create(
                snapshot,
                session,
                _timeProvider.GetUtcNow()))
            .ToArray();
        DateText = snapshot.Event.Date.ToString("dddd, d MMMM yyyy");
        TimeZoneLabel = _eventTimeService.DescribeTimeZone(
            snapshot.Event.TimeZone,
            snapshot.Event.Date,
            snapshot.Event.City);
        OnPropertyChanged(nameof(HasSessions));
    }

    [RelayCommand]
    private Task OpenSessionAsync(SessionCardModel? session) =>
        session is null || !session.IsNavigable
            ? Task.CompletedTask
            : _navigator.OpenSessionAsync(session.Id);
}
