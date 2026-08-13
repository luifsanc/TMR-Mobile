// DTOs/CargaActividadesResponseDto.cs
namespace TMR.Shared.DTOs.CargaActividades;

public class CargaActividadesResponseDto
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public int RegistrosProcesados { get; set; }
    public List<string> ErroresValidacion { get; set; } = new();
    public List<ActividadCargadaDto> Actividades { get; set; } = new();
}