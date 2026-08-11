using tmr_mobile.ViewModels;

namespace tmr_mobile.Views.Operaciones;

public partial class ClienteDetallePage : ContentPage
{
    public ClienteDetallePage(ClienteDetalleViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
