using Microsoft.Extensions.DependencyInjection;

using tmr_mobile.Services;

namespace tmr_mobile;

public partial class App : Application
{
	private static readonly TimeSpan SessionRestoreTimeout = TimeSpan.FromSeconds(12);
	private readonly IAuthService _authService;
	private readonly IUserModuleAccessService _moduleAccessService;
	private int _sessionRestored;

	public App(
		IAuthService authService,
		IUserModuleAccessService moduleAccessService)
	{
		_authService = authService;
		_moduleAccessService = moduleAccessService;

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
		// No se crea el Shell con Login como ruta provisional. Mientras se
		// restaura la sesiÃ³n se conserva una vista neutra alineada con el splash.
		var window = new Window(CreateSessionLoadingPage());
		window.Created += OnWindowCreated;

#if WINDOWS
		window.HandlerChanged += (_, _) => UpdateWindowsTitleBar(window);
		RequestedThemeChanged += (_, _) =>
			window.Dispatcher.Dispatch(() => UpdateWindowsTitleBar(window));
#endif

		return window;
	}

	private async void OnWindowCreated(object? sender, EventArgs e)
	{
		if (Interlocked.Exchange(ref _sessionRestored, 1) != 0)
			return;
		if (sender is not Window window)
			return;

		try
		{
			using var restoreCancellation = new CancellationTokenSource(SessionRestoreTimeout);

			if (!await _authService.IsAuthenticatedAsync(restoreCancellation.Token))
			{
				window.Page = new AppShell("LoginPage");
				return;
			}

			var route = await ResolveAuthenticatedRouteAsync(restoreCancellation.Token);
			var shell = new AppShell(route);
			window.Page = shell;

			if (_authService.CurrentUser?.DebeCambiarPassword == true)
			{
				await shell.GoToAsync(
					nameof(Views.Auth.ChangePasswordPage),
					false,
					new Dictionary<string, object> { ["required"] = true });
			}
		}
		catch (OperationCanceledException)
		{
			System.Diagnostics.Debug.WriteLine(
				$"[SESSION RESTORE] Tiempo de espera agotado ({SessionRestoreTimeout.TotalSeconds:0} s).");
			window.Page = new AppShell("LoginPage");
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"[SESSION RESTORE] {ex.Message}");
			window.Page = new AppShell("LoginPage");
		}
	}

	private async Task<string> ResolveAuthenticatedRouteAsync(CancellationToken cancellationToken)
	{
		if (await _moduleAccessService.EsColaboradorAsync(cancellationToken))
			return "ColaboradorDashboardPage";

		var modules = await _moduleAccessService.ObtenerModulosAsync(cancellationToken);
		return modules.Contains("Dashboard")
			? "DashboardPage"
			: modules.Contains("Actividades") || modules.Contains("Time Report")
				? "TimeReportPage"
				: "HomePage";
	}

	private Page CreateSessionLoadingPage()
	{
		var isDark = RequestedTheme == AppTheme.Dark;
		return new ContentPage
		{
			SafeAreaEdges = SafeAreaEdges.All,
			BackgroundColor = Color.FromArgb(isDark ? "#18191A" : "#F8FAFC"),
			Content = new Grid
			{
				Children =
				{
					new Image
					{
						Source = isDark ? "isotipo2.png" : "isotipo1.png",
						WidthRequest = 96,
						HeightRequest = 96,
						Aspect = Aspect.AspectFit,
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.Center
					}
				}
			}
		};
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
