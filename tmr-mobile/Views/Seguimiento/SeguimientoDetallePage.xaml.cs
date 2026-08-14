using Microsoft.Maui.Controls;
using tmr_mobile.ViewModels;

namespace tmr_mobile.Views.Seguimiento;

public partial class SeguimientoDetallePage : ContentPage
{
    public SeguimientoDetallePage(SeguimientoDetalleViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
