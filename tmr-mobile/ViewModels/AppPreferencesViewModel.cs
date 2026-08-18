using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace tmr_mobile.ViewModels;

public partial class AppPreferencesViewModel : BaseViewModel
{
    private const string ThemeKey = "profile_theme";
    private const string NotificationsKey = "profile_notifications";
    private const string DateFormatKey = "profile_date_format";
    private const string TimeFormatKey = "profile_time_format";

    public IReadOnlyList<string> Themes { get; } = ["Usar tema del dispositivo", "Claro", "Oscuro"];
    public IReadOnlyList<string> DateFormats { get; } = ["DD/MM/AAAA", "MM/DD/AAAA", "AAAA-MM-DD"];
    public IReadOnlyList<string> TimeFormats { get; } = ["24 horas", "12 horas"];

    [ObservableProperty] public partial string SelectedTheme { get; set; }
    [ObservableProperty] public partial bool NotificationsEnabled { get; set; }
    [ObservableProperty] public partial string SelectedDateFormat { get; set; }
    [ObservableProperty] public partial string SelectedTimeFormat { get; set; }

    public AppPreferencesViewModel()
    {
        Title = "Configuración";
        SelectedTheme = Preferences.Default.Get(ThemeKey, Themes[0]);
        NotificationsEnabled = Preferences.Default.Get(NotificationsKey, true);
        SelectedDateFormat = Preferences.Default.Get(DateFormatKey, DateFormats[0]);
        SelectedTimeFormat = Preferences.Default.Get(TimeFormatKey, TimeFormats[0]);
    }

    partial void OnSelectedThemeChanged(string value)
    {
        Preferences.Default.Set(ThemeKey, value);
        if (Application.Current is not null)
            Application.Current.UserAppTheme = ToAppTheme(value);
    }

    partial void OnNotificationsEnabledChanged(bool value) => Preferences.Default.Set(NotificationsKey, value);
    partial void OnSelectedDateFormatChanged(string value) => Preferences.Default.Set(DateFormatKey, value);
    partial void OnSelectedTimeFormatChanged(string value) => Preferences.Default.Set(TimeFormatKey, value);

    public static AppTheme GetSavedTheme() =>
        ToAppTheme(Preferences.Default.Get(ThemeKey, "Usar tema del dispositivo"));

    private static AppTheme ToAppTheme(string value) => value switch
    {
        "Claro" => AppTheme.Light,
        "Oscuro" => AppTheme.Dark,
        _ => AppTheme.Unspecified
    };

    [RelayCommand]
    private Task GoBackAsync() => Shell.Current.GoToAsync("..");
}
