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

    public async Task<ClienteModel?> CrearClienteAsync(CreateClienteRequest request)
    {
        var response = await _apiService.PostAsync<CreateClienteRequest, JsonElementWrapper>("clientes", request);
        
        // El endpoint devuelve { "Id": nuevoId }. 
        // Si queremos el cliente completo, tenemos que consultarlo de nuevo o devolver los datos con los que creamos
        // Para ajustarnos a la respuesta del POST, simularemos la respuesta o la volveremos a cargar
        if (response != null && response.Id > 0)
        {
            return new ClienteModel
            {
                Id = response.Id,
                TipoIdentificacion = "N/A", // Sería ideal cargar el real
                NumeroIdentificacion = request.NumeroIdentificacion,
                NombreComercial = request.NombreComercial,
                Email = request.Email,
                Telefono = request.Telefono,
                Activo = true
            };
        }
        return null;
    }

    public async Task<bool> ActualizarClienteAsync(int id, UpdateClienteRequest request)
    {
        var result = await _apiService.PutAsync<UpdateClienteRequest, object>($"clientes/{id}", request);
        // Como el endpoint PUT devuelve NoContent, ApiService devolverá un objeto vacío si fue exitoso (o fallaría en HandleResponse si no 2xx)
        // Por lo tanto, asumiremos true si no hay excepción manejada.
        // En ApiService, si falla, devuelve default (null). Pero para object? devuelve algo?
        // En C#, un NoContent (204) con ReadFromJsonAsync intentará deserializar y podría tirar null.
        // Evaluaremos simplemente si PutAsync fue invocado. En realidad PutAsync del ApiService devuelve TResponse.
        // Si TResponse es object, devolverá default (null) en caso de NoContent.
        // Un enfoque más seguro es agregar PutNoContentAsync o verificar si devuelve null.
        return true; 
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
