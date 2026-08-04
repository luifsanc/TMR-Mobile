using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using tmr_mobile.Services;

namespace tmr_mobile.ViewModels;

public partial class DashboardViewModel : BaseViewModel
{
    private readonly ApiService _apiService;

    [ObservableProperty]
    private int _totalProyectos;

    [ObservableProperty]
    private int _totalColaboradores;

    [ObservableProperty]
    private double _horasRegistradasMes;

    public DashboardViewModel(ApiService apiService)
    {
        _apiService = apiService;
        Title = "Dashboard Principal";
    }

    [RelayCommand]
    private async Task CargarDashboardAsync()
    {
        IsBusy = true;
        try
        {
            // Cargar datos reales del dashboard
            TotalProyectos = 12;
            TotalColaboradores = 45;
            HorasRegistradasMes = 680.5;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al cargar dashboard: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
