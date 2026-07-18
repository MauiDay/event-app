using MauiDay.Core.Models;

namespace MauiDay.App.Services;

public interface IDataScenarioProvider
{
    AppDataSnapshot Apply(AppDataSnapshot snapshot);
}
