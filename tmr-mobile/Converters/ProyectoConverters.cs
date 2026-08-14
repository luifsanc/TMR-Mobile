using System.Globalization;

namespace tmr_mobile.Converters;

/// <summary>
/// Convierte el nombre del líder a solo la inicial
/// </summary>
public class LiderInicialConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string nombre && !string.IsNullOrWhiteSpace(nombre))
        {
            var partes = nombre.Trim().Split(' ');
            return partes.Length > 0 ? partes[0][0].ToString().ToUpper() : "?";
        }
        return "?";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>
/// Convierte el estado a un color de fondo para el badge
/// </summary>
public class EstadoColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string estado)
        {
            return estado?.ToLower() switch
            {
                "activo" or "en progreso" => Color.FromArgb("#DCFCE7"),      // Verde claro
                "inactivo" or "cancelado" => Color.FromArgb("#FEE2E2"),      // Rojo claro
                "completado" => Color.FromArgb("#E0E7FF"),                    // Azul claro
                "en riesgo" => Color.FromArgb("#FEF08A"),                     // Amarillo claro
                _ => Color.FromArgb("#F3F4F6")                                // Gris por defecto
            };
        }
        return Color.FromArgb("#F3F4F6");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>
/// Convierte el estado a un color de texto para el badge
/// </summary>
public class EstadoTextColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string estado)
        {
            return estado?.ToLower() switch
            {
                "activo" or "en progreso" => Color.FromArgb("#166534"),      // Verde oscuro
                "inactivo" or "cancelado" => Color.FromArgb("#991B1B"),      // Rojo oscuro
                "completado" => Color.FromArgb("#3730A3"),                    // Azul oscuro
                "en riesgo" => Color.FromArgb("#854D0E"),                     // Amarillo oscuro
                _ => Color.FromArgb("#374151")                                // Gris por defecto
            };
        }
        return Color.FromArgb("#374151");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
