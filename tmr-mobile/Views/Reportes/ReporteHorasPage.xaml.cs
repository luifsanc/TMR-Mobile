using tmr_mobile.ViewModels;

namespace tmr_mobile.Views.Reportes;

public partial class ReporteHorasPage : ContentPage
{
    public ReporteHorasPage(ReporteHorasViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
