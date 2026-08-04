using tmr_mobile.ViewModels;

namespace tmr_mobile.Views.Configuracion;

public partial class UsuariosPage : ContentPage
{
    public UsuariosPage(UsuariosConfigViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
