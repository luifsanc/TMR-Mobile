using tmr_mobile.ViewModels;

namespace tmr_mobile.Views.Configuracion;

public partial class UsuarioDetallePage : ContentPage
{
    public UsuarioDetallePage(UsuarioDetalleViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
