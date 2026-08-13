// tmr_shared/DTOs/CargaActividades/ActividadCargaResponse.cs
namespace tmr_shared.DTOs.CargaActividades;

public class ActividadCargaResponse
{
    public int Id { get; set; }
    public string Colaborador { get; set; } = string.Empty;
    public string Proyecto { get; set; } = string.Empty;
    public string Cliente { get; set; } = string.Empty;
    public string LiderTecnico { get; set; } = string.Empty;
    public DateTime Fecha { get; set; } // "2026-07-06" es ISO 8601, no necesita converter especial
    public decimal NroHoras { get; set; }
    public string Estado { get; set; } = string.Empty;
}