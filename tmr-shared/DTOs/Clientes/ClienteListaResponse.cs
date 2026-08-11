namespace tmr_shared.DTOs.Clientes;

public record ClienteListaResponse
{
    public int Id { get; init; }
    public string NombreComercial { get; init; } = string.Empty;
    public string NumeroIdentificacion { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}