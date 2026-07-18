using MauiDay.App.ViewModels;

namespace MauiDay.App.Pages;

public partial class SessionDetailPage : ContentPage, IQueryAttributable
{
    private readonly SessionDetailViewModel _viewModel;

    public SessionDetailPage(SessionDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query) =>
        _viewModel.ApplyQueryAttributes(query);

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }
}
