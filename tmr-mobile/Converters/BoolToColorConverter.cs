using System.Globalization;

namespace tmr_mobile.Converters;

public class BoolToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue && parameter is string colors)
        {
            var colorParts = colors.Split('|');
            if (colorParts.Length == 2)
            {
                var colorString = boolValue ? colorParts[0] : colorParts[1];

                // Si viene en formato "True:#EEF4FF" o "False:#FFFFFF", extraer la parte del color
                if (colorString.Contains(':'))
                {
                    colorString = colorString.Split(':')[1];
                }

                try
                {
                    return Color.FromArgb(colorString.Trim());
                }
                catch
                {
                    return Colors.Transparent;
                }
            }
        }
        return Colors.Transparent;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
