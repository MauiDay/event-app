namespace MauiDay.App.Services;

public interface IAppNavigator
{
    Task GoToScheduleAsync();

    Task GoToSpeakersAsync();

    Task GoToInfoAsync();

    Task OpenSessionAsync(string sessionId, string? sourceSpeakerId = null);

    Task OpenSpeakerAsync(string speakerId, string? sourceSessionId = null);

    Task OpenVenueAsync();

    Task OpenEditionAsync();

    Task OpenPartnersAsync();

    Task OpenCodeOfConductAsync();

    Task OpenAboutAsync();

    Task GoBackAsync();
}
