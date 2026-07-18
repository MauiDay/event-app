using MauiDay.App.Pages;
using MauiDay.App.Services;
using MauiDay.App.ViewModels;
using MauiDay.Core.Services;
using IconFont.Maui.FluentIcons;
using Microsoft.Extensions.Logging;
#if DEBUG
using Microsoft.Maui.DevFlow.Agent;
#endif

namespace MauiDay.App;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("Inter-Variable.ttf", "Inter");
				fonts.AddFont("Poppins-Regular.ttf", "Poppins");
				fonts.AddFont("Poppins-Medium.ttf", "PoppinsMedium");
				fonts.AddFont("Poppins-SemiBold.ttf", "PoppinsSemiBold");
			})
			.UseFluentIcons();

		builder.Services.AddHttpClient(AppDataService.HttpClientName, client =>
		{
			client.Timeout = Timeout.InfiniteTimeSpan;
			client.DefaultRequestHeaders.UserAgent.ParseAdd("MauiDay-Companion/1.0");
		});
		builder.Services.AddSingleton<TimeProvider, AppTimeProvider>();
		builder.Services.AddSingleton<IEventTimeService, EventTimeService>();
		builder.Services.AddSingleton<SessionizeMapper>();
		builder.Services.AddSingleton<TodayStateCalculator>();
		builder.Services.AddSingleton<IAppStorage, MauiAppStorage>();
		builder.Services.AddSingleton<IDataScenarioProvider, AppDataScenarioProvider>();
		builder.Services.AddSingleton<AppDataService>();
		builder.Services.AddSingleton<IAppDataService>(
			services => services.GetRequiredService<AppDataService>());
		builder.Services.AddSingleton<IAppNavigator, ShellNavigator>();
		builder.Services.AddSingleton<IExternalLauncher, MauiExternalLauncher>();

		builder.Services.AddSingleton<TodayViewModel>();
		builder.Services.AddSingleton<ScheduleViewModel>();
		builder.Services.AddSingleton<SpeakersViewModel>();
		builder.Services.AddSingleton<InfoViewModel>();
		builder.Services.AddTransient<SessionDetailViewModel>();
		builder.Services.AddTransient<SpeakerDetailViewModel>();
		builder.Services.AddTransient<VenueViewModel>();
		builder.Services.AddTransient<EditionViewModel>();
		builder.Services.AddTransient<PartnersViewModel>();
		builder.Services.AddTransient<CodeOfConductViewModel>();
		builder.Services.AddTransient<AboutViewModel>();

		builder.Services.AddSingleton<TodayPage>();
		builder.Services.AddSingleton<SchedulePage>();
		builder.Services.AddSingleton<SpeakersPage>();
		builder.Services.AddSingleton<InfoPage>();
		builder.Services.AddTransient<SessionDetailPage>();
		builder.Services.AddTransient<SpeakerDetailPage>();
		builder.Services.AddTransient<VenuePage>();
		builder.Services.AddTransient<EditionPage>();
		builder.Services.AddTransient<PartnersPage>();
		builder.Services.AddTransient<CodeOfConductPage>();
		builder.Services.AddTransient<AboutPage>();
		builder.Services.AddSingleton<AppShell>();

#if DEBUG
		builder.Logging.AddDebug();
		builder.AddMauiDevFlowAgent();
#endif

		return builder.Build();
	}
}
