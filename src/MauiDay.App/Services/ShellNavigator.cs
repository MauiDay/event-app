using MauiDay.App.Pages;

namespace MauiDay.App.Services;

public sealed class ShellNavigator : IAppNavigator
{
    public Task GoToScheduleAsync() => Shell.Current.GoToAsync("//schedule");

    public Task GoToSpeakersAsync() => Shell.Current.GoToAsync("//speakers");

    public Task GoToInfoAsync() => Shell.Current.GoToAsync("//info");

    public Task OpenSessionAsync(string sessionId, string? sourceSpeakerId = null)
    {
        var query = $"sessionId={Uri.EscapeDataString(sessionId)}";
        if (!string.IsNullOrWhiteSpace(sourceSpeakerId))
        {
            query += $"&sourceSpeakerId={Uri.EscapeDataString(sourceSpeakerId)}";
        }

        return Shell.Current.GoToAsync($"{nameof(SessionDetailPage)}?{query}");
    }

    public Task OpenSpeakerAsync(string speakerId, string? sourceSessionId = null)
    {
        var query = $"speakerId={Uri.EscapeDataString(speakerId)}";
        if (!string.IsNullOrWhiteSpace(sourceSessionId))
        {
            query += $"&sourceSessionId={Uri.EscapeDataString(sourceSessionId)}";
        }

        return Shell.Current.GoToAsync($"{nameof(SpeakerDetailPage)}?{query}");
    }

    public Task OpenVenueAsync() => Shell.Current.GoToAsync(nameof(VenuePage));

    public Task OpenEditionAsync() => Shell.Current.GoToAsync(nameof(EditionPage));

    public Task OpenPartnersAsync() => Shell.Current.GoToAsync(nameof(PartnersPage));

    public Task OpenCodeOfConductAsync() =>
        Shell.Current.GoToAsync(nameof(CodeOfConductPage));

    public Task OpenAboutAsync() => Shell.Current.GoToAsync(nameof(AboutPage));

    public Task GoBackAsync() => Shell.Current.GoToAsync("..");
}
