using tmr_mobile.ViewModels;

namespace tmr_mobile.Views.TimeReport;

public partial class TimeReportPage : ContentPage
{
    public TimeReportPage(TimeReportViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is TimeReportViewModel vm)
        {
            vm.CargarActividadesCommand.Execute(null);
        }
    }
}
