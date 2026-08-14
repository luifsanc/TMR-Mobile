using Microsoft.Maui.Controls;
using tmr_mobile.ViewModels;

namespace tmr_mobile.Views.Seguimiento;

public partial class SeguimientoPage : ContentPage
{
    public SeguimientoPage(SeguimientoViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
