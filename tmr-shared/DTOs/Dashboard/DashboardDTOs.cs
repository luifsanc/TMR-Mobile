namespace tmr_shared.DTOs.Dashboard;

/// <summary>
/// Métricas principales del dashboard.
/// Endpoint: GET /api/dashboard?rango={mes|trimestre|anio}
/// </summary>
public class DashboardDataResponse
{
    public DashboardMetricasResponse Metricas { get; init; } = new();
    public IEnumerable<DashboardProyectoResumenResponse> ProximosACerrar { get; init; } = [];
    public IEnumerable<DashboardHorasPorProyectoResponse> HorasPorProyecto { get; init; } = [];
    public IEnumerable<DashboardProyectosPorClienteResponse> ProyectosPorCliente { get; init; } = [];
}

public class DashboardMetricasResponse
{
    public int TotalProyectos { get; init; }
    public decimal HorasReportadas { get; init; }
    public int ColaboradoresActivos { get; init; }
    public int ClientesActivos { get; init; }
}

public class DashboardProyectoResumenResponse
{
    public string Codigo { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public string Cliente { get; init; } = string.Empty;
    public string Estado { get; init; } = string.Empty;
    public decimal Horas { get; init; }
    public decimal Presupuesto { get; init; }
    public string? FechaFinPlaneada { get; init; } // string para evitar problema con DateOnly
}

public class DashboardHorasPorProyectoResponse
{
    public int Id { get; init; }
    public string Proyecto { get; init; } = string.Empty;
    public decimal Horas { get; init; }
    public string Codigo { get; init; } = string.Empty;
    public decimal HorasAsignadas { get; init; }
}

public class DashboardProyectosPorClienteResponse
{
    public string Cliente { get; init; } = string.Empty;
    public int ProyectosAsignados { get; init; }
    public decimal Porcentaje { get; init; }
}
