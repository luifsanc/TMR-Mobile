using tmr_mobile.Models.Configuracion;

namespace tmr_mobile.Services;

public interface ICatalogosService
{
    Task<List<CatalogoMaster>> ObtenerCatalogosAsync();

    Task<List<CatalogoDetalle>> ObtenerDetallesAsync(
        int idCatalogo
    );

    Task<CatalogoDetalle?> CrearDetalleAsync(
        CreateCatalogoDetalleRequest request
    );

    Task<CatalogoDetalle?> ActualizarDetalleAsync(
        int id,
        UpdateCatalogoDetalleRequest request
    );

    Task<bool> EliminarDetalleAsync(
        int id,
        int idCatalogo
    );
}

public class CatalogosService : ICatalogosService
{
    private readonly ApiService _apiService;

    public CatalogosService(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<List<CatalogoMaster>> ObtenerCatalogosAsync()
    {
        var response =
            await _apiService.GetAsync<List<CatalogoMaster>>(
                "configuracion/catalogos"
            );

        return response ?? new();
    }

    public async Task<List<CatalogoDetalle>> ObtenerDetallesAsync(
        int idCatalogo)
    {
        var response =
            await _apiService.GetAsync<List<CatalogoDetalle>>(
                $"configuracion/catalogos/{idCatalogo}/detalles"
            );

        return response ?? new();
    }

    public async Task<CatalogoDetalle?> CrearDetalleAsync(
        CreateCatalogoDetalleRequest request)
    {
        return await _apiService.PostAsync<
            CreateCatalogoDetalleRequest,
            CatalogoDetalle>(
            "configuracion/catalogos/detalles",
            request
        );
    }

    public async Task<CatalogoDetalle?> ActualizarDetalleAsync(
        int id,
        UpdateCatalogoDetalleRequest request)
    {
        return await _apiService.PutAsync<
            UpdateCatalogoDetalleRequest,
            CatalogoDetalle>(
            $"configuracion/catalogos/detalles/{id}",
            request
        );
    }

    public async Task<bool> EliminarDetalleAsync(
        int id,
        int idCatalogo)
    {
        return await _apiService.DeleteAsync(
            $"configuracion/catalogos/detalles/{id}?idCatalogo={idCatalogo}"
        );
    }
}