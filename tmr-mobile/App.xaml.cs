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
		return new Window(new AppShell());
	}



}
