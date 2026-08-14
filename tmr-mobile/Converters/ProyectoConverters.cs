using System.Globalization;
using tmr_mobile.Resources.Styles;

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
                "activo" or "en progreso" => Color.FromArgb(DesignColors.SuccessSurface),
                "inactivo" or "cancelado" => Color.FromArgb(DesignColors.ErrorSurface),
                "completado" => Color.FromArgb(DesignColors.PrimaryLight),
                "en riesgo" => Color.FromArgb(DesignColors.WarningSurface),
                _ => Color.FromArgb(DesignColors.Secondary)
            };
        }
        return Color.FromArgb(DesignColors.Secondary);
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
                "activo" or "en progreso" => Color.FromArgb(DesignColors.SuccessText),
                "inactivo" or "cancelado" => Color.FromArgb(DesignColors.ErrorText),
                "completado" => Color.FromArgb(DesignColors.Primary),
                "en riesgo" => Color.FromArgb(DesignColors.WarningText),
                _ => Color.FromArgb(DesignColors.TextMain)
            };
        }
        return Color.FromArgb(DesignColors.TextMain);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
