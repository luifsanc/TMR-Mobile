using tmr_mobile.ViewModels;

namespace tmr_mobile.Views.Configuracion;

public partial class FeriadosPage : ContentPage
{
    public FeriadosPage(FeriadosConfigViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
