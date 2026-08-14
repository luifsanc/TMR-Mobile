using System.Globalization;

namespace tmr_mobile.Converters;

public class BoolToStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue && parameter is string options)
        {
            var parts = options.Split('|');
            if (parts.Length == 2)
            {
                var val = boolValue ? parts[0] : parts[1];
                if (val.Contains(':'))
                {
                    val = val.Split(':')[1];
                }
                return val.Trim();
            }
        }
        return string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
