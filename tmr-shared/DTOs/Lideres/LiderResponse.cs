namespace tmr_shared.DTOs.Lideres;

public record LiderResponse
{
    public int Id { get; init; }
    public string NombreCompleto { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Tipopersona { get; init; } = string.Empty;
}