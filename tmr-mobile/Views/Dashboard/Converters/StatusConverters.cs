using System.Globalization;
using tmr_mobile.Resources.Styles;

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
            "Planificación" or "Planificacion" => Color.FromArgb(DesignColors.PrimaryLight),
            "Completado" => Color.FromArgb(DesignColors.SuccessSurface),
            "En progreso" => Color.FromArgb(DesignColors.PrimaryLight),
            "En riesgo" => Color.FromArgb(DesignColors.ErrorSurface),
            "Pendiente" => Color.FromArgb(DesignColors.WarningSurface),
            "Pausado" or "En pausa" => Color.FromArgb(DesignColors.WarningSurface),
            "Cancelado" => Color.FromArgb(DesignColors.ErrorSurface),
            "Activo" => Color.FromArgb(DesignColors.SuccessSurface),
            _ => Color.FromArgb(DesignColors.Secondary)
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
            "Planificación" or "Planificacion" => Color.FromArgb(DesignColors.Primary),
            "Completado" => Color.FromArgb(DesignColors.SuccessText),
            "En progreso" => Color.FromArgb(DesignColors.Primary),
            "En riesgo" => Color.FromArgb(DesignColors.ErrorText),
            "Pendiente" => Color.FromArgb(DesignColors.WarningText),
            "Pausado" or "En pausa" => Color.FromArgb(DesignColors.WarningText),
            "Cancelado" => Color.FromArgb(DesignColors.ErrorText),
            "Activo" => Color.FromArgb(DesignColors.SuccessText),
            _ => Color.FromArgb(DesignColors.TextSecondary)
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
