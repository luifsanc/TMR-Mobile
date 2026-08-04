using tmr_mobile.ViewModels;

namespace tmr_mobile.Views.Reportes;

public partial class ReporteFechasPage : ContentPage
{
    public ReporteFechasPage(ReporteFechasViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
