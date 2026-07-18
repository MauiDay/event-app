using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiDay.App.Services;
using MauiDay.Core.Models;

namespace MauiDay.App.ViewModels;

public sealed partial class SpeakerDetailViewModel(
    IAppDataService dataService,
    IAppNavigator navigator,
    IExternalLauncher launcher,
    TimeProvider timeProvider) : BaseViewModel
{
    private string? _speakerId;
    private string? _sourceSessionId;
    private EventSpeaker? _speaker;

    [ObservableProperty]
    private string _fullName = "Speaker";

    [ObservableProperty]
    private string? _tagLine;

    [ObservableProperty]
    private string _bio = "Speaker information is being prepared.";

    [ObservableProperty]
    private Uri? _profilePicture;

    [ObservableProperty]
    private string _initials = string.Empty;

    [ObservableProperty]
    private IReadOnlyList<SpeakerLink> _links = [];

    [ObservableProperty]
    private IReadOnlyList<SessionCardModel> _sessions = [];

    public bool HasProfilePicture => ProfilePicture is not null;

    public bool HasTagLine => !string.IsNullOrWhiteSpace(TagLine);

    public bool HasLinks => Links.Count > 0;

    public bool HasSessions => Sessions.Count > 0;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        _speakerId = query.TryGetValue("speakerId", out var speakerId)
            ? speakerId?.ToString()
            : null;
        _sourceSessionId = query.TryGetValue("sourceSessionId", out var sourceSessionId)
            ? sourceSessionId?.ToString()
            : null;
    }

    public async Task LoadAsync()
    {
        var snapshot = await GetSnapshotAsync(dataService);
        if (snapshot is null || string.IsNullOrWhiteSpace(_speakerId))
        {
            StatusMessage = "This speaker could not be found.";
            return;
        }

        _speaker = snapshot.Conference.FindSpeaker(_speakerId);
        if (_speaker is null)
        {
            StatusMessage = "This speaker is no longer in the published program.";
            return;
        }

        FullName = _speaker.FullName;
        TagLine = _speaker.TagLine;
        Bio = string.IsNullOrWhiteSpace(_speaker.Bio)
            ? "A biography for this speaker has not been published yet."
            : _speaker.Bio;
        ProfilePicture = _speaker.ProfilePicture;
        Initials = _speaker.Initials;
        Links = _speaker.Links;
        Sessions = _speaker.SessionIds
            .Select(snapshot.Conference.FindSession)
            .Where(session => session is not null)
            .OrderBy(session => session!.StartsAt)
            .Select(session => SessionCardModel.Create(
                snapshot,
                session!,
                timeProvider.GetUtcNow()))
            .ToArray();
        OnPropertyChanged(nameof(HasProfilePicture));
        OnPropertyChanged(nameof(HasTagLine));
        OnPropertyChanged(nameof(HasLinks));
        OnPropertyChanged(nameof(HasSessions));
    }

    [RelayCommand]
    private Task OpenSessionAsync(SessionCardModel? session)
    {
        if (session is null || _speaker is null)
        {
            return Task.CompletedTask;
        }

        return session.Id == _sourceSessionId
            ? navigator.GoBackAsync()
            : navigator.OpenSessionAsync(session.Id, _speaker.Id);
    }

    [RelayCommand]
    private Task OpenLinkAsync(SpeakerLink? link) =>
        link is null
            ? Task.CompletedTask
            : RunExternalActionAsync(() => launcher.OpenBrowserAsync(link.Url));
}
