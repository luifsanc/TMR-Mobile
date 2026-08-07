using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using tmr_mobile.Services;
using tmr_mobile.Views.Dashboard.Models;
using tmr_shared.DTOs.Dashboard;

namespace tmr_mobile.ViewModels;

public partial class DashboardViewModel : BaseViewModel
{
    private readonly ApiService _apiService;

    // Paleta usada para "Detalle de horas" (derivado de HorasPorProyecto,
    // ya que el endpoint no trae un color por proyecto).
    private static readonly string[] DetallePalette =
        { "#2E5BFF", "#1F9254", "#F58220", "#7C4DFF", "#E4374B", "#0BA5EC" };

    [ObservableProperty]
    public partial int TotalProyectos { get; set; }

    [ObservableProperty]
    public partial int TotalColaboradores { get; set; }

    [ObservableProperty]
    public partial decimal HorasRegistradasMes { get; set; }

    [ObservableProperty]
    public partial int ClientesActivos { get; set; }

    [ObservableProperty]
    public partial string DetalleTotalHoras { get; set; } = "-";

    public DashboardViewModel(ApiService apiService)
    {
        _apiService = apiService;
        Title = "Dashboard Principal";

        // Carga inicial. CargarDashboardCommand es un IAsyncRelayCommand
        // generado por [RelayCommand]; Execute maneja el fire-and-forget
        // de forma segura (las excepciones ya se capturan adentro).
        CargarDashboardCommand.Execute(null);
    }

    public string NombreUsuario { get; set; } = "Teofilo";
    public string DetalleTitulo { get; set; } = "Detalle por proyecto";

    // Inicial para el avatar circular del header (reemplaza avatar_icon.png,
    // que no existe como recurso). NombreUsuario no es reactivo por ahora,
    // así que basta con un getter simple.
    public string AvatarInicial => string.IsNullOrWhiteSpace(NombreUsuario)
        ? "?"
        : NombreUsuario.Trim()[..1].ToUpperInvariant();

    // ---------------------------------------------------------------
    // Colecciones bindeadas al XAML. StatCard/MetricMini/etc. son POCOs
    // planos (sin INotifyPropertyChanged), así que NO se pueden mutar
    // sus propiedades después de agregados a la ObservableCollection y
    // esperar que la UI se entere. Por eso siempre se hace Clear() + Add()
    // con instancias nuevas, nunca item.Propiedad = valor.
    // ---------------------------------------------------------------

    public System.Collections.ObjectModel.ObservableCollection<StatCard> StatCards { get; } = new();
    public System.Collections.ObjectModel.ObservableCollection<ClosingProject> ProximosACerrar { get; } = new();
    public System.Collections.ObjectModel.ObservableCollection<ChartPoint> HorasPorProyecto { get; } = new();
    public System.Collections.ObjectModel.ObservableCollection<MetricMini> MetricasMini { get; } = new();
    public System.Collections.ObjectModel.ObservableCollection<HourDetail> DetalleHoras { get; } = new();

    // TODO: reemplazar por datos reales cuando exista el endpoint de actividad.
    public System.Collections.ObjectModel.ObservableCollection<ActivityItem> ActividadReciente { get; } = new()
    {
        new ActivityItem
        {
            IconBackground = "#F1E9FF",
            Descripcion = "Cristopher Vera reportó 8h en Documentacion de procesos BCN",
            Tiempo = "hace 2 horas"
        },
        new ActivityItem
        {
            IconBackground = "#E1F8E9",
            Descripcion = "Calidad Fabrica de software fue completado",
            Tiempo = "hace 1 día"
        },
        new ActivityItem
        {
            IconBackground = "#FFF0DE",
            Descripcion = "Nuevo colaborador: Ana Romero",
            Tiempo = "hace 2 días"
        },
    };

    [RelayCommand]
    private async Task CargarDashboardAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var response = await _apiService.GetAsync<DashboardDataResponse>("api/dashboard?rango=mes");

            if (response?.Metricas == null)
            {
                ErrorMessage = "No se pudieron cargar los datos del dashboard.";
                return;
            }

            MapMetricas(response.Metricas);
            MapProximosACerrar(response.ProximosACerrar);
            MapHorasPorProyecto(response.HorasPorProyecto);
            MapDetalleHoras(response.HorasPorProyecto);
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

    private void MapMetricas(DashboardMetricasResponse metricas)
    {
        TotalProyectos = metricas.TotalProyectos;
        TotalColaboradores = metricas.ColaboradoresActivos;
        HorasRegistradasMes = metricas.HorasReportadas;
        ClientesActivos = metricas.ClientesActivos;
        DetalleTotalHoras = $"{metricas.HorasReportadas:0} h";

        // Icon usa el campo que StatCard ya tenía (antes pensado para un
        // path de imagen, ej. "icon_projects.png"); aquí guardamos un
        // emoji directo, siguiendo la misma convención que ya usan otras
        // pantallas de la app (emojis en vez de fuentes de íconos).
        StatCards.Clear();
        StatCards.Add(new StatCard { Icon = "📁", IconColor = "#3559D6", Value = metricas.TotalProyectos.ToString(), Label = "Proyectos\nen total", IconBackground = "#E7EEFF" });
        StatCards.Add(new StatCard { Icon = "⏱", IconColor = "#168A52", Value = $"{metricas.HorasReportadas:0} h", Label = "Horas\nreportadas", IconBackground = "#E1F8E9" });
        StatCards.Add(new StatCard { Icon = "👥", IconColor = "#C9781A", Value = metricas.ColaboradoresActivos.ToString(), Label = "Colaboradores\nactivos", IconBackground = "#FFF0DE" });
        StatCards.Add(new StatCard { Icon = "🏢", IconColor = "#7A58D1", Value = metricas.ClientesActivos.ToString(), Label = "Clientes\nactivos", IconBackground = "#F1E9FF" });

        var promedio = metricas.TotalProyectos > 0
            ? metricas.HorasReportadas / metricas.TotalProyectos
            : 0m;

        MetricasMini.Clear();
        MetricasMini.Add(new MetricMini { Titulo = "TOTAL DE HORAS", Valor = $"{metricas.HorasReportadas:0} h" });
        MetricasMini.Add(new MetricMini { Titulo = "PROMEDIO DE HORAS", Valor = $"{promedio:0.#} h" });
        // "AVANCE DE JORNADA" queda pendiente: el endpoint no expone horas
        // planeadas vs. horas trabajadas del día/mes actual todavía.
        MetricasMini.Add(new MetricMini { Titulo = "AVANCE DE JORNADA", Valor = "0%" });
    }

    private void MapProximosACerrar(List<ProximoACerrarResponse>? items)
    {
        ProximosACerrar.Clear();
        if (items == null) return;

        foreach (var item in items)
        {
            ProximosACerrar.Add(new ClosingProject
            {
                Codigo = item.Codigo,
                Titulo = item.Nombre,
                Cliente = item.Cliente,
                Estado = item.Estado,
                FechaCierre = item.FechaFinPlaneada ?? DateTime.MinValue,
                HorasRestantes = (int)item.Horas
            });
        }
    }

    private void MapHorasPorProyecto(List<HorasPorProyectoResponse>? items)
    {
        HorasPorProyecto.Clear();
        if (items == null || items.Count == 0) return;

        var maxHoras = items.Max(i => i.Horas);

        foreach (var item in items)
        {
            var porcentaje = maxHoras > 0
                ? (double)(item.Horas / maxHoras) * 100
                : 0;

            HorasPorProyecto.Add(new ChartPoint
            {
                Categoria = item.Proyecto,
                Horas = (double)item.Horas,
                Porcentaje = porcentaje
            });
        }
    }

    private void MapDetalleHoras(List<HorasPorProyectoResponse>? items)
    {
        DetalleHoras.Clear();
        if (items == null) return;

        var top = items.OrderByDescending(i => i.Horas).Take(4).ToList();

        for (int i = 0; i < top.Count; i++)
        {
            DetalleHoras.Add(new HourDetail
            {
                Color = DetallePalette[i % DetallePalette.Length],
                Proyecto = top[i].Proyecto,
                Horas = (double)top[i].Horas
            });
        }
    }
}