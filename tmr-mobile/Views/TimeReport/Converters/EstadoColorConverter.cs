using System.Globalization;
using tmr_mobile.Resources.Styles;

namespace tmr_mobile.Views.TimeReport.Converters
{
    public class EstadoToColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var estado = value as string ?? string.Empty;
            return estado switch
            {
                "Cargado" => Color.FromArgb(DesignColors.Primary),
                "Pendiente" => Color.FromArgb(DesignColors.Warning),
                "Error" => Color.FromArgb(DesignColors.Error),
                _ => Color.FromArgb(DesignColors.TextSecondary)
            };
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
