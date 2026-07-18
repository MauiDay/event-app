using Android.App;
using Android.Content.PM;
using AndroidX.Core.View;

namespace MauiDay.App;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
	protected override void OnResume()
	{
		base.OnResume();
		SyncStatusBarIcons();
	}

	// MAUI repaints the status bar per-page (navy on Shell-toolbar pages, light on
	// the toolbar-less Today page) but doesn't always match the icon contrast to
	// it, which can leave dark icons on the navy bar (or invisible ones on light).
	// Read the actual bar color and pick readable icon contrast from its luminance
	// so every screen stays legible regardless of what MAUI painted.
	public void SyncStatusBarIcons()
	{
		if (Window is null)
		{
			return;
		}

#pragma warning disable CA1422 // StatusBarColor is obsolete on API 35+ but still reflects the value MAUI sets.
		var argb = Window.StatusBarColor;
#pragma warning restore CA1422
		var color = new Android.Graphics.Color(argb);
		var luminance = (0.299 * color.R) + (0.587 * color.G) + (0.114 * color.B);
		var lightBackground = luminance > 140;

		var insetsController = WindowCompat.GetInsetsController(Window, Window.DecorView);
		if (insetsController is not null)
		{
			// AppearanceLightStatusBars = light bar => dark icons.
			insetsController.AppearanceLightStatusBars = lightBackground;
		}
	}
}
