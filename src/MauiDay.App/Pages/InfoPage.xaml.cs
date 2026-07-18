using MauiDay.App.ViewModels;

namespace MauiDay.App.Pages;

public partial class InfoPage : ContentPage
{
    private readonly InfoViewModel _viewModel;

    public InfoPage(InfoViewModel viewModel)
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
