namespace tmr_mobile.Views.Dashboard.Models;

public class StatCard
{
    public string Icon { get; set; } = string.Empty;      // ej: "icon_projects.png"
    public string IconBackground { get; set; } = "#E7EEFF";
    public string IconColor { get; set; } = "#101828";
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}

public class ClosingProject
{
    public string Codigo { get; set; } = string.Empty;      // BCN_DOC_PROC
    public string Titulo { get; set; } = string.Empty;      // Documentacion de procesos BCN
    public string Cliente { get; set; } = string.Empty;      // BANCO COOPNACIONAL
    public DateTime FechaCierre { get; set; }
    public string Estado { get; set; } = string.Empty;      // Completado | En progreso | Pendiente
    public int HorasRestantes { get; set; }
}

public class ChartPoint
{
    public string Categoria { get; set; } = string.Empty;   // "Proyectos de Procesos"
    public double Horas { get; set; }

    // Porcentaje (0-100) de Horas respecto al máximo del set actual.
    // Se calcula en el ViewModel al mapear la respuesta del endpoint,
    // porque solo ahí se conoce el máximo de todo el conjunto.
    // Se usa para dibujar la barra proporcional en el XAML vía
    // PercentToColumnsConverter. Default 100 para que si algún día
    // se usa este modelo sin pasar por el mapper, la barra se vea
    // llena en vez de en 0 (comportamiento visual previo).
    public double Porcentaje { get; set; } = 100;
}

public class MetricMini
{
    public string Titulo { get; set; } = string.Empty;   // "TOTAL DE HORAS"
    public string Valor { get; set; } = string.Empty;   // "110 h"
}

public class ActivityItem
{
    public string Icon { get; set; } = string.Empty;
    public string IconBackground { get; set; } = "#F1E9FF";
    public string Descripcion { get; set; } = string.Empty;
    public string Tiempo { get; set; } = string.Empty;   // "hace 2 horas"
}

public class HourDetail
{
    public string Color { get; set; } = "#2E5BFF";
    public string Proyecto { get; set; } = string.Empty;
    public double Horas { get; set; }
}
