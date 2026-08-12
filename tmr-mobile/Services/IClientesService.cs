using tmr_mobile.Models.Operaciones;

namespace tmr_mobile.Services;

public interface IClientesService
{
    Task<List<ClienteModel>> ObtenerClientesAsync(string busqueda = "", bool? activo = null);
    
    Task<ClienteDetalleModel?> ObtenerClienteAsync(int id);
    
    Task<ClienteModel?> CrearClienteAsync(CreateClienteRequest request);
    
    Task<bool> ActualizarClienteAsync(int id, UpdateClienteRequest request);
    
    Task<List<TipoIdentificacionModel>> ObtenerTiposIdentificacionAsync();
}
