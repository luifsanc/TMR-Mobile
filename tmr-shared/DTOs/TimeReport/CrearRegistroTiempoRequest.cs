namespace tmr_shared.DTOs.TimeReport;

public record CrearRegistroTiempoRequest
{
    public string Nombre { get; init; } = string.Empty;
    public string Descripcion { get; init; } = string.Empty;
}