using tmr_mobile.Models.Seguimiento;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace tmr_mobile.Services;

public interface ISeguimientoService
{
    Task<List<SeguimientoColaboradorDto>> ObtenerSeguimientoAsync(FiltroSeguimientoDto filtro);
    Task<DetalleActividadesResponse> ObtenerActividadesColaboradorAsync(int idColaborador, string fechaDesde, string fechaHasta);
}
