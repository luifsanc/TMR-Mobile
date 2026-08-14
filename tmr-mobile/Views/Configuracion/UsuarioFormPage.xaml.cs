using tmr_mobile.ViewModels;

namespace tmr_mobile.Views.Configuracion;

public partial class UsuarioFormPage : ContentPage
{
    public UsuarioFormPage(UsuarioFormViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
