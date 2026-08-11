namespace tmr_shared.DTOs.Colaboradores;

/// <summary>
/// DTO de lista para colaboradores.
/// Endpoint: GET /api/colaboradores?busqueda=&amp;activo=
/// </summary>
public class ColaboradorListaResponse
{
    public int Id { get; init; }
    public int IdPersona { get; init; }
    public string CodigoEmpleado { get; init; } = string.Empty;
    public string NumeroIdentificacion { get; init; } = string.Empty;
    public string Asociacion { get; init; } = string.Empty;
    public string NombreCompleto { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Cargo { get; init; } = string.Empty;
    public int NumProyectos { get; init; }
    public bool Activo { get; init; }
}
