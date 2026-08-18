using Microsoft.Extensions.DependencyInjection;

namespace tmr_mobile;

public partial class App : Application
{
	public App()
	{
		AppDomain.CurrentDomain.UnhandledException += (s, e) =>
			{
				var ex = (Exception)e.ExceptionObject; System.Diagnostics.Debug.WriteLine($"[UNHANDLED] {ex}");
				File.WriteAllText(Path.Combine(FileSystem.AppDataDirectory, "crash.log"), ex.ToString());
			};
		TaskScheduler.UnobservedTaskException += (s, e) =>
			{
				System.Diagnostics.Debug.WriteLine($"[UNOBSERVED TASK] {e.Exception}");
			};
		InitializeComponent();
		UserAppTheme = ViewModels.AppPreferencesViewModel.GetSavedTheme();

	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var window = new Window(new AppShell());

#if WINDOWS
		window.HandlerChanged += (_, _) => UpdateWindowsTitleBar(window);
		RequestedThemeChanged += (_, _) =>
			window.Dispatcher.Dispatch(() => UpdateWindowsTitleBar(window));
#endif

		return window;
	}

#if WINDOWS
	private static void UpdateWindowsTitleBar(Window window)
	{
		if (window.Handler?.PlatformView is not Microsoft.UI.Xaml.Window nativeWindow)
			return;

		try
		{
			var handle = WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);
			var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(handle);
			var titleBar = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId).TitleBar;
			var isDark = Current?.RequestedTheme == AppTheme.Dark;

			var background = isDark
				? global::Windows.UI.Color.FromArgb(255, 36, 37, 38)
				: global::Windows.UI.Color.FromArgb(255, 248, 250, 252);
			var foreground = isDark
				? global::Windows.UI.Color.FromArgb(255, 228, 230, 235)
				: global::Windows.UI.Color.FromArgb(255, 30, 41, 59);
			var hover = isDark
				? global::Windows.UI.Color.FromArgb(255, 62, 64, 66)
				: global::Windows.UI.Color.FromArgb(255, 229, 231, 235);

			titleBar.BackgroundColor = background;
			titleBar.ForegroundColor = foreground;
			titleBar.ButtonBackgroundColor = background;
			titleBar.ButtonForegroundColor = foreground;
			titleBar.ButtonHoverBackgroundColor = hover;
			titleBar.ButtonHoverForegroundColor = foreground;
			titleBar.ButtonPressedBackgroundColor = hover;
			titleBar.ButtonPressedForegroundColor = foreground;
			titleBar.InactiveBackgroundColor = background;
			titleBar.InactiveForegroundColor = foreground;
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"[TITLE BAR] {ex.Message}");
		}
	}
#endif

}
