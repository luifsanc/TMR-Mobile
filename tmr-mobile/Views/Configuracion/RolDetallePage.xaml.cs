using tmr_mobile.ViewModels;

namespace tmr_mobile.Views.Configuracion;

public partial class RolDetallePage : ContentPage
{
    public RolDetallePage(RolDetalleViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
