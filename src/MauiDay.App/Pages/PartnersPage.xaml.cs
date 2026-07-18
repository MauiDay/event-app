using MauiDay.App.ViewModels;

namespace MauiDay.App.Pages;

public partial class PartnersPage : ContentPage
{
    private readonly PartnersViewModel _viewModel;

    public PartnersPage(PartnersViewModel viewModel)
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
