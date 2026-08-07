namespace tmr_shared.DTOs.TimeReport;

public record RegistroTiempoResponse
{
    public int Id { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string Descripcion { get; init; } = string.Empty;
}