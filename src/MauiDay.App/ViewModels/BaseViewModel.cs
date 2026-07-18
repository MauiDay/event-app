using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiDay.App.Services;
using MauiDay.Core.Models;
using MauiDay.Core.Validation;

namespace MauiDay.App.ViewModels;

public abstract partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _statusMessage;

    public bool HasStatusMessage => !string.IsNullOrWhiteSpace(StatusMessage);

    partial void OnStatusMessageChanged(string? value) =>
        OnPropertyChanged(nameof(HasStatusMessage));

    protected async Task RunExternalActionAsync(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (FeatureNotSupportedException)
        {
            StatusMessage = "This action is not available on this device.";
        }
        catch (InvalidOperationException)
        {
            StatusMessage = "No compatible app is available for this action.";
        }
    }

    protected async Task<AppDataSnapshot?> GetSnapshotAsync(IAppDataService dataService)
    {
        IsBusy = true;
        try
        {
            await dataService.EnsureInitializedAsync();
            return dataService.Current;
        }
        catch (IOException exception)
        {
            SetLoadFailure(exception);
        }
        catch (UnauthorizedAccessException exception)
        {
            SetLoadFailure(exception);
        }
        catch (JsonException exception)
        {
            SetLoadFailure(exception);
        }
        catch (InvalidDataException exception)
        {
            SetLoadFailure(exception);
        }
        catch (ConfigurationValidationException exception)
        {
            SetLoadFailure(exception);
        }
        finally
        {
            IsBusy = false;
        }

        return null;
    }

    protected void SetLoadFailure(Exception exception)
    {
        System.Diagnostics.Debug.WriteLine(exception);
        StatusMessage = "Event data could not be loaded. Please try again.";
    }
}

public abstract partial class DataViewModel(IAppDataService dataService) : BaseViewModel
{
    protected IAppDataService DataService { get; } = dataService;

    protected void SubscribeToUpdates() =>
        DataService.SnapshotChanged += HandleSnapshotChanged;

    public async Task LoadAsync()
    {
        if (DataService.Current is not null)
        {
            ApplySnapshot(DataService.Current);
            return;
        }

        IsBusy = true;
        try
        {
            await DataService.EnsureInitializedAsync();
            if (DataService.Current is not null)
            {
                ApplySnapshot(DataService.Current);
            }
        }
        catch (IOException exception)
        {
            SetLoadFailure(exception);
        }
        catch (UnauthorizedAccessException exception)
        {
            SetLoadFailure(exception);
        }
        catch (JsonException exception)
        {
            SetLoadFailure(exception);
        }
        catch (InvalidDataException exception)
        {
            SetLoadFailure(exception);
        }
        catch (ConfigurationValidationException exception)
        {
            SetLoadFailure(exception);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsBusy = true;
        try
        {
            await DataService.RefreshAsync();
            if (DataService.Current is not null)
            {
                ApplySnapshot(DataService.Current);
            }
        }
        catch (IOException exception)
        {
            SetRefreshFailure(exception);
        }
        catch (UnauthorizedAccessException exception)
        {
            SetRefreshFailure(exception);
        }
        finally
        {
            IsBusy = false;
        }
    }

    protected abstract void ApplySnapshot(AppDataSnapshot snapshot);

    private void HandleSnapshotChanged(object? sender, AppDataSnapshot snapshot) =>
        ApplySnapshot(snapshot);

    private void SetRefreshFailure(Exception exception)
    {
        System.Diagnostics.Debug.WriteLine(exception);
        StatusMessage = "The latest data could not be fetched. Saved content is still available.";
    }
}
