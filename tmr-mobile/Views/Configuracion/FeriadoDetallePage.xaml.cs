using tmr_mobile.ViewModels;

namespace tmr_mobile.Views.Configuracion;

public partial class FeriadoDetallePage : ContentPage
{
    public FeriadoDetallePage(FeriadoDetalleViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
