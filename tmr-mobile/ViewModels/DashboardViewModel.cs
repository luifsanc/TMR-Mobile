using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using tmr_mobile.Services;
using tmr_shared.DTOs.Dashboard;

namespace tmr_mobile.ViewModels;

public partial class DashboardViewModel : BaseViewModel
{
    private readonly ApiService _apiService;

    [ObservableProperty]
    public partial int TotalProyectos { get; set; }

    [ObservableProperty]
    public partial int TotalColaboradores { get; set; }

    [ObservableProperty]
    public partial decimal HorasRegistradasMes { get; set; }

    [ObservableProperty]
    public partial int ClientesActivos { get; set; }

    public DashboardViewModel(ApiService apiService)
    {
        _apiService = apiService;
        Title = "Dashboard Principal";
    }

    public string NombreUsuario { get; set; } = "Teofilo";

    public System.Collections.ObjectModel.ObservableCollection<tmr_mobile.Views.Dashboard.Models.StatCard> StatCards { get; } = new()
    {
        new tmr_mobile.Views.Dashboard.Models.StatCard { Value = "80",  Label = "Proyectos\nen total",       IconBackground = "#E7EEFF" },
        new tmr_mobile.Views.Dashboard.Models.StatCard { Value = "110 h", Label = "Horas\nreportadas",        IconBackground = "#E1F8E9" },
        new tmr_mobile.Views.Dashboard.Models.StatCard { Value = "139", Label = "Colaboradores\nactivos",     IconBackground = "#FFF0DE" },
        new tmr_mobile.Views.Dashboard.Models.StatCard { Value = "35",  Label = "Clientes\nactivos",          IconBackground = "#F1E9FF" },
    };

    public System.Collections.ObjectModel.ObservableCollection<tmr_mobile.Views.Dashboard.Models.ClosingProject> ProximosACerrar { get; } = new()
    {
        new tmr_mobile.Views.Dashboard.Models.ClosingProject
        {
            Codigo = "BCN_DOC_PROC", Titulo = "Documentacion de procesos BCN",
            Cliente = "BANCO COOPNACIONAL", FechaCierre = new DateTime(2025,1,14),
            Estado = "Completado", HorasRestantes = 0
        },
        new tmr_mobile.Views.Dashboard.Models.ClosingProject
        {
            Codigo = "CONECEL_FS_CALIDAD", Titulo = "Calidad Fabrica de software",
            Cliente = "CLARO - CONECEL S.A.", FechaCierre = new DateTime(2025,4,29),
            Estado = "Completado", HorasRestantes = 0
        },
        new tmr_mobile.Views.Dashboard.Models.ClosingProject
        {
            Codigo = "BM_FS_PROYECTOS", Titulo = "Proyectos Fabrica de software",
            Cliente = "BANCO DE MACHALA", FechaCierre = new DateTime(2025,6,12),
            Estado = "En progreso", HorasRestantes = 0
        },
    };

    public System.Collections.ObjectModel.ObservableCollection<tmr_mobile.Views.Dashboard.Models.ChartPoint> HorasPorProyecto { get; } = new()
    {
        new tmr_mobile.Views.Dashboard.Models.ChartPoint { Categoria = "Proyectos de\nProcesos", Horas = 110 }
    };

    public System.Collections.ObjectModel.ObservableCollection<tmr_mobile.Views.Dashboard.Models.MetricMini> MetricasMini { get; } = new()
    {
        new tmr_mobile.Views.Dashboard.Models.MetricMini { Titulo = "TOTAL DE HORAS",      Valor = "110 h" },
        new tmr_mobile.Views.Dashboard.Models.MetricMini { Titulo = "PROMEDIO DE HORAS",   Valor = "110 h" },
        new tmr_mobile.Views.Dashboard.Models.MetricMini { Titulo = "AVANCE DE JORNADA",   Valor = "0%"   },
    };

    public System.Collections.ObjectModel.ObservableCollection<tmr_mobile.Views.Dashboard.Models.ActivityItem> ActividadReciente { get; } = new()
    {
        new tmr_mobile.Views.Dashboard.Models.ActivityItem
        {
            IconBackground = "#F1E9FF",
            Descripcion = "Cristopher Vera reportó 8h en Documentacion de procesos BCN",
            Tiempo = "hace 2 horas"
        },
        new tmr_mobile.Views.Dashboard.Models.ActivityItem
        {
            IconBackground = "#E1F8E9",
            Descripcion = "Calidad Fabrica de software fue completado",
            Tiempo = "hace 1 día"
        },
        new tmr_mobile.Views.Dashboard.Models.ActivityItem
        {
            IconBackground = "#FFF0DE",
            Descripcion = "Nuevo colaborador: Ana Romero",
            Tiempo = "hace 2 días"
        },
    };

    public string DetalleTitulo { get; set; } = "Proyectos de Procesos";
    public string DetalleTotalHoras { get; set; } = "110 h";

    public System.Collections.ObjectModel.ObservableCollection<tmr_mobile.Views.Dashboard.Models.HourDetail> DetalleHoras { get; } = new()
    {
        new tmr_mobile.Views.Dashboard.Models.HourDetail { Color = "#2E5BFF", Proyecto = "Documentacion de procesos BCN", Horas = 48 },
        new tmr_mobile.Views.Dashboard.Models.HourDetail { Color = "#1F9254", Proyecto = "Calidad Fabrica de software",    Horas = 32 },
        new tmr_mobile.Views.Dashboard.Models.HourDetail { Color = "#F58220", Proyecto = "Proyectos Fabrica de software",  Horas = 20 },
        new tmr_mobile.Views.Dashboard.Models.HourDetail { Color = "#7C4DFF", Proyecto = "Otros proyectos",                 Horas = 10 },
    };

    [RelayCommand]
    private async Task CargarDashboardAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            // GET /api/dashboard?rango=mes
            var response = await _apiService.GetAsync<DashboardDataResponse>("api/dashboard?rango=mes");
            if (response?.Metricas != null)
            {
                TotalProyectos      = response.Metricas.TotalProyectos;
                TotalColaboradores  = response.Metricas.ColaboradoresActivos;
                HorasRegistradasMes = response.Metricas.HorasReportadas;
                ClientesActivos     = response.Metricas.ClientesActivos;
            }
            else
            {
                ErrorMessage = "No se pudieron cargar los datos del dashboard.";
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
