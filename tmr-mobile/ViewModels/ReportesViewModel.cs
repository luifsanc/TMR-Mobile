using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using tmr_mobile.Views.Reportes;

namespace tmr_mobile.ViewModels;

public partial class ReportesViewModel : BaseViewModel
{
    public ReportesViewModel()
    {
        Title = "Reportes";
    }

    [RelayCommand]
    private async Task IrAReporteHorasAsync()
    {
        await Shell.Current.GoToAsync(nameof(ReporteHorasPage));
    }

    [RelayCommand]
    private async Task IrAReporteFechasAsync()
    {
        await Shell.Current.GoToAsync(nameof(ReporteFechasPage));
    }
}
