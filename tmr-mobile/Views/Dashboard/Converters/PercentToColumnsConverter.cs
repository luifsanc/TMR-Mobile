using System.Globalization;

namespace tmr_mobile.Views.Dashboard.Converters;

/// <summary>
/// Convierte un porcentaje (double, 0-100) en un ColumnDefinitionCollection
/// de 2 columnas Star: [porcentaje*, (100-porcentaje)*]. Al ser columnas
/// proporcionales (Star), el resultado es responsive sin necesitar medir
/// el ancho real en píxeles del contenedor.
///
/// Uso en XAML:
///   <Grid ColumnDefinitions="{Binding Porcentaje, Converter={StaticResource PercentToColumns}}">
///       <Border Grid.Column="0" BackgroundColor="{StaticResource Primary}" .../>  <!-- parte llena -->
///       <!-- Grid.Column="1" se deja vacío/transparente: es el resto de la barra -->
///   </Grid>
/// </summary>
public sealed class PercentToColumnsConverter : IValueConverter
{
    // Piso mínimo para que valores muy pequeños (ej. 0.5%) sigan siendo
    // visibles como una barrita, en vez de desaparecer por completo.
    private const double MinVisiblePercent = 3.0;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var percent = value switch
        {
            double d => d,
            decimal m => (double)m,
            int i => i,
            _ => 0d
        };

        if (double.IsNaN(percent) || double.IsInfinity(percent))
            percent = 0;

        percent = Math.Clamp(percent, 0, 100);
        if (percent > 0 && percent < MinVisiblePercent)
            percent = MinVisiblePercent;

        return new ColumnDefinitionCollection
    {
        new ColumnDefinition(new GridLength(percent, GridUnitType.Star)),
        new ColumnDefinition(new GridLength(100 - percent, GridUnitType.Star)),
    };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException($"{nameof(PercentToColumnsConverter)} solo soporta binding de un solo sentido.");
}
