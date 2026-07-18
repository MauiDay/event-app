using MauiDay.App.Pages;

namespace MauiDay.App;

public partial class AppShell : Shell
{
	public AppShell(
		TodayPage todayPage,
		SchedulePage schedulePage,
		SpeakersPage speakersPage,
		InfoPage infoPage)
	{
		InitializeComponent();
		TodayContent.Content = todayPage;
		ScheduleContent.Content = schedulePage;
		SpeakersContent.Content = speakersPage;
		InfoContent.Content = infoPage;

		Routing.RegisterRoute(nameof(SessionDetailPage), typeof(SessionDetailPage));
		Routing.RegisterRoute(nameof(SpeakerDetailPage), typeof(SpeakerDetailPage));
		Routing.RegisterRoute(nameof(VenuePage), typeof(VenuePage));
		Routing.RegisterRoute(nameof(EditionPage), typeof(EditionPage));
		Routing.RegisterRoute(nameof(PartnersPage), typeof(PartnersPage));
		Routing.RegisterRoute(nameof(CodeOfConductPage), typeof(CodeOfConductPage));
		Routing.RegisterRoute(nameof(AboutPage), typeof(AboutPage));
	}

	protected override void OnNavigated(ShellNavigatedEventArgs args)
	{
		base.OnNavigated(args);
#if ANDROID
		// MAUI repaints the status bar on every navigation; re-sync the icon
		// contrast afterward so it stays readable. Posting (twice, with a small
		// delay) defers past MAUI's own status-bar update on this frame.
		if (Platform.CurrentActivity is MainActivity activity)
		{
			activity.SyncStatusBarIcons();
			var decorView = activity.Window?.DecorView;
			decorView?.Post(activity.SyncStatusBarIcons);
			decorView?.PostDelayed(activity.SyncStatusBarIcons, 120);
		}
#endif
	}
}
