using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiDay.App.Services;
using MauiDay.Core.Configuration;
using MauiDay.Core.Models;
using MauiDay.Core.Services;

namespace MauiDay.App.ViewModels;

public sealed partial class SessionDetailViewModel(
    IAppDataService dataService,
    IAppNavigator navigator,
    IEventTimeService eventTimeService) : BaseViewModel
{
    private string? _sessionId;
    private string? _sourceSpeakerId;
    private EventSession? _session;

    [ObservableProperty]
    private string _title = "Session";

    [ObservableProperty]
    private string _description = "Session details are being prepared.";

    [ObservableProperty]
    private string _timeText = string.Empty;

    [ObservableProperty]
    private string _dateText = string.Empty;

    [ObservableProperty]
    private string _durationText = string.Empty;

    [ObservableProperty]
    private string _timeZoneNote = string.Empty;

    [ObservableProperty]
    private string? _roomText;

    [ObservableProperty]
    private string? _sessionStatus;

    [ObservableProperty]
    private IReadOnlyList<SpeakerCardModel> _speakers = [];

    public bool ShowRoom => !string.IsNullOrWhiteSpace(RoomText);

    public bool HasSessionStatus => !string.IsNullOrWhiteSpace(SessionStatus);

    public bool HasSpeakers => Speakers.Count > 0;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        _sessionId = query.TryGetValue("sessionId", out var sessionId)
            ? sessionId?.ToString()
            : null;
        _sourceSpeakerId = query.TryGetValue("sourceSpeakerId", out var sourceSpeakerId)
            ? sourceSpeakerId?.ToString()
            : null;
    }

    public async Task LoadAsync()
    {
        var snapshot = await GetSnapshotAsync(dataService);
        if (snapshot is null || string.IsNullOrWhiteSpace(_sessionId))
        {
            StatusMessage = "This session could not be found.";
            return;
        }

        _session = snapshot.Conference.FindSession(_sessionId);
        if (_session is null)
        {
            StatusMessage = "This session is no longer in the published program.";
            return;
        }

        Title = _session.Title;
        Description = string.IsNullOrWhiteSpace(_session.Description)
            ? "A description for this session has not been published yet."
            : _session.Description;
        TimeText = $"{_session.StartsAt:HH:mm} - {_session.EndsAt:HH:mm}";
        DateText = _session.StartsAt.ToString("dddd, d MMMM yyyy");
        TimeZoneNote = eventTimeService.DescribeTimeZone(
            snapshot.Event.TimeZone,
            snapshot.Event.Date,
            snapshot.Event.City);
        DurationText = $"{Math.Max(1, (int)Math.Round(_session.Duration.TotalMinutes))} minutes";
        RoomText = snapshot.Conference.Rooms.Count > 1
            ? snapshot.Conference.FindRoom(_session.RoomId)?.Name
            : null;
        SessionStatus = _session.DisplayStatus switch
        {
            SessionDisplayStatus.Cancelled =>
                _session.StatusNote ?? "This session has been cancelled.",
            SessionDisplayStatus.Rescheduled =>
                _session.StatusNote ?? "This session has been rescheduled.",
            _ => null,
        };
        Speakers = _session.SpeakerIds
            .Select(snapshot.Conference.FindSpeaker)
            .Where(speaker => speaker is not null)
            .Select(speaker => SpeakerCardModel.Create(speaker!))
            .ToArray();
        StatusMessage = snapshot.Notice;
        OnPropertyChanged(nameof(ShowRoom));
        OnPropertyChanged(nameof(HasSessionStatus));
        OnPropertyChanged(nameof(HasSpeakers));
    }

    [RelayCommand]
    private Task OpenSpeakerAsync(SpeakerCardModel? speaker)
    {
        if (speaker is null || _session is null)
        {
            return Task.CompletedTask;
        }

        return speaker.Id == _sourceSpeakerId
            ? navigator.GoBackAsync()
            : navigator.OpenSpeakerAsync(speaker.Id, _session.Id);
    }
}
