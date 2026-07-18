using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiDay.App.Services;
using MauiDay.Core.Configuration;

namespace MauiDay.App.ViewModels;

public sealed partial class AboutViewModel(
    IAppDataService dataService,
    IExternalLauncher launcher) : BaseViewModel
{
    private Uri? _privacyUrl;
    private Uri? _websiteUrl;

    [ObservableProperty]
    private string _versionText = string.Empty;

    [ObservableProperty]
    private IReadOnlyList<ExternalLink> _socialLinks = [];

    public string LicenseSummary =>
        "Poppins and Inter are licensed under the SIL Open Font License 1.1. " +
        "Fluent System Icons are provided by Microsoft under the MIT License. " +
        "Event content is provided by MAUI Day and Sessionize.";

    public async Task LoadAsync()
    {
        var snapshot = await GetSnapshotAsync(dataService);
        if (snapshot is null)
        {
            return;
        }

        VersionText = $"Version {AppInfo.Current.VersionString} ({AppInfo.Current.BuildString})";
        SocialLinks = snapshot.Event.SocialLinks;
        _privacyUrl = snapshot.Event.Links.Privacy;
        _websiteUrl = snapshot.Event.Links.Website;
        StatusMessage = "Made with .NET MAUI for the MAUI Day community.";
    }

    [RelayCommand]
    private Task OpenSocialLinkAsync(ExternalLink? link) =>
        link is null
            ? Task.CompletedTask
            : RunExternalActionAsync(() => launcher.OpenBrowserAsync(link.Url));

    [RelayCommand]
    private Task OpenPrivacyAsync() =>
        _privacyUrl is null
            ? Task.CompletedTask
            : RunExternalActionAsync(() => launcher.OpenBrowserAsync(_privacyUrl));

    [RelayCommand]
    private Task OpenWebsiteAsync() =>
        _websiteUrl is null
            ? Task.CompletedTask
            : RunExternalActionAsync(() => launcher.OpenBrowserAsync(_websiteUrl));
}
