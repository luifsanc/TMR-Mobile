using tmr_mobile.Models.Configuracion;

namespace tmr_mobile.Services;

public class FeriadosService : IFeriadosService
{
    private readonly ApiService _apiService;

    public FeriadosService(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<List<FeriadoItem>> ObtenerFeriadosAsync()
    {
        var endpoint = "configuracion/dias-festivos";
        System.Diagnostics.Debug.WriteLine($"[FERIADOS] Endpoint: {endpoint}");

        try
        {
            var result = await _apiService.GetAsync<List<FeriadoItem>>(endpoint);
            if (result is not null)
                return result;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FERIADOS][ERROR] GET {endpoint}: {ex.Message}");
        }

        // Fallback a ruta alternativa
        try
        {
            var altResult = await _apiService.GetAsync<List<FeriadoItem>>("configuracion/feriados");
            if (altResult is not null)
                return altResult;
        }
        catch { }

        return new List<FeriadoItem>();
    }

    public async Task<FeriadoItem?> ObtenerFeriadoPorIdAsync(int id)
    {
        var endpoint = $"configuracion/dias-festivos/{id}";
        System.Diagnostics.Debug.WriteLine($"[FERIADOS] Endpoint: {endpoint}");

        try
        {
            var result = await _apiService.GetAsync<FeriadoItem>(endpoint);
            if (result is not null)
                return result;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FERIADOS][ERROR] GET {endpoint}: {ex.Message}");
        }

        var todos = await ObtenerFeriadosAsync();
        return todos.FirstOrDefault(f => f.Id == id);
    }

    public async Task<ApiOperationResult> CrearFeriadoAsync(CreateFeriadoRequest request)
    {
        var endpoint = "configuracion/dias-festivos";
        System.Diagnostics.Debug.WriteLine($"[FERIADOS] Endpoint: {endpoint}");
        return await _apiService.PostForResultAsync(endpoint, request);
    }

    public async Task<ApiOperationResult> ActualizarFeriadoAsync(int id, UpdateFeriadoRequest request)
    {
        var endpoint = $"configuracion/dias-festivos/{id}";
        System.Diagnostics.Debug.WriteLine($"[FERIADOS] Endpoint: {endpoint}");
        return await _apiService.PutForResultAsync(endpoint, request);
    }

    public async Task<ApiOperationResult> EliminarFeriadoAsync(int id)
    {
        var endpoint = $"configuracion/dias-festivos/{id}";
        System.Diagnostics.Debug.WriteLine($"[FERIADOS] Endpoint: {endpoint}");
        return await _apiService.DeleteForResultAsync(endpoint);
    }
}
