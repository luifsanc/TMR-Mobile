using tmr_mobile.Models.Operaciones;

namespace tmr_mobile.Services;

public class ClientesService : IClientesService
{
    private readonly ApiService _apiService;

    public ClientesService(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<List<ClienteModel>> ObtenerClientesAsync(string busqueda = "", bool? activo = null)
    {
        var url = "clientes";
        var queryParams = new List<string>();

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            queryParams.Add($"busqueda={Uri.EscapeDataString(busqueda)}");
        }

        if (activo.HasValue)
        {
            queryParams.Add($"activo={activo.Value.ToString().ToLowerInvariant()}");
        }

        if (queryParams.Any())
        {
            url += "?" + string.Join("&", queryParams);
        }

        var response = await _apiService.GetAsync<List<ClienteModel>>(url);
        return response ?? new List<ClienteModel>();
    }

    public async Task<ClienteDetalleModel?> ObtenerClienteAsync(int id)
    {
        return await _apiService.GetAsync<ClienteDetalleModel>($"clientes/{id}");
    }

    public async Task<ApiOperationResult> CrearClienteAsync(CreateClienteRequest request)
    {
        return await _apiService.PostForResultAsync("clientes", request);
    }

    public async Task<ApiOperationResult> ActualizarClienteAsync(int id, UpdateClienteRequest request)
    {
        return await _apiService.PutForResultAsync($"clientes/{id}", request);
    }

    public async Task<List<TipoIdentificacionModel>> ObtenerTiposIdentificacionAsync()
    {
        var response = await _apiService.GetAsync<List<TipoIdentificacionModel>>("clientes/tipos-identificacion");
        return response ?? new List<TipoIdentificacionModel>();
    }
    
    private class JsonElementWrapper
    {
        public int Id { get; set; }
    }
}
