namespace tmr_shared.DTOs.TimeReport;

/// <summary>
/// DTO de actividad diaria para el calendario.
/// Endpoint: GET /api/time-report/actividades/calendario?idEmpleado=&amp;anio=&amp;mes=
/// DateOnly se mapea a string para compatibilidad con System.Text.Json en MAUI.
/// </summary>
public class CalendarioActividadDto
{
    public int Id { get; init; }
    public int IdEmpleado { get; init; }
    public int? IdProyecto { get; init; }
    public string ProyectoNombre { get; init; } = string.Empty;
    public int IdTipoActividad { get; init; }
    public string TipoActividadNombre { get; init; } = string.Empty;
    public string? CodigoRequerimiento { get; init; }
    public decimal CantidadHoras { get; init; }
    public string FechaActividad { get; init; } = string.Empty; // DateOnly → string
    public string DescripcionActividad { get; init; } = string.Empty;
    public string? Notas { get; init; }
    public bool? EsBillable { get; init; }
}

/// <summary>
/// DTO para crear una actividad.
/// Endpoint: POST /api/time-report/actividades
/// </summary>
public class CrearActividadDto
{
    public int IdEmpleado { get; init; }
    public int? IdProyecto { get; init; }
    public int IdTipoActividad { get; init; }
    public string? CodigoRequerimiento { get; init; }
    public decimal CantidadHoras { get; init; }
    public string FechaActividad { get; init; } = string.Empty; // "yyyy-MM-dd"
    public string DescripcionActividad { get; init; } = string.Empty;
    public string? Notas { get; init; }
    public bool? EsBillable { get; init; }
}

/// <summary>
/// Lookup de proyectos disponibles para el empleado autenticado.
/// Endpoint: GET /api/time-report/actividades/proyectos-disponibles
/// </summary>
public class ProyectoLookupDto
{
    public int Id { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string? Codigo { get; init; }
}

/// <summary>
/// Resumen de horas del empleado.
/// </summary>
public class ResumenHorasDto
{
    public decimal HorasPorRegistrar { get; init; }
    public decimal HorasRegistradas { get; init; }
    public decimal HorasSemana { get; init; }
    public decimal HorasMes { get; init; }
}

/// <summary>
/// DTO para crear un registro genérico.
/// Endpoint: POST /api/time-report
/// </summary>
public class CrearRegistroTiempoRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
}

public class RegistroTiempoResponse
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
}
