using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace tmr_mobile.ViewModels;

public partial class ReporteHorasViewModel : BaseViewModel
{
    [ObservableProperty]
    private DateTime _fechaInicio = DateTime.Now.AddDays(-30);

    [ObservableProperty]
    private DateTime _fechaFin = DateTime.Now;

    public ReporteHorasViewModel()
    {
        Title = "Reporte de Horas Registradas";
    }

    [RelayCommand]
    private async Task GenerarReporteAsync()
    {
        IsBusy = true;
        try
        {
            await Task.Delay(500);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
