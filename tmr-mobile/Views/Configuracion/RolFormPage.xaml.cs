using tmr_mobile.ViewModels;

namespace tmr_mobile.Views.Configuracion;

public partial class RolFormPage : ContentPage
{
    public RolFormPage(RolFormViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
