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
		Routing.RegisterRoute(nameof(PartnersPage), typeof(PartnersPage));
		Routing.RegisterRoute(nameof(CodeOfConductPage), typeof(CodeOfConductPage));
		Routing.RegisterRoute(nameof(AboutPage), typeof(AboutPage));
	}
}
