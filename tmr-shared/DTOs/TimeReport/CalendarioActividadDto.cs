namespace tmr_shared.DTOs.TimeReport;

public record CalendarioActividadDto
{
    public int Id { get; init; }
    public string ProyectoNombre { get; init; } = string.Empty;
    public string DescripcionActividad { get; init; } = string.Empty;
    public decimal CantidadHoras { get; init; }
}