using tmr_mobile.Models.Seguimiento;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace tmr_mobile.Services;

public class SeguimientoService : ISeguimientoService
{
    private readonly ApiService _apiService;

    public SeguimientoService(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<List<SeguimientoColaboradorDto>> ObtenerSeguimientoAsync(FiltroSeguimientoDto filtro)
    {
        var url = "time-report/seguimiento";
        var queryParams = new List<string>();

        if (!string.IsNullOrWhiteSpace(filtro.Busqueda))
        {
            queryParams.Add($"Busqueda={Uri.EscapeDataString(filtro.Busqueda)}");
        }
        if (!string.IsNullOrWhiteSpace(filtro.ClienteSeleccionado))
        {
            queryParams.Add($"ClienteSeleccionado={Uri.EscapeDataString(filtro.ClienteSeleccionado)}");
        }
        if (!string.IsNullOrWhiteSpace(filtro.FechaDesde))
        {
            queryParams.Add($"FechaDesde={filtro.FechaDesde}");
        }
        if (!string.IsNullOrWhiteSpace(filtro.FechaHasta))
        {
            queryParams.Add($"FechaHasta={filtro.FechaHasta}");
        }

        if (queryParams.Any())
        {
            url += "?" + string.Join("&", queryParams);
        }

        var response = await _apiService.GetAsync<List<SeguimientoColaboradorDto>>(url);
        return response ?? new List<SeguimientoColaboradorDto>();
    }
}
