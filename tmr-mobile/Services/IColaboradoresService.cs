using tmr_mobile.Models.Operaciones;

namespace tmr_mobile.Services;

public interface IColaboradoresService
{
    Task<List<ColaboradorModel>> ObtenerColaboradoresAsync(string busqueda = "", bool? activo = null, int? asignacion = null);
    
    Task<ColaboradorDetalleModel?> ObtenerColaboradorAsync(int id);
    
    Task<ColaboradorModel?> CrearColaboradorAsync(CreateColaboradorRequest request);
    
    Task<bool> ActualizarColaboradorAsync(int id, UpdateColaboradorRequest request);
    
    Task<bool> RegistrarSalidaAsync(int id, RegistrarSalidaRequest request);
    
    Task<List<ColaboradorCatalogoItem>> ObtenerCatalogoAsync(string codigo);
    
    Task<List<CargoItem>> ObtenerCargosPorDepartamentoAsync(int idDepartamento);
}
