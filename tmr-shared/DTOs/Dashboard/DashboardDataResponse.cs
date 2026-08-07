using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
 
namespace tmr_shared.DTOs.Dashboard;
 
public record DashboardDataResponse
{
    public DashboardMetricasResponse? Metricas { get; init; }
    public List<ProximoACerrarResponse> ProximosACerrar { get; init; } = new();
    public List<HorasPorProyectoResponse> HorasPorProyecto { get; init; } = new();
    public List<ProyectoPorClienteResponse> ProyectosPorCliente { get; init; } = new();
}
 
public record DashboardMetricasResponse
{
    public int TotalProyectos { get; init; }
    public int ColaboradoresActivos { get; init; }
    public decimal HorasReportadas { get; init; }
    public int ClientesActivos { get; init; }
}
 
public record ProximoACerrarResponse
{
    public string Codigo { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public string Cliente { get; init; } = string.Empty;
    public string Estado { get; init; } = string.Empty;
    public decimal Horas { get; init; }
    public decimal Presupuesto { get; init; }
 
    // El endpoint manda "dd-MM-yyyy" (no ISO 8601) -> requiere converter custom.
    [JsonConverter(typeof(DdMmYyyyDateTimeConverter))]
    public DateTime? FechaFinPlaneada { get; init; }
}
 
public record HorasPorProyectoResponse
{
    public int Id { get; init; }
    public string Proyecto { get; init; } = string.Empty;
    public decimal Horas { get; init; }
    public string Codigo { get; init; } = string.Empty;
    public decimal HorasAsignadas { get; init; }
}
 
public record ProyectoPorClienteResponse
{
    public string Cliente { get; init; } = string.Empty;
    public int ProyectosAsignados { get; init; }
    public decimal Porcentaje { get; init; }
}
 