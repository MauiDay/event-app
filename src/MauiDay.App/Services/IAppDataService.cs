using MauiDay.Core.Models;

namespace MauiDay.App.Services;

public interface IAppDataService
{
    AppDataSnapshot? Current { get; }

    bool IsRefreshing { get; }

    event EventHandler<AppDataSnapshot>? SnapshotChanged;

    Task EnsureInitializedAsync(CancellationToken cancellationToken = default);

    Task RefreshAsync(CancellationToken cancellationToken = default);
}
