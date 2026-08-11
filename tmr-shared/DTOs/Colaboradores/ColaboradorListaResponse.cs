namespace tmr_shared.DTOs.Colaboradores;

public record ColaboradorListaResponse
{
    public int Id { get; init; }
    public string NombreCompleto { get; init; } = string.Empty;
    public string Cargo { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}