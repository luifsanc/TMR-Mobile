using tmr_mobile.Models.Operaciones;

namespace tmr_mobile.Services;

public class ColaboradoresService : IColaboradoresService
{
    private readonly ApiService _apiService;

    public ColaboradoresService(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<List<ColaboradorModel>> ObtenerColaboradoresAsync(string busqueda = "", bool? activo = null, int? asignacion = null)
    {
        var url = "colaboradores";
        var queryParams = new List<string>();

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            queryParams.Add($"busqueda={Uri.EscapeDataString(busqueda.Trim())}");
        }

        if (activo.HasValue)
        {
            queryParams.Add($"activo={activo.Value.ToString().ToLowerInvariant()}");
        }

        if (asignacion.HasValue)
        {
            queryParams.Add($"asignacion={asignacion.Value}");
        }

        if (queryParams.Any())
        {
            url += "?" + string.Join("&", queryParams);
        }

        var response = await _apiService.GetAsync<List<ColaboradorModel>>(url);
        return response ?? new List<ColaboradorModel>();
    }

    public async Task<ColaboradorDetalleModel?> ObtenerColaboradorAsync(int id)
    {
        return await _apiService.GetAsync<ColaboradorDetalleModel>($"colaboradores/{id}");
    }

    public async Task<ColaboradorModel?> CrearColaboradorAsync(CreateColaboradorRequest request)
    {
        var response = await _apiService.PostAsync<CreateColaboradorRequest, JsonElementWrapper>("colaboradores", request);
        if (response != null && response.Id > 0)
        {
            return new ColaboradorModel
            {
                Id = response.Id,
                NombreCompleto = $"{request.Nombres} {request.Apellidos}".Trim(),
                NumeroIdentificacion = request.NumeroIdentificacion,
                Email = request.Email ?? string.Empty,
                Activo = true
            };
        }
        return null;
    }

    public async Task<bool> ActualizarColaboradorAsync(int id, UpdateColaboradorRequest request)
    {
        await _apiService.PutAsync<UpdateColaboradorRequest, object>($"colaboradores/{id}", request);
        return true;
    }

    public async Task<bool> RegistrarSalidaAsync(int id, RegistrarSalidaRequest request)
    {
        await _apiService.PostAsync<RegistrarSalidaRequest, object>($"colaboradores/{id}/salida", request);
        return true;
    }

    public async Task<List<ColaboradorCatalogoItem>> ObtenerCatalogoAsync(string codigo)
    {
        var response = await _apiService.GetAsync<List<ColaboradorCatalogoItem>>($"colaboradores/catalogos/{codigo}");
        return response ?? new List<ColaboradorCatalogoItem>();
    }

    public async Task<List<CargoItem>> ObtenerCargosPorDepartamentoAsync(int idDepartamento)
    {
        var response = await _apiService.GetAsync<List<CargoItem>>($"colaboradores/cargos?idDepartamento={idDepartamento}");
        return response ?? new List<CargoItem>();
    }

    private class JsonElementWrapper
    {
        public int Id { get; set; }
    }
}
