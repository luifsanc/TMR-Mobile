namespace tmr_shared.DTOs.Lideres;

/// <summary>
/// DTO de respuesta para líderes.
/// Endpoint: GET /api/lideres
/// </summary>
public class LiderResponse
{
    public int Id { get; init; }
    public string Nombres { get; init; } = string.Empty;
    public string Apellidos { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Telefono { get; init; }
    public string Tipopersona { get; init; } = string.Empty;
    public int? Idtipo { get; init; }
    public string? TipoNombre { get; init; }
    public string NumeroIdentificacion { get; init; } = string.Empty;
    public bool Activo { get; init; }
    public DateTime Fechacreacion { get; init; }

    public string NombreCompleto => $"{Nombres} {Apellidos}".Trim();
}
