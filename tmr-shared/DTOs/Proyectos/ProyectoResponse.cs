namespace tmr_shared.DTOs.Proyectos;

/// <summary>
/// Response del endpoint GET /api/proyectos
/// Contiene toda la información de un proyecto para visualización en lista
/// </summary>
public record ProyectoResponse
{
    /// <summary>
    /// Identificador único del proyecto
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Código único del proyecto (ej: PROJ-001, PRJ-2023-084)
    /// </summary>
    public string Codigo { get; init; } = string.Empty;

    /// <summary>
    /// Nombre o título del proyecto
    /// </summary>
    public string Nombre { get; init; } = string.Empty;

    /// <summary>
    /// Nombre del cliente asociado al proyecto
    /// </summary>
    public string Cliente { get; init; } = string.Empty;

    /// <summary>
    /// Estado del proyecto: "Activo", "Inactivo", "Completado", "En Progreso", "En Riesgo", etc.
    /// </summary>
    public string Estado { get; init; } = string.Empty;

    /// <summary>
    /// Presupuesto asignado al proyecto en USD
    /// </summary>
    public decimal? Presupuesto { get; init; }

    /// <summary>
    /// Horas totales estimadas o registradas para el proyecto
    /// </summary>
    public decimal? Horas { get; init; }

    /// <summary>
    /// Nombre completo del líder/responsable del proyecto
    /// </summary>
    public string Lider { get; init; } = string.Empty;

    /// <summary>
    /// Cantidad de recursos/colaboradores asignados al proyecto
    /// </summary>
    public int NumeroRecursos { get; init; }

    /// <summary>
    /// Tipo de proyecto: "Cloud Migration", "Desarrollo Web", etc.
    /// </summary>
    public string Tipo { get; init; } = string.Empty;

    /// <summary>
    /// Fecha de inicio del proyecto (formato: YYYY-MM-DD)
    /// </summary>
    public DateOnly? FechaInicio { get; init; }

    /// <summary>
    /// Fecha de finalización del proyecto (formato: YYYY-MM-DD)
    /// </summary>
    public DateOnly? FechaFin { get; init; }
}
