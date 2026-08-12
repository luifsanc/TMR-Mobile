using tmr_mobile.ViewModels;

namespace tmr_mobile.Views.Configuracion;

public partial class CatalogoFormPage : ContentPage
{
    public CatalogoFormPage(
        CatalogoFormViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }
}