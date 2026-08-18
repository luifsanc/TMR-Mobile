namespace tmr_mobile.Resources.Styles;

/// <summary>
/// Colores TMR para los valores creados desde C# que no pueden usar StaticResource.
/// Deben mantenerse sincronizados con Colors.xaml.
/// </summary>
public static class DesignColors
{
    private static bool IsDark => Application.Current?.RequestedTheme == AppTheme.Dark;

    private static string Theme(string light, string dark) => IsDark ? dark : light;

    public static string Primary => Theme("#163572", "#5B8DEF");
    public static string PrimaryDark => Theme("#162056", "#4776CC");
    public static string PrimaryLight => Theme("#EEF4FF", "#263A59");
    public static string Secondary => Theme("#F5F5F5", "#3A3B3C");
    public const string Accent = "#4CAF50";
    public const string Warning = "#FF9800";
    public const string Error = "#F44336";
    public const string Danger = "#F43F5E";

    public static string TextPrimary => Theme("#1A1A2E", "#F5F6F7");
    public static string TextMain => Theme("#1E293B", "#E4E6EB");
    public static string TextSecondary => Theme("#6B7280", "#C8CCD2");
    public static string TextMuted => Theme("#5F6B7A", "#B0B3B8");
    public static string TextLight => Theme("#94A3B8", "#8A8D91");
    public static string Border => Theme("#E5E7EB", "#3E4042");
    public static string BackgroundLight => Theme("#F8FAFC", "#18191A");
    public static string Surface => Theme("#FFFFFF", "#242526");
    public static string SurfaceElevated => Theme("#FFFFFF", "#303031");
    public const string White = "#FFFFFF";

    public static string SuccessText => Accent;
    public static string SuccessSurface => Theme("#E6FDEE", "#193A2A");
    public static string WarningText => Warning;
    public static string WarningSurface => Theme("#FFEDD5", "#44331C");
    public static string ErrorText => Error;
    public static string ErrorSurface => Theme("#FEECEC", "#46252A");
    public static string PurpleText => Theme("#9333EA", "#D8B4FE");
    public static string PurpleSurface => Theme("#F3E8FF", "#392B45");

    public static string ResolveLiteral(string color) => color.Trim().ToUpperInvariant() switch
    {
        "#EEF4FF" => PrimaryLight,
        "#FFFFFF" => Surface,
        "#163572" => Primary,
        "#D1D5DB" => Border,
        "#9CA3AF" => TextLight,
        "#374151" => TextMain,
        _ => color
    };
}
