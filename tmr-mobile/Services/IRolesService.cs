using tmr_mobile.Models.Configuracion;

namespace tmr_mobile.Services;

public interface IRolesService
{
    Task<List<RolListaItem>> ObtenerRolesAsync(string busqueda = "", bool? activo = null);
    Task<RolListaItem?> ObtenerRolAsync(int id);
    Task<List<RolModuloItem>> ObtenerModulosDisponiblesAsync();
    Task<RolListaItem?> CrearRolAsync(CreateRolRequest request);
    Task<bool> ActualizarRolAsync(int id, UpdateRolRequest request);
    Task<bool> CambiarEstadoAsync(int id, bool activo);
}
