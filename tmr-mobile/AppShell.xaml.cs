using tmr_mobile.Views.Configuracion;
using tmr_mobile.Views.Operaciones;

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
    }
}