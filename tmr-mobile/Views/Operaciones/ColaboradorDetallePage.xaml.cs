using tmr_mobile.ViewModels;

namespace tmr_mobile.Views.Operaciones;

public partial class ColaboradorDetallePage : ContentPage
{
    public ColaboradorDetallePage(ColaboradorDetalleViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
