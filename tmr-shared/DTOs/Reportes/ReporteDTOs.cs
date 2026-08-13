namespace tmr_shared.DTOs.Reportes;

public class PaginatedResponse<T>
{
    public List<T> Data { get; set; } = new();
    public int Total { get; set; }
    public int? AnioMinimo { get; set; }
    public int? AnioMaximo { get; set; }
}

public class ReporteHorasResponse
{
    public string Id { get; set; } = string.Empty;
    public string Cliente { get; set; } = string.Empty;
    public int Recursos { get; set; }
    public decimal Horas { get; set; }
    public string Mes { get; set; } = string.Empty;
    public string Anio { get; set; } = string.Empty;
    public string EstadoCliente { get; set; } = string.Empty;

    public string Inicial => !string.IsNullOrWhiteSpace(Cliente) ? Cliente.Substring(0, 1).ToUpper() : "R";
    public string ColorFondoEstado => EstadoCliente == "Activo" ? "#E8F5E9" : "#FFEBEE";
    public string ColorEstado => EstadoCliente == "Activo" ? "#2E7D32" : "#C62828";
}

public class ReporteFechasResponse
{
    public string Id { get; set; } = string.Empty;
    public string Cliente { get; set; } = string.Empty;
    public string Lider { get; set; } = string.Empty;
    public string Recurso { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public string? CodigoProyecto { get; set; }
    public string? Proyecto { get; set; }
    public string? EstadoProyecto { get; set; }
    public string? TipoProyecto { get; set; }
    public DateTime? FechaFinReal { get; set; }
    public decimal? Presupuesto { get; set; }
    public decimal? Horas { get; set; }
    public DateTime? FechaInicioEspera { get; set; }
    public DateTime? FechaFinEspera { get; set; }
    public string? Observaciones { get; set; }

    public string Inicial => !string.IsNullOrWhiteSpace(Recurso) ? Recurso.Substring(0, 1).ToUpper() : "R";
    public string FechaFormateada => FechaInicio.ToString("dd/MM/yyyy");
    public string FechaInicioFormateada => FechaInicio.ToString("dd/MM/yyyy");
    public string FechaFinFormateada => FechaFin.ToString("dd/MM/yyyy");
    public string PeriodoFormateado => $"{FechaInicio:dd/MM/yyyy} - {FechaFin:dd/MM/yyyy}";
}
