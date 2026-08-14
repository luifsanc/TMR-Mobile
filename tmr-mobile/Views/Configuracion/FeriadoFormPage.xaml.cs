using tmr_mobile.ViewModels;

namespace tmr_mobile.Views.Configuracion;

public partial class FeriadoFormPage : ContentPage
{
    public FeriadoFormPage(FeriadoFormViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
