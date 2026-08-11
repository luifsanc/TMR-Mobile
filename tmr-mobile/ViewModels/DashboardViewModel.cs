using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using tmr_mobile.Services;

namespace tmr_mobile.ViewModels;

public class DashboardProyectoItem
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Cliente { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public decimal Horas { get; set; }
}

public class DashboardClienteItem
{
    public string Cliente { get; set; } = string.Empty;
    public int ProyectosAsignados { get; set; }
    public decimal Porcentaje { get; set; }
}

public sealed class DashboardMetricsApiResponse
{
    public int TotalProyectos { get; set; }
    public decimal HorasReportadas { get; set; }
    public int ColaboradoresActivos { get; set; }
    public int ClientesActivos { get; set; }
}

public sealed class DashboardProjectSummaryApiResponse
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Cliente { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public decimal Horas { get; set; }
}

public sealed class DashboardClientApiResponse
{
    public string Cliente { get; set; } = string.Empty;
    public int ProyectosAsignados { get; set; }
    public decimal Porcentaje { get; set; }
}

public sealed class DashboardApiResponse
{
    public DashboardMetricsApiResponse? Metricas { get; set; }
    public IEnumerable<DashboardProjectSummaryApiResponse>? ProximosACerrar { get; set; }
    public IEnumerable<DashboardClientApiResponse>? ProyectosPorCliente { get; set; }
}

public partial class DashboardViewModel : BaseViewModel
{
    private readonly ApiService _apiService;

    [ObservableProperty]
    private int _totalProyectos;

    [ObservableProperty]
    private int _totalColaboradores;

    [ObservableProperty]
    private double _horasRegistradasMes;

    public ObservableCollection<DashboardProyectoItem> ProyectosRecientes { get; } = new();
    public ObservableCollection<DashboardClienteItem> ProyectosPorCliente { get; } = new();

    public DashboardViewModel(ApiService apiService)
    {
        _apiService = apiService;
        Title = "Dashboard Principal";
    }

    [RelayCommand]
    private async Task CargarDashboardAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var response = await _apiService.GetAsync<DashboardApiResponse>("dashboard?rango=mes");
            if (response?.Metricas is null)
            {
                ErrorMessage = "No se pudo cargar el dashboard desde el backend.";
                return;
            }

            TotalProyectos = response.Metricas.TotalProyectos;
            TotalColaboradores = response.Metricas.ColaboradoresActivos;
            HorasRegistradasMes = (double)response.Metricas.HorasReportadas;

            ProyectosRecientes.Clear();
            foreach (var proyecto in response.ProximosACerrar ?? Enumerable.Empty<DashboardProjectSummaryApiResponse>())
            {
                ProyectosRecientes.Add(new DashboardProyectoItem
                {
                    Codigo = proyecto.Codigo,
                    Nombre = proyecto.Nombre,
                    Cliente = proyecto.Cliente,
                    Estado = proyecto.Estado,
                    Horas = proyecto.Horas
                });
            }

            ProyectosPorCliente.Clear();
            foreach (var cliente in response.ProyectosPorCliente ?? Enumerable.Empty<DashboardClientApiResponse>())
            {
                ProyectosPorCliente.Add(new DashboardClienteItem
                {
                    Cliente = cliente.Cliente,
                    ProyectosAsignados = cliente.ProyectosAsignados,
                    Porcentaje = cliente.Porcentaje
                });
            }
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
