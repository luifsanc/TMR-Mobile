namespace tmr_shared.DTOs.Proyectos;

/// <summary>
/// DTO de respuesta para proyectos.
/// Endpoint: GET /api/proyectos
/// DateOnly se mapea a string para compatibilidad con System.Text.Json en MAUI.
/// </summary>
public class ProyectoResponse
{
    public int Id { get; init; }
    public string Codigo { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public string Descripcion { get; init; } = string.Empty;
    public int? IdCliente { get; init; }
    public string Cliente { get; init; } = string.Empty;
    public int? IdTipoProyecto { get; init; }
    public string Tipo { get; init; } = string.Empty;
    public string? Observacion { get; init; }
    public string? FechaInicioReal { get; init; }
    public string? FechaFinReal { get; init; }
    public string? FechaInicio { get; init; }
    public string? FechaFin { get; init; }
    public int? IdLider { get; init; }
    public string Lider { get; init; } = string.Empty;
    public int IdEstadoProyecto { get; init; }
    public string Estado { get; init; } = string.Empty;
    public decimal? Presupuesto { get; init; }
    public decimal? Horas { get; init; }
    public int NumeroRecursos { get; init; }
    public bool Activo { get; init; }
    public DateTime FechaCreacion { get; init; }
}
