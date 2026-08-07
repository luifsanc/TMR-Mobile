using System.Globalization;

namespace tmr_mobile.Views.Dashboard.Converters;

/// <summary>
/// Devuelve el color de fondo del badge según el estado del proyecto.
/// </summary>
public class StatusToBackgroundColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var estado = value as string ?? string.Empty;
        return estado switch
        {
            "Completado" => Color.FromArgb("#DFF6E4"),
            "En progreso" => Color.FromArgb("#E4ECFF"),
            "Pendiente" => Color.FromArgb("#FFF0DE"),
            _ => Color.FromArgb("#F1F1F5")
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Devuelve el color de texto del badge según el estado del proyecto.
/// </summary>
public class StatusToTextColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var estado = value as string ?? string.Empty;
        return estado switch
        {
            "Completado" => Color.FromArgb("#1F9254"),
            "En progreso" => Color.FromArgb("#2E5BFF"),
            "Pendiente" => Color.FromArgb("#F58220"),
            _ => Color.FromArgb("#667085")
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
