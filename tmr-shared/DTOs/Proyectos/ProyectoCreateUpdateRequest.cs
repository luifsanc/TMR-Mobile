namespace tmr_shared.DTOs.Proyectos;

/// <summary>
/// Request para crear o actualizar un proyecto
/// POST /api/proyectos o PUT /api/proyectos/{id}
/// </summary>
public record ProyectoCreateUpdateRequest
{
    /// <summary>
    /// Código único del proyecto (ej: PROJ-001)
    /// </summary>
    public string Codigo { get; init; } = string.Empty;

    /// <summary>
    /// Nombre o título del proyecto
    /// </summary>
    public string Nombre { get; init; } = string.Empty;

    /// <summary>
    /// ID del cliente
    /// </summary>
    public int IdCliente { get; init; }

    /// <summary>
    /// ID del estado
    /// </summary>
    public int IdEstado { get; init; }

    /// <summary>
    /// Presupuesto asignado
    /// </summary>
    public decimal? Presupuesto { get; init; }

    /// <summary>
    /// Horas totales estimadas
    /// </summary>
    public decimal? Horas { get; init; }

    /// <summary>
    /// ID del líder del proyecto
    /// </summary>
    public int IdLider { get; init; }

    /// <summary>
    /// ID del tipo de proyecto
    /// </summary>
    public int IdTipo { get; init; }

    /// <summary>
    /// Fecha de inicio
    /// </summary>
    public DateOnly? FechaInicio { get; init; }

    /// <summary>
    /// Fecha de finalización
    /// </summary>
    public DateOnly? FechaFin { get; init; }

    /// <summary>
    /// IDs de recursos/colaboradores asignados
    /// </summary>
    public List<int> IdRecursos { get; init; } = new();
}
