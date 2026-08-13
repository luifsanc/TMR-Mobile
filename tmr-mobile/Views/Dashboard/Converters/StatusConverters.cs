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
            "En riesgo" => Color.FromArgb("#FDECEC"),
            "Pendiente" => Color.FromArgb("#FFF0DE"),
            "Activo" => Color.FromArgb("#E7F7ED"),
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
            "En riesgo" => Color.FromArgb("#D92D20"),
            "Pendiente" => Color.FromArgb("#F58220"),
            "Activo" => Color.FromArgb("#0E9F6E"),
            _ => Color.FromArgb("#667085")
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class ShortNameConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var nombre = value as string ?? string.Empty;
        var partes = nombre.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        if (partes.Length == 0)
            return string.Empty;

        if (partes.Length == 1)
            return partes[0].Substring(0, Math.Min(2, partes[0].Length)).ToUpperInvariant();

        var iniciales = string.Concat(partes.Take(2).Select(p => char.IsLetter(p[0]) ? p[0].ToString() : string.Empty));
        return iniciales.Length > 0 ? iniciales.ToUpperInvariant() : string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
