namespace tmr_shared.DTOs.Proyectos;

public record ProyectoResponse
{
    public int Id { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string Cliente { get; init; } = string.Empty;
    public string Estado { get; init; } = string.Empty;
}