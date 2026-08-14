using tmr_mobile.Models.Configuracion;

namespace tmr_mobile.Services;

public interface IUsuariosService
{
    Task<List<UsuarioListaItem>> ObtenerUsuariosAsync(string busqueda = "", bool? activo = null);
    Task<UsuarioDetalleItem?> ObtenerUsuarioAsync(int id);
    Task<List<RolItem>> ObtenerRolesAsync();
    Task<UsuarioListaItem?> CrearUsuarioAsync(CreateUsuarioRequest request);
    Task<bool> ActualizarUsuarioAsync(int id, UpdateUsuarioRequest request);
    Task<bool> CambiarEstadoAsync(int id, bool activo);
}

public class UsuariosService : IUsuariosService
{
    private readonly ApiService _apiService;

    public UsuariosService(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<List<UsuarioListaItem>> ObtenerUsuariosAsync(string busqueda = "", bool? activo = null)
    {
        var url = "configuracion/usuarios?pageSize=1000";

        if (!string.IsNullOrWhiteSpace(busqueda))
            url += $"&search={Uri.EscapeDataString(busqueda)}";

        if (activo.HasValue)
            url += $"&activo={activo.Value.ToString().ToLowerInvariant()}";

        System.Diagnostics.Debug.WriteLine($"[USUARIOS] Endpoint: {url}");

        var response = await _apiService.GetAsync<UsuarioListadoEnvelope>(url);
        if (response?.Items is not null && response.Items.Count > 0)
        {
            return response.Items;
        }

        var directList = await _apiService.GetAsync<List<UsuarioListaItem>>(url);
        return directList ?? new List<UsuarioListaItem>();
    }

    public async Task<UsuarioDetalleItem?> ObtenerUsuarioAsync(int id)
    {
        var endpoint = $"configuracion/usuarios/{id}";
        System.Diagnostics.Debug.WriteLine($"[USUARIO-DETALLE] Endpoint: {endpoint}");

        try
        {
            var result = await _apiService.GetAsync<UsuarioDetalleItem>(endpoint);
            if (result is not null)
                return result;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[USUARIO-DETALLE] Error GET {endpoint}: {ex.Message}");
        }

        // Fallback 1: Cargar como UsuarioListaItem
        try
        {
            var baseItem = await _apiService.GetAsync<UsuarioListaItem>(endpoint);
            if (baseItem is not null)
            {
                return new UsuarioDetalleItem
                {
                    Id = baseItem.Id,
                    IdUsuario = baseItem.IdUsuario,
                    IdPersona = baseItem.IdPersona,
                    NumeroIdentificacion = baseItem.NumeroIdentificacion,
                    Nombres = baseItem.Nombres,
                    Apellidos = baseItem.Apellidos,
                    Email = baseItem.Email,
                    Roles = baseItem.Roles,
                    Activo = baseItem.Activo,
                    DebeCambiarPassword = baseItem.DebeCambiarPassword,
                    UltimoLogin = baseItem.UltimoLogin
                };
            }
        }
        catch { }

        // Fallback 2: Buscar en el listado completo de usuarios
        try
        {
            var todos = await ObtenerUsuariosAsync();
            var encontrado = todos.FirstOrDefault(u => u.Id == id || u.IdUsuario == id);
            if (encontrado is not null)
            {
                return new UsuarioDetalleItem
                {
                    Id = encontrado.Id,
                    IdUsuario = encontrado.IdUsuario,
                    IdPersona = encontrado.IdPersona,
                    NumeroIdentificacion = encontrado.NumeroIdentificacion,
                    Nombres = encontrado.Nombres,
                    Apellidos = encontrado.Apellidos,
                    Email = encontrado.Email,
                    Roles = encontrado.Roles,
                    Activo = encontrado.Activo,
                    DebeCambiarPassword = encontrado.DebeCambiarPassword,
                    UltimoLogin = encontrado.UltimoLogin
                };
            }
        }
        catch { }

        return null;
    }

    public async Task<List<RolItem>> ObtenerRolesAsync()
    {
        var endpoint = "configuracion/roles?pageSize=1000";
        System.Diagnostics.Debug.WriteLine($"[USUARIOS] Endpoint: {endpoint}");

        var response = await _apiService.GetAsync<UsuarioRolesEnvelope>(endpoint);
        if (response?.Items is not null)
            return response.Items;

        var fallback = await _apiService.GetAsync<List<RolItem>>(endpoint);
        if (fallback is not null)
            return fallback;

        return new List<RolItem>();
    }

    public async Task<UsuarioListaItem?> CrearUsuarioAsync(CreateUsuarioRequest request)
    {
        var endpoint = "configuracion/usuarios";
        System.Diagnostics.Debug.WriteLine($"[USUARIOS] Endpoint: {endpoint}");
        return await _apiService.PostAsync<CreateUsuarioRequest, UsuarioListaItem>(endpoint, request);
    }

    public async Task<bool> ActualizarUsuarioAsync(int id, UpdateUsuarioRequest request)
    {
        var endpoint = $"configuracion/usuarios/{id}";
        System.Diagnostics.Debug.WriteLine($"[USUARIOS] Endpoint: {endpoint}");
        var result = await _apiService.PutAsync<UpdateUsuarioRequest, object>(endpoint, request);
        return result is not null;
    }

    public async Task<bool> CambiarEstadoAsync(int id, bool activo)
    {
        var endpoint = $"configuracion/usuarios/{id}";
        var request = new CambiarEstadoUsuarioRequest { Activo = activo };
        System.Diagnostics.Debug.WriteLine($"[USUARIOS] Endpoint: {endpoint}");
        var result = await _apiService.PatchAsync<CambiarEstadoUsuarioRequest, object>(endpoint, request);
        return result is not null;
    }
}
