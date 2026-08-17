using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using tmr_mobile.Resources.Styles;
using tmr_mobile.Services;
using tmr_mobile.Views.Dashboard.Models;
using tmr_shared.DTOs.Dashboard;
namespace tmr_mobile.ViewModels;

public partial class DashboardViewModel : BaseViewModel
{
    private readonly ApiService _apiService;
    private readonly IAuthService _authService;

    // Paleta usada para "Detalle de horas" (derivado de HorasPorProyecto,
    // ya que el endpoint no trae un color por proyecto).
    private static readonly string[] DetallePalette =
    {
        DesignColors.Primary,
        DesignColors.Accent,
        DesignColors.Warning,
        DesignColors.PrimaryDark,
        DesignColors.Danger,
        DesignColors.TextMuted
    };

    [ObservableProperty]
    public partial int TotalProyectos { get; set; }

    [ObservableProperty]
    public partial int TotalColaboradores { get; set; }

    [ObservableProperty]
    public partial decimal HorasRegistradasMes { get; set; }

    [ObservableProperty]
    public partial int ClientesActivos { get; set; }

    [ObservableProperty]
    public partial bool TieneNotificaciones { get; set; } = false;

    [ObservableProperty]
    public partial string DetalleTotalHoras { get; set; } = "-";



    public DashboardViewModel(ApiService apiService, IAuthService authService)
    {
        _apiService = apiService;
        _authService = authService;
        Title = "Dashboard Principal";

        // Leer el nombre del usuario logueado
        NombreUsuario = _authService.CurrentUser?.Name ?? "Usuario";

        // Carga inicial. CargarDashboardCommand es un IAsyncRelayCommand
        // generado por [RelayCommand]; Execute maneja el fire-and-forget
        // de forma segura (las excepciones ya se capturan adentro).
        CargarDashboardCommand.Execute(null);
    }

    [ObservableProperty]
    public partial string NombreUsuario { get; set; } = "Usuario";

    // Cuando NombreUsuario cambia, notificar también AvatarInicial (getter calculado)
    partial void OnNombreUsuarioChanged(string value) => OnPropertyChanged(nameof(AvatarInicial));

    public string DetalleTitulo { get; set; } = "Detalle por proyecto";

    // Inicial para el avatar: primera letra del nombre del usuario logueado.
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
    public System.Collections.ObjectModel.ObservableCollection<ClientDistribution> ProyectosPorCliente { get; } = new();
    public System.Collections.ObjectModel.ObservableCollection<ClientDistribution> ProyectosPorClienteVisibles { get; } = new();

    [RelayCommand]
    private async Task CargarDashboardAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var response = await _apiService.GetAsync<DashboardDataResponse>($"dashboard?rango={RangoSeleccionado}");

            if (response?.Metricas == null)
            {
                ErrorMessage = "No se pudieron cargar los datos del dashboard.";
                return;
            }

            MapMetricas(response.Metricas);
            MapProximosACerrar(response.ProximosACerrar);
            MapHorasPorProyecto(response.HorasPorProyecto);
            ActualizarMetricasMini();
            MapDetalleHoras(response.HorasPorProyecto);
            MapProyectosPorCliente(response.ProyectosPorCliente);
            await CargarEstadoNotificacionesAsync();
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

    private async Task CargarEstadoNotificacionesAsync()
    {
        try
        {
            var response = await _apiService.GetAsync<HorasIncompletasResponse>(
                "dashboard/mis-horas-incompletas?rango=mes");
            TieneNotificaciones = response?.TieneFaltantes == true;
        }
        catch
        {
            TieneNotificaciones = false;
        }
    }

    private void ActualizarMetricasMini()
    {
        var totalHoras = HorasPorProyecto.Sum(h => h.Horas);
        var cantidadProyectos = HorasPorProyecto.Count;
        var promedio = cantidadProyectos > 0 ? totalHoras / cantidadProyectos : 0;

        MetricasMini.Clear();
        MetricasMini.Add(new MetricMini { Titulo = "TOTAL DE HORAS", Valor = $"{totalHoras:0} h" });
        MetricasMini.Add(new MetricMini { Titulo = "PROMEDIO DE HORAS", Valor = $"{promedio:0} h" });
        // "AVANCE DE JORNADA" sigue en 0%: el endpoint no expone
        // horas planeadas vs. trabajadas del período actual todavía.
        MetricasMini.Add(new MetricMini
        {
            Titulo = "AVANCE DE JORNADA",
            Valor = "0%"
        });
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
        StatCards.Add(new StatCard { Icon = "📁", IconColor = DesignColors.Primary, Value = metricas.TotalProyectos.ToString(), Label = "Proyectos\nen total", IconBackground = DesignColors.PrimaryLight });
        StatCards.Add(new StatCard { Icon = "⏱", IconColor = DesignColors.Accent, Value = $"{metricas.HorasReportadas:0} h", Label = "Horas\nreportadas", IconBackground = DesignColors.SuccessSurface });
        StatCards.Add(new StatCard { Icon = "👥", IconColor = DesignColors.Warning, Value = metricas.ColaboradoresActivos.ToString(), Label = "Colaboradores\nactivos", IconBackground = DesignColors.WarningSurface });
        StatCards.Add(new StatCard { Icon = "🏢", IconColor = DesignColors.PrimaryDark, Value = metricas.ClientesActivos.ToString(), Label = "Clientes\nactivos", IconBackground = DesignColors.PrimaryLight });

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
                Horas = (int)item.Horas
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


    private void MapProyectosPorCliente(List<ProyectoPorClienteResponse>? proyectosPorCliente)
    {
        ProyectosPorCliente.Clear();
        if (proyectosPorCliente == null || proyectosPorCliente.Count == 0) return;

        // El endpoint ya trae el porcentaje calculado, así que no hace falta
        // sacar un máximo como en MapHorasPorProyecto.
        foreach (var item in proyectosPorCliente)
        {
            ProyectosPorCliente.Add(new ClientDistribution
            {
                Cliente = item.Cliente,
                Proyectos = item.ProyectosAsignados,
                Porcentaje = item.Porcentaje
            });
        }
        ActualizarProyectosPorClienteVisibles();
    }

    private const int TopClientesVisible = 8;

    [ObservableProperty]
    public partial bool MostrarTodosClientes { get; set; }

    public string TextoVerMasClientes => MostrarTodosClientes ? "Ver menos" : "Ver más";

    partial void OnMostrarTodosClientesChanged(bool value)
    {
        OnPropertyChanged(nameof(TextoVerMasClientes));
        ActualizarProyectosPorClienteVisibles();
    }

    [RelayCommand]
    private void ToggleVerMasClientes()
    {
        MostrarTodosClientes = !MostrarTodosClientes;
    }

    private void ActualizarProyectosPorClienteVisibles()
    {
        ProyectosPorClienteVisibles.Clear();
        var fuente = MostrarTodosClientes
            ? ProyectosPorCliente
            : ProyectosPorCliente.Take(TopClientesVisible);

        foreach (var item in fuente)
            ProyectosPorClienteVisibles.Add(item);
    }

    [ObservableProperty]
    public partial string RangoSeleccionado { get; set; } = "mes";

    [ObservableProperty]
    public partial string RangoLabel { get; set; } = "Este mes";

    private static readonly (string Valor, string Texto)[] RangosDisponibles =
    {
    ("mes", "Este mes"),
    ("trimestre", "Este trimestre"),
    ("anio", "Este año")
};

    [RelayCommand]
    [Obsolete]
    private async Task ToggleSelectorRangoAsync()
    {
        var opciones = RangosDisponibles.Select(r => r.Texto).ToArray();

        string seleccion = await Application.Current!.MainPage!.DisplayActionSheet(
            "Selecciona un rango",
            "Cancelar",
            null,
            opciones);

        if (string.IsNullOrEmpty(seleccion) || seleccion == "Cancelar")
            return;

        var rango = RangosDisponibles.FirstOrDefault(r => r.Texto == seleccion);
        if (rango.Valor is null || rango.Valor == RangoSeleccionado)
            return;

        RangoSeleccionado = rango.Valor;
        RangoLabel = rango.Texto;

        await CargarDashboardAsync();
    }


}
