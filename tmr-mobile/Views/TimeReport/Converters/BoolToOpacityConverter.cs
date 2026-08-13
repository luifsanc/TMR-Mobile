using System.Globalization;
using Microsoft.Maui.Controls;

namespace tmr_mobile.Views.TimeReport.Converters
{
    public class BoolToOpacityConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => (value is bool b && b) ? 1.0 : 0.35;

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}