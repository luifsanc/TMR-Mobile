namespace tmr_shared.DTOs.Clientes;

/// <summary>
/// DTO de lista para clientes.
/// Endpoint: GET /api/clientes?busqueda=&amp;activo=
/// </summary>
public class ClienteListaResponse
{
    public int Id { get; init; }
    public string TipoIdentificacion { get; init; } = string.Empty;
    public string NumeroIdentificacion { get; init; } = string.Empty;
    public string NombreComercial { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Telefono { get; init; } = string.Empty;
    public bool Activo { get; init; }
}
