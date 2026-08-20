using tmr_mobile.ViewModels;

namespace tmr_mobile.Views.Operaciones;

public partial class ProyectosDetallePage : ContentPage
{
    public ProyectosDetallePage(ProyectosDetalleViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
