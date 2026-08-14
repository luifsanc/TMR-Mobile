using System.Globalization;

namespace tmr_mobile.Converters;

public class BoolToEstadoConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolVal)
        {
            return boolVal ? "Activo" : "Inactivo";
        }
        return "Desconocido";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
