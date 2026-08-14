using tmr_mobile.Resources.Styles;

namespace tmr_mobile.Views.Dashboard.Models;

public class StatCard
{
    public string Icon { get; set; } = string.Empty;      // ej: "icon_projects.png"
    public string IconBackground { get; set; } = DesignColors.PrimaryLight;
    public string IconColor { get; set; } = DesignColors.TextPrimary;
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
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
    public string IconBackground { get; set; } = DesignColors.PrimaryLight;
    public string Descripcion { get; set; } = string.Empty;
    public string Tiempo { get; set; } = string.Empty;   // "hace 2 horas"
}

public class HourDetail
{
    public string Color { get; set; } = DesignColors.Primary;
    public string Proyecto { get; set; } = string.Empty;
    public double Horas { get; set; }
}


public class ClosingProject
{
    public string Codigo { get; set; } = "";
    public string Titulo { get; set; } = "";
    public string Cliente { get; set; } = "";
    public string Estado { get; set; } = "";
    public decimal Horas { get; set; }
    public DateTime? FechaCierre { get; set; }
}


public class ClientShare
{
    public string Cliente { get; set; } = "";
    public int ProyectosAsignados { get; set; }
    public double Porcentaje { get; set; }
}


public class ClientDistribution
{
    public string Cliente { get; set; } = string.Empty;
    public int Proyectos { get; set; }
    public double Porcentaje { get; set; }
}

public class RangoOption
{
    public string Valor { get; set; } = string.Empty;
    public string Texto { get; set; } = string.Empty;
    public bool EsSeleccionado { get; set; }
}
