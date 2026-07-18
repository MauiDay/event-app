using Android.App;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using AndroidX.Core.View;

namespace MauiDay.App;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
	protected override void OnCreate(Bundle? savedInstanceState)
	{
		base.OnCreate(savedInstanceState);
		ApplyStatusBarAppearance();
	}

	public override void OnConfigurationChanged(Configuration newConfig)
	{
		base.OnConfigurationChanged(newConfig);
		ApplyStatusBarAppearance();
	}

	private void ApplyStatusBarAppearance()
	{
		if (Window is null)
		{
			return;
		}

		WindowCompat.SetDecorFitsSystemWindows(Window, true);

		var isNightMode = (Resources?.Configuration?.UiMode & UiMode.NightMask) == UiMode.NightYes;
		if (!OperatingSystem.IsAndroidVersionAtLeast(35))
		{
			Window.SetStatusBarColor(isNightMode
				? Android.Graphics.Color.ParseColor("#07112B")
				: Android.Graphics.Color.ParseColor("#F7F8FC"));
		}

		var insetsController = WindowCompat.GetInsetsController(Window, Window.DecorView);
		if (insetsController is null)
		{
			return;
		}

		insetsController.AppearanceLightStatusBars = !isNightMode;
	}
}
