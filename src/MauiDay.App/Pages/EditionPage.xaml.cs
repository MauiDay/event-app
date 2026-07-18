using MauiDay.App.ViewModels;

namespace MauiDay.App.Pages;

public partial class EditionPage : ContentPage
{
    private readonly EditionViewModel _viewModel;

    public EditionPage(EditionViewModel viewModel)
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
