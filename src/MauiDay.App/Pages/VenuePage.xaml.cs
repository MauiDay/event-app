using MauiDay.App.ViewModels;

namespace MauiDay.App.Pages;

public partial class VenuePage : ContentPage
{
    private readonly VenueViewModel _viewModel;

    public VenuePage(VenueViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }
}
