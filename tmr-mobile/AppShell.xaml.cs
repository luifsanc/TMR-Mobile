using tmr_mobile.Views.Configuracion;
using tmr_mobile.Views.Operaciones;
using tmr_mobile.Views.TimeReport;
using tmr_mobile.Views.Reportes;
using tmr_mobile.Views.Home;

namespace tmr_mobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Subpáginas y configuraciones que no son items raíz del Shell
        Routing.RegisterRoute(nameof(ReporteHorasPage), typeof(ReporteHorasPage));
        Routing.RegisterRoute(nameof(ReporteFechasPage), typeof(ReporteFechasPage));
        Routing.RegisterRoute(nameof(UsuariosPage), typeof(UsuariosPage));
        Routing.RegisterRoute(nameof(RolesPage), typeof(RolesPage));
        Routing.RegisterRoute(nameof(CargaActividadesPage), typeof(CargaActividadesPage));
        Routing.RegisterRoute(nameof(CrearActividadPage), typeof(CrearActividadPage));
        Routing.RegisterRoute(nameof(FeriadosPage), typeof(FeriadosPage));
        Routing.RegisterRoute(nameof(CatalogosPage), typeof(CatalogosPage));
        Routing.RegisterRoute(nameof(CatalogoDetallePage), typeof(CatalogoDetallePage));
        Routing.RegisterRoute(nameof(CatalogoFormPage), typeof(CatalogoFormPage));
    }
}