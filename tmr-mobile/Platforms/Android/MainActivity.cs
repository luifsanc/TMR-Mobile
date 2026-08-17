using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;

namespace tmr_mobile;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
	protected override void OnCreate(Bundle? savedInstanceState)
	{
		base.OnCreate(savedInstanceState);
		ApplySystemBarTheme();

		if (Microsoft.Maui.Controls.Application.Current is { } app)
			app.RequestedThemeChanged += OnRequestedThemeChanged;
	}

	protected override void OnDestroy()
	{
		if (Microsoft.Maui.Controls.Application.Current is { } app)
			app.RequestedThemeChanged -= OnRequestedThemeChanged;

		base.OnDestroy();
	}

	private void OnRequestedThemeChanged(object? sender, AppThemeChangedEventArgs e) =>
		RunOnUiThread(ApplySystemBarTheme);

	private void ApplySystemBarTheme()
	{
		var isDark = Microsoft.Maui.Controls.Application.Current?.RequestedTheme == AppTheme.Dark;
		var background = Android.Graphics.Color.ParseColor(isDark ? "#18191A" : "#F8FAFC");

		Window?.SetStatusBarColor(background);
		Window?.SetNavigationBarColor(background);

		if (Build.VERSION.SdkInt >= BuildVersionCodes.M && Window?.DecorView is { } decorView)
		{
			decorView.SystemUiFlags = isDark
				? SystemUiFlags.Visible
				: SystemUiFlags.LightStatusBar;
		}
	}
}
