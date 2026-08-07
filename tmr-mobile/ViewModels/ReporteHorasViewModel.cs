using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace tmr_mobile.ViewModels;

public partial class ReporteHorasViewModel : BaseViewModel
{
    [ObservableProperty]
    public partial DateTime FechaInicio { get; set; } = DateTime.Now.AddDays(-30);

    [ObservableProperty]
    public partial DateTime FechaFin { get; set; } = DateTime.Now;

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
