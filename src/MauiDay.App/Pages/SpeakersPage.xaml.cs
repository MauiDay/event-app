using MauiDay.App.ViewModels;

namespace MauiDay.App.Pages;

public partial class SpeakersPage : ContentPage
{
    private readonly SpeakersViewModel _viewModel;

    public SpeakersPage(SpeakersViewModel viewModel)
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
