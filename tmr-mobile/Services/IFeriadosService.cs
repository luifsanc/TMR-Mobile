using tmr_mobile.Models.Configuracion;

namespace tmr_mobile.Services;

public interface IFeriadosService
{
    Task<List<FeriadoItem>> ObtenerFeriadosAsync();
    Task<FeriadoItem?> ObtenerFeriadoPorIdAsync(int id);
    Task<ApiOperationResult> CrearFeriadoAsync(CreateFeriadoRequest request);
    Task<ApiOperationResult> ActualizarFeriadoAsync(int id, UpdateFeriadoRequest request);
    Task<ApiOperationResult> EliminarFeriadoAsync(int id);
}
