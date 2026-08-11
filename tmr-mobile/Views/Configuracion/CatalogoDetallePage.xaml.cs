using tmr_mobile.ViewModels;

namespace tmr_mobile.Views.Configuracion;

public partial class CatalogoDetallePage : ContentPage
{
    public CatalogoDetallePage(
        CatalogoDetalleViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }
}