namespace tmr_shared.DTOs.Proyectos;

/// <summary>
/// Datos de un proyecto devueltos por GET /api/proyectos.
/// Mantiene el contrato del backend y expone alias usados por la interfaz móvil.
/// </summary>
public record ProyectoResponse
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
    [System.Text.Json.Serialization.JsonConverter(typeof(DdMmYyyyDateOnlyConverter))]
    public DateOnly? FechaInicioReal { get; init; }
    [System.Text.Json.Serialization.JsonConverter(typeof(DdMmYyyyDateOnlyConverter))]
    public DateOnly? FechaFinReal { get; init; }
    [System.Text.Json.Serialization.JsonConverter(typeof(DdMmYyyyDateOnlyConverter))]
    public DateOnly? FechaInicioEspera { get; init; }
    [System.Text.Json.Serialization.JsonConverter(typeof(DdMmYyyyDateOnlyConverter))]
    public DateOnly? FechaFinEspera { get; init; }
    public int? IdLider { get; init; }
    public string Lider { get; init; } = string.Empty;
    public string CargoLider { get; init; } = string.Empty;
    public decimal? CostoHoraLider { get; init; }
    public decimal? HorasLider { get; init; }
    public int IdEstadoProyecto { get; init; }
    public string Estado { get; init; } = string.Empty;
    [System.Text.Json.Serialization.JsonConverter(typeof(DdMmYyyyDateOnlyConverter))]
    public DateOnly? FechaInicio { get; init; }
    [System.Text.Json.Serialization.JsonConverter(typeof(DdMmYyyyDateOnlyConverter))]
    public DateOnly? FechaFin { get; init; }
    public decimal? Presupuesto { get; init; }
    public decimal? Horas { get; init; }
    public int NumeroRecursos { get; init; }
    public bool Activo { get; init; }
    public DateTime FechaCreacion { get; init; }
    public List<ProyectoRecursoResponse> Recursos { get; init; } = new();
    public List<ProyectoLiderResponse> Lideres { get; init; } = new();

    public string LiderAsignado => Lider;
}

public record ProyectoLiderResponse
{
    public int? IdLider { get; init; }
    public string Lider { get; init; } = string.Empty;
    public string CargoLider { get; init; } = string.Empty;
    public decimal? CostoHoraLider { get; init; }
    public decimal? HorasLider { get; init; }
    public List<ProyectoRecursoResponse> Recursos { get; init; } = new();
}

public record ProyectoRecursoResponse
{
    public int Id { get; init; }
    public int? IdEmpleado { get; init; }
    public string Tipo { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public string Rol { get; init; } = string.Empty;
    [System.Text.Json.Serialization.JsonConverter(typeof(DdMmYyyyDateOnlyConverter))]
    public DateOnly? Entrada { get; init; }
    [System.Text.Json.Serialization.JsonConverter(typeof(DdMmYyyyDateOnlyConverter))]
    public DateOnly? Salida { get; init; }
    public decimal? CostoHora { get; init; }
    public decimal? Horas { get; init; }
    public int? IdDepartamento { get; init; }
}
