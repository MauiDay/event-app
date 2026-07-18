using MauiDay.App.ViewModels;

namespace MauiDay.App.Pages;

public partial class TodayPage : ContentPage
{
    private readonly TodayViewModel _viewModel;
    private readonly IDispatcherTimer _timer;

    public TodayPage(TodayViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        _timer = Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromMinutes(1);
        _timer.Tick += (_, _) => _viewModel.UpdateTimeSensitiveState();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _timer.Start();
        await _viewModel.LoadAsync();
        _viewModel.UpdateTimeSensitiveState();
    }

    protected override void OnDisappearing()
    {
        _timer.Stop();
        base.OnDisappearing();
    }
}
