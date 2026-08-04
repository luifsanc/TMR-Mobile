using tmr_mobile.ViewModels;

namespace tmr_mobile.Views.TimeReport;

public partial class CargaActividadesPage : ContentPage
{
    public CargaActividadesPage(CargaActividadesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
