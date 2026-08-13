using System.Globalization;

namespace tmr_mobile.Views.TimeReport.Converters
{
    public class EstadoToColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var estado = value as string ?? string.Empty;
            return estado switch
            {
                "Cargado" => Color.FromArgb("#2563EB"), // azul, como en el mockup
                "Pendiente" => Color.FromArgb("#D97706"), // ámbar
                "Error" => Color.FromArgb("#DC2626"), // rojo
                _ => Color.FromArgb("#667085")  // gris neutro
            };
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}