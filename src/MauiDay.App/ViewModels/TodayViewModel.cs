using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiDay.App.Services;
using MauiDay.Core.Configuration;
using MauiDay.Core.Models;
using MauiDay.Core.Services;

namespace MauiDay.App.ViewModels;

public sealed partial class TodayViewModel : DataViewModel
{
    private readonly IAppNavigator _navigator;
    private readonly TodayStateCalculator _todayStateCalculator;
    private readonly TimeProvider _timeProvider;
    private AppDataSnapshot? _snapshot;

    [ObservableProperty]
    private string _eventName = "MAUI Day Cologne";

    [ObservableProperty]
    private string _editionLabel = "Cologne 2026";

    [ObservableProperty]
    private string _dateText = "23 October 2026";

    [ObservableProperty]
    private string _phaseLabel = "NEXT UP";

    [ObservableProperty]
    private string _phaseTitle = "Cologne is next";

    [ObservableProperty]
    private string _phaseDescription = "The published program is still taking shape.";

    [ObservableProperty]
    private string? _countdownText;

    [ObservableProperty]
    private SessionCardModel? _currentSession;

    [ObservableProperty]
    private SessionCardModel? _nextSession;

    [ObservableProperty]
    private string _venueName = "Microsoft Cologne Office";

    [ObservableProperty]
    private string _locationText = "Holzmarkt 2, 50676 Köln";

    public bool HasCurrentSession => CurrentSession is not null;

    public bool HasNextSession => NextSession is not null;

    public bool HasCountdown => !string.IsNullOrEmpty(CountdownText);

    public string EditionAccessibility =>
        $"{EditionLabel}, {DateText}. Opens edition details and patch pickup.";

    public TodayViewModel(
        IAppDataService dataService,
        IAppNavigator navigator,
        TodayStateCalculator todayStateCalculator,
        TimeProvider timeProvider)
        : base(dataService)
    {
        _navigator = navigator;
        _todayStateCalculator = todayStateCalculator;
        _timeProvider = timeProvider;
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
        var state = _todayStateCalculator.Calculate(
            snapshot.Event,
            snapshot.Conference,
            _timeProvider.GetUtcNow());

        EventName = snapshot.Event.Name;
        EditionLabel = snapshot.Event.EditionLabel;
        DateText = snapshot.Event.Date.ToString("dddd, d MMMM yyyy", CultureInfo.CurrentCulture);
        VenueName = snapshot.Event.Venue.Name;
        LocationText =
            $"{snapshot.Event.Venue.AddressLine1}, {snapshot.Event.Venue.PostalCode} {snapshot.Event.Venue.City}";
        CurrentSession = state.CurrentSession is null
            ? null
            : SessionCardModel.Create(snapshot, state.CurrentSession, _timeProvider.GetUtcNow());
        NextSession = state.NextSession is null
            ? null
            : SessionCardModel.Create(snapshot, state.NextSession, _timeProvider.GetUtcNow());

        (PhaseLabel, PhaseTitle, PhaseDescription, CountdownText) = state.Phase switch
        {
            TodayPhase.PreEvent => (
                "NEXT UP",
                "Cologne is next",
                "One focused day for .NET MAUI builders.",
                FormatCountdown(state.TimeUntilEvent)),
            TodayPhase.EventDayBeforeStart => (
                "TODAY",
                "The room is getting ready",
                "Your first published session is shown below.",
                FormatStartCountdown(state.TimeUntilEvent)),
            TodayPhase.Live => (
                "LIVE NOW",
                "MAUI Day is in progress",
                "Follow the single-track program without missing a beat.",
                null),
            TodayPhase.BetweenSessions => (
                "COMING UP",
                "A moment between sessions",
                "The next published session is ready below.",
                null),
            TodayPhase.EventDayUnscheduled => (
                "TODAY",
                "MAUI Day is here",
                "The detailed program has not been published yet.",
                null),
            TodayPhase.PostEvent => (
                "THAT'S A WRAP",
                "Thanks for joining MAUI Day",
                "Revisit the speakers and sessions from Cologne.",
                null),
            _ => throw new InvalidOperationException("Unknown Today phase."),
        };

        OnPropertyChanged(nameof(HasCurrentSession));
        OnPropertyChanged(nameof(HasNextSession));
        OnPropertyChanged(nameof(HasCountdown));
        OnPropertyChanged(nameof(EditionAccessibility));
    }

    [RelayCommand]
    private Task OpenEditionAsync() => _navigator.OpenEditionAsync();

    [RelayCommand]
    private Task OpenScheduleAsync() => _navigator.GoToScheduleAsync();

    [RelayCommand]
    private Task OpenSpeakersAsync() => _navigator.GoToSpeakersAsync();

    [RelayCommand]
    private Task OpenInfoAsync() => _navigator.GoToInfoAsync();

    [RelayCommand]
    private Task OpenVenueAsync() => _navigator.OpenVenueAsync();

    [RelayCommand]
    private Task OpenPartnersAsync() => _navigator.OpenPartnersAsync();

    [RelayCommand]
    private Task OpenSessionAsync(SessionCardModel? session) =>
        session is null ? Task.CompletedTask : _navigator.OpenSessionAsync(session.Id);

    private static string? FormatCountdown(TimeSpan? timeUntilEvent)
    {
        if (timeUntilEvent is null)
        {
            return null;
        }

        var days = Math.Max(0, (int)Math.Ceiling(timeUntilEvent.Value.TotalDays));
        return days switch
        {
            0 => "It starts today",
            1 => "1 day to go",
            _ => $"{days} days to go",
        };
    }

    private static string? FormatStartCountdown(TimeSpan? timeUntilStart)
    {
        if (timeUntilStart is null)
        {
            return null;
        }

        var minutes = Math.Max(0, (int)Math.Ceiling(timeUntilStart.Value.TotalMinutes));
        if (minutes < 60)
        {
            return $"{minutes} min to the first session";
        }

        var hours = (int)Math.Ceiling(minutes / 60d);
        return $"{hours} h to the first session";
    }
}
