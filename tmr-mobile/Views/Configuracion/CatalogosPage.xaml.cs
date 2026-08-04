using tmr_mobile.ViewModels;

namespace tmr_mobile.Views.Configuracion;

public partial class CatalogosPage : ContentPage
{
    public CatalogosPage(CatalogosConfigViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
