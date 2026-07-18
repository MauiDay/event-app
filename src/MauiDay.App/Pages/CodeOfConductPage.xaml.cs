using MauiDay.App.ViewModels;

namespace MauiDay.App.Pages;

public partial class CodeOfConductPage : ContentPage
{
    private readonly CodeOfConductViewModel _viewModel;

    public CodeOfConductPage(CodeOfConductViewModel viewModel)
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
