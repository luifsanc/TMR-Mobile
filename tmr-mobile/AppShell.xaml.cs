using tmr_mobile.Views.Configuracion;
using tmr_mobile.Views.Operaciones;
using tmr_mobile.Views.TimeReport;
using tmr_mobile.Views.Reportes;
using tmr_mobile.Views.Home;
using tmr_mobile.Views.Auth;
using tmr_mobile.Views.Seguimiento;

namespace tmr_mobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(
            nameof(CatalogoDetallePage),
            typeof(CatalogoDetallePage)
        );

        Routing.RegisterRoute(
            nameof(CatalogoFormPage),
            typeof(CatalogoFormPage)
        );

        Routing.RegisterRoute(
            nameof(ClienteDetallePage),
            typeof(ClienteDetallePage)
        );

        Routing.RegisterRoute(
            nameof(ClienteFormPage),
            typeof(ClienteFormPage)
        );

        Routing.RegisterRoute(
            nameof(ColaboradorDetallePage),
            typeof(ColaboradorDetallePage)
        );

        Routing.RegisterRoute(
            nameof(ColaboradorSalidaPage),
            typeof(ColaboradorSalidaPage)
        );

        Routing.RegisterRoute(
            nameof(ColaboradorFormPage),
            typeof(ColaboradorFormPage)
        );

        Routing.RegisterRoute(
            nameof(ProyectosFormPage),
            typeof(ProyectosFormPage)
        );

        Routing.RegisterRoute(
            nameof(ProyectosDetallePage),
            typeof(ProyectosDetallePage)
        );

        // Subpáginas y configuraciones que no son items raíz del Shell
        Routing.RegisterRoute(nameof(ReporteHorasPage), typeof(ReporteHorasPage));
        Routing.RegisterRoute(nameof(ReporteFechasPage), typeof(ReporteFechasPage));
        Routing.RegisterRoute(nameof(UsuariosPage), typeof(UsuariosPage));
        Routing.RegisterRoute(nameof(UsuarioDetallePage), typeof(UsuarioDetallePage));
        Routing.RegisterRoute(nameof(UsuarioFormPage), typeof(UsuarioFormPage));
        Routing.RegisterRoute(nameof(RolesPage), typeof(RolesPage));
        Routing.RegisterRoute(nameof(RolDetallePage), typeof(RolDetallePage));
        Routing.RegisterRoute(nameof(RolFormPage), typeof(RolFormPage));
        Routing.RegisterRoute(nameof(CargaActividadesPage), typeof(CargaActividadesPage));
        Routing.RegisterRoute(nameof(CrearActividadPage), typeof(CrearActividadPage));
        Routing.RegisterRoute(nameof(FeriadosPage), typeof(FeriadosPage));
        Routing.RegisterRoute(nameof(FeriadoDetallePage), typeof(FeriadoDetallePage));
        Routing.RegisterRoute(nameof(FeriadoFormPage), typeof(FeriadoFormPage));
        Routing.RegisterRoute(nameof(CatalogosPage), typeof(CatalogosPage));
        Routing.RegisterRoute(nameof(ChangePasswordPage), typeof(ChangePasswordPage));
        Routing.RegisterRoute(nameof(PersonalDataPage), typeof(PersonalDataPage));
        Routing.RegisterRoute(nameof(AppPreferencesPage), typeof(AppPreferencesPage));
        Routing.RegisterRoute(nameof(AboutTmrPage), typeof(AboutTmrPage));
        Routing.RegisterRoute(nameof(SeguimientoDetallePage), typeof(SeguimientoDetallePage));
    }
}
