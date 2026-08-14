using tmr_mobile.Models.Configuracion;

namespace tmr_mobile.Services;

public interface IFeriadosService
{
    Task<List<FeriadoItem>> ObtenerFeriadosAsync();
    Task<FeriadoItem?> ObtenerFeriadoPorIdAsync(int id);
    Task<FeriadoItem?> CrearFeriadoAsync(CreateFeriadoRequest request);
    Task<bool> ActualizarFeriadoAsync(int id, UpdateFeriadoRequest request);
    Task<bool> EliminarFeriadoAsync(int id);
}
