namespace TMR.Shared.DTOs.CargaActividades;
public class ActividadCargadaDto
{
    public string? Colaborador { get; set; }
    public string? Proyecto { get; set; }
    public string? Cliente { get; set; }
    public string? LiderTecnico { get; set; }
    public string? Fecha { get; set; }
    public decimal NroHoras { get; set; }
    public string? Estado { get; set; }
}