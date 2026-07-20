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
    private TodayPhase? _appliedPhase;
    private string? _currentSessionId;
    private string? _nextSessionId;

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
    private string _countdownDays = "0";

    [ObservableProperty]
    private string _countdownHours = "0";

    [ObservableProperty]
    private string _countdownMinutes = "0";

    [ObservableProperty]
    private string _countdownSeconds = "0";

    [ObservableProperty]
    private string _countdownAccessibility = string.Empty;

    private bool _hasCountdown;

    public bool HasCountdown
    {
        get => _hasCountdown;
        private set
        {
            if (_hasCountdown == value)
            {
                return;
            }

            _hasCountdown = value;
            OnPropertyChanged();
        }
    }

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
            Recompute(fullRefresh: false);
        }
    }

    protected override void ApplySnapshot(AppDataSnapshot snapshot)
    {
        _snapshot = snapshot;
        Recompute(fullRefresh: true);
    }

    private void Recompute(bool fullRefresh)
    {
        var snapshot = _snapshot!;
        var now = _timeProvider.GetUtcNow();
        var state = _todayStateCalculator.Calculate(
            snapshot.Event,
            snapshot.Conference,
            now);

        // The countdown ticks every second, so refresh it on every pass.
        UpdateCountdown(state.TimeUntilEvent, snapshot.Event.Name);

        var currentId = state.CurrentSession?.Id;
        var nextId = state.NextSession?.Id;

        // Everything else only changes when the phase or featured sessions change,
        // so avoid rebuilding session cards (and rebinding views) on every tick.
        if (!fullRefresh
            && state.Phase == _appliedPhase
            && currentId == _currentSessionId
            && nextId == _nextSessionId)
        {
            return;
        }

        EventName = snapshot.Event.Name;
        EditionLabel = snapshot.Event.EditionLabel;
        DateText = snapshot.Event.Date.ToString("dddd, d MMMM yyyy", CultureInfo.CurrentCulture);
        VenueName = snapshot.Event.Venue.Name;
        LocationText =
            $"{snapshot.Event.Venue.AddressLine1}, {snapshot.Event.Venue.PostalCode} {snapshot.Event.Venue.City}";
        CurrentSession = state.CurrentSession is null
            ? null
            : SessionCardModel.Create(snapshot, state.CurrentSession, now);
        NextSession = state.NextSession is null
            ? null
            : SessionCardModel.Create(snapshot, state.NextSession, now);

        (PhaseLabel, PhaseTitle, PhaseDescription) = state.Phase switch
        {
            TodayPhase.PreEvent => (
                "NEXT UP",
                "Cologne is next",
                "One focused day for .NET MAUI builders."),
            TodayPhase.EventDayBeforeStart => (
                "TODAY",
                "The room is getting ready",
                "Your first published session is shown below."),
            TodayPhase.Live => (
                "LIVE NOW",
                "MAUI Day is in progress",
                "Follow the single-track program without missing a beat."),
            TodayPhase.BetweenSessions => (
                "COMING UP",
                "A moment between sessions",
                "The next published session is ready below."),
            TodayPhase.EventDayUnscheduled => (
                "TODAY",
                "MAUI Day is here",
                "The detailed program has not been published yet."),
            TodayPhase.PostEvent => (
                "THAT'S A WRAP",
                "Thanks for joining MAUI Day",
                "Revisit the speakers and sessions from Cologne."),
            _ => throw new InvalidOperationException("Unknown Today phase."),
        };

        _appliedPhase = state.Phase;
        _currentSessionId = currentId;
        _nextSessionId = nextId;

        OnPropertyChanged(nameof(HasCurrentSession));
        OnPropertyChanged(nameof(HasNextSession));
        OnPropertyChanged(nameof(EditionAccessibility));
    }

    private void UpdateCountdown(TimeSpan? timeUntilEvent, string eventName)
    {
        if (timeUntilEvent is null || timeUntilEvent.Value <= TimeSpan.Zero)
        {
            HasCountdown = false;
            return;
        }

        var remaining = timeUntilEvent.Value;
        var days = (int)remaining.TotalDays;

        CountdownDays = days.ToString(CultureInfo.CurrentCulture);
        CountdownHours = remaining.Hours.ToString(CultureInfo.CurrentCulture);
        CountdownMinutes = remaining.Minutes.ToString(CultureInfo.CurrentCulture);
        CountdownSeconds = remaining.Seconds.ToString(CultureInfo.CurrentCulture);
        CountdownAccessibility =
            $"{days} days, {remaining.Hours} hours, {remaining.Minutes} minutes and {remaining.Seconds} seconds until {eventName}.";
        HasCountdown = true;
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
}
