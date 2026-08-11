using tmr_mobile.Views.Configuracion;

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
    }
}