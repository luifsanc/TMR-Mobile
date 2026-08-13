using tmr_mobile.Models;

namespace tmr_mobile.Services;

public interface ICargaActividadesService
{
    Task<CargaActividadesUploadResult> SubirExcelAsync(byte[] fileBytes, string fileName);
    Task<List<ActividadCargaItem>> ObtenerActividadesAsync();
}

public sealed class CargaActividadesUploadResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public int RegistrosProcesados { get; set; }
    public List<string> ErroresValidacion { get; set; } = new();
}

public class CargaActividadesService : ICargaActividadesService
{
    private readonly ApiService _apiService;

    public CargaActividadesService(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<CargaActividadesUploadResult> SubirExcelAsync(byte[] fileBytes, string fileName)
    {
        var result = await _apiService.PostFileAsync<CargaActividadesUploadResult>(
            "api/carga-actividades/excel", fileBytes, fileName);

        return result ?? new CargaActividadesUploadResult
        {
            IsSuccess = false,
            Message = "No se pudo conectar con el servidor. Verifica tu conexión e intenta de nuevo."
        };
    }

    public async Task<List<ActividadCargaItem>> ObtenerActividadesAsync()
    {
        var result = await _apiService.GetAsync<List<ActividadCargaItem>>("api/carga-actividades/");
        return result ?? new List<ActividadCargaItem>();
    }
}