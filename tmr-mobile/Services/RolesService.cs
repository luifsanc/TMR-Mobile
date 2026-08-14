using tmr_mobile.Models.Configuracion;

namespace tmr_mobile.Services;

public class RolesService : IRolesService
{
    private readonly ApiService _apiService;

    private static readonly List<RolModuloItem> ModulosCatalogoDefault = new()
    {
        new RolModuloItem { Id = 1, Nombre = "Dashboard" },
        new RolModuloItem { Id = 2, Nombre = "Proyectos" },
        new RolModuloItem { Id = 3, Nombre = "Actividades" },
        new RolModuloItem { Id = 4, Nombre = "Seguimiento" },
        new RolModuloItem { Id = 5, Nombre = "Colaboradores" },
        new RolModuloItem { Id = 6, Nombre = "Clientes" },
        new RolModuloItem { Id = 7, Nombre = "Lideres" },
        new RolModuloItem { Id = 8, Nombre = "Roles" },
        new RolModuloItem { Id = 9, Nombre = "Usuarios" },
        new RolModuloItem { Id = 10, Nombre = "Dias Festivos" },
        new RolModuloItem { Id = 11, Nombre = "Proyecto por horas" },
        new RolModuloItem { Id = 12, Nombre = "Proyecto por fechas" },
        new RolModuloItem { Id = 13, Nombre = "Solicitud de requerimiento" },
        new RolModuloItem { Id = 14, Nombre = "Historial de requerimiento" },
        new RolModuloItem { Id = 15, Nombre = "Configuración" },
        new RolModuloItem { Id = 16, Nombre = "Reportes" },
        new RolModuloItem { Id = 17, Nombre = "Time Report" },
        new RolModuloItem { Id = 18, Nombre = "Requerimientos" },
    };

    public RolesService(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<List<RolListaItem>> ObtenerRolesAsync(string busqueda = "", bool? activo = null)
    {
        var url = "configuracion/roles?pageSize=1000";

        if (!string.IsNullOrWhiteSpace(busqueda))
            url += $"&search={Uri.EscapeDataString(busqueda)}";

        if (activo.HasValue)
            url += $"&activo={activo.Value.ToString().ToLowerInvariant()}";

        System.Diagnostics.Debug.WriteLine($"[ROLES] Endpoint: {url}");

        var envelope = await _apiService.GetAsync<RolListadoEnvelope>(url);
        if (envelope?.Items is not null && envelope.Items.Count > 0)
        {
            return envelope.Items;
        }

        var directList = await _apiService.GetAsync<List<RolListaItem>>(url);
        return directList ?? new List<RolListaItem>();
    }

    public async Task<RolListaItem?> ObtenerRolAsync(int id)
    {
        var endpoint = $"configuracion/roles/{id}";
        System.Diagnostics.Debug.WriteLine($"[ROLES] Endpoint: {endpoint}");
        return await _apiService.GetAsync<RolListaItem>(endpoint);
    }

    public async Task<List<RolModuloItem>> ObtenerModulosDisponiblesAsync()
    {
        var endpoint = "configuracion/roles/modulos";
        System.Diagnostics.Debug.WriteLine($"[ROLES] Endpoint: {endpoint}");

        var envelope = await _apiService.GetAsync<ModuloListadoEnvelope>(endpoint);
        if (envelope?.Items is not null && envelope.Items.Count > 0)
            return envelope.Items;

        var directList = await _apiService.GetAsync<List<RolModuloItem>>(endpoint);
        if (directList is not null && directList.Count > 0)
            return directList;

        return ModulosCatalogoDefault;
    }

    public async Task<RolListaItem?> CrearRolAsync(CreateRolRequest request)
    {
        var endpoint = "configuracion/roles";
        System.Diagnostics.Debug.WriteLine($"[ROLES] Endpoint: {endpoint}");
        return await _apiService.PostAsync<CreateRolRequest, RolListaItem>(endpoint, request);
    }

    public async Task<bool> ActualizarRolAsync(int id, UpdateRolRequest request)
    {
        var endpoint = $"configuracion/roles/{id}";
        System.Diagnostics.Debug.WriteLine($"[ROLES] Endpoint: {endpoint}");
        var result = await _apiService.PutAsync<UpdateRolRequest, object>(endpoint, request);
        return result is not null;
    }

    public async Task<bool> CambiarEstadoAsync(int id, bool activo)
    {
        var endpoint = $"configuracion/roles/{id}";
        var request = new CambiarEstadoRolRequest { Activo = activo };
        System.Diagnostics.Debug.WriteLine($"[ROLES] Endpoint: {endpoint}");
        var result = await _apiService.PatchAsync<CambiarEstadoRolRequest, object>(endpoint, request);
        return result is not null;
    }
}
