using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiDay.App.Services;
using MauiDay.Core.Configuration;

namespace MauiDay.App.ViewModels;

public sealed partial class PartnersViewModel(
    IAppDataService dataService,
    IExternalLauncher launcher) : BaseViewModel
{
    [ObservableProperty]
    private IReadOnlyList<PartnerCardModel> _sponsors = [];

    [ObservableProperty]
    private IReadOnlyList<PartnerCardModel> _supporters = [];

    public bool HasSponsors => Sponsors.Count > 0;

    public bool HasSupporters => Supporters.Count > 0;

    public async Task LoadAsync()
    {
        var snapshot = await GetSnapshotAsync(dataService);
        if (snapshot is null)
        {
            return;
        }

        Sponsors = snapshot.Event.Partners
            .Where(partner => partner.Tier == PartnerTier.Sponsor)
            .Select(PartnerCardModel.Create)
            .ToArray();
        Supporters = snapshot.Event.Partners
            .Where(partner => partner.Tier == PartnerTier.Supporter)
            .Select(PartnerCardModel.Create)
            .ToArray();
        StatusMessage = snapshot.Notice;
        OnPropertyChanged(nameof(HasSponsors));
        OnPropertyChanged(nameof(HasSupporters));
    }

    [RelayCommand]
    private Task OpenPartnerAsync(PartnerCardModel? partner) =>
        partner is null
            ? Task.CompletedTask
            : RunExternalActionAsync(() => launcher.OpenBrowserAsync(partner.WebsiteUrl));
}
