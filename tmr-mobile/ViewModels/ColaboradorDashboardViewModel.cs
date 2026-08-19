using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using tmr_mobile.Services;
using tmr_shared.DTOs.TimeReport;

namespace tmr_mobile.ViewModels;

public partial class ColaboradorDashboardViewModel : BaseViewModel
{
    private readonly ApiService _apiService;
    private readonly IAuthService _authService;

    [ObservableProperty]
    public partial string NombreUsuario { get; set; }

    [ObservableProperty] public partial string HorasHoyTexto { get; set; } = "0 de 8 h";
    [ObservableProperty] public partial string EstadoHoy { get; set; } = "Sin registros";
    [ObservableProperty] public partial string HorasSemanaTexto { get; set; } = "0 de 40 h";
    [ObservableProperty] public partial double ProgresoSemana { get; set; }
    [ObservableProperty] public partial string DiasSemanaTexto { get; set; } = "0 días completos";
    public ObservableCollection<ActividadRecienteItem> ActividadesRecientes { get; } = new();
    public ObservableCollection<ProyectoColaboradorItem> Proyectos { get; } = new();

    public bool TieneActividades => ActividadesRecientes.Count > 0;
    public bool TieneProyectos => Proyectos.Count > 0;

    public ColaboradorDashboardViewModel(ApiService apiService, IAuthService authService)
    {
        _apiService = apiService;
        _authService = authService;
        Title = "Inicio";
        NombreUsuario = authService.CurrentUser?.Name ?? "Usuario";
    }

    [RelayCommand]
    private async Task CargarAsync()
    {
        var idEmpleado = _authService.CurrentUser?.IdEmpleado;
        if (idEmpleado is null)
        {
            ErrorMessage = "No se encontró el colaborador vinculado a tu cuenta.";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var hoy = DateTime.Today;
            var resumenTask = _apiService.GetAsync<ResumenHorasDto>(
                $"api/time-report/actividades/resumen?idEmpleado={idEmpleado}&anio={hoy.Year}&mes={hoy.Month}");
            var actividadesTask = _apiService.GetAsync<List<CalendarioActividadDto>>(
                $"api/time-report/actividades/calendario?idEmpleado={idEmpleado}&anio={hoy.Year}&mes={hoy.Month}");
            var proyectosTask = _apiService.GetAsync<List<ProyectoLookupDto>>(
                "api/time-report/actividades/proyectos-disponibles");
            await Task.WhenAll(resumenTask, actividadesTask, proyectosTask);

            MapearResumen(await resumenTask, await actividadesTask ?? []);
            MapearActividades(await actividadesTask ?? []);
            MapearProyectos(await proyectosTask ?? [], await actividadesTask ?? []);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"No se pudo cargar tu resumen: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void MapearResumen(ResumenHorasDto? resumen, List<CalendarioActividadDto> actividades)
    {
        var horasHoy = resumen?.HorasRegistradas ?? 0;
        var horasSemana = resumen?.HorasSemana ?? 0;
        HorasHoyTexto = $"{horasHoy:0.#} de 8 h";
        EstadoHoy = horasHoy >= 8 ? "Jornada completa" : horasHoy > 0 ? $"Faltan {8 - horasHoy:0.#} h" : "Sin registros";
        HorasSemanaTexto = $"{horasSemana:0.#} de 40 h";
        ProgresoSemana = Math.Clamp((double)(horasSemana / 40m), 0, 1);

        var inicioSemana = DateOnly.FromDateTime(DateTime.Today.AddDays(-((int)DateTime.Today.DayOfWeek + 6) % 7));
        var completos = actividades
            .Where(a => a.FechaActividad >= inicioSemana)
            .GroupBy(a => a.FechaActividad)
            .Count(g => g.Sum(a => a.CantidadHoras) >= 8);
        DiasSemanaTexto = $"{completos} {(completos == 1 ? "día completo" : "días completos")}";
    }

    private void MapearActividades(List<CalendarioActividadDto> actividades)
    {
        ActividadesRecientes.Clear();
        foreach (var actividad in actividades.OrderByDescending(a => a.FechaActividad).ThenByDescending(a => a.Id).Take(3))
        {
            ActividadesRecientes.Add(new ActividadRecienteItem(
                actividad.ProyectoNombre,
                actividad.DescripcionActividad,
                actividad.FechaActividad.ToString("dd/MM/yyyy"),
                $"{actividad.CantidadHoras:0.##} h"));
        }
        OnPropertyChanged(nameof(TieneActividades));
    }

    private void MapearProyectos(List<ProyectoLookupDto> proyectos, List<CalendarioActividadDto> actividades)
    {
        Proyectos.Clear();
        foreach (var proyecto in proyectos.Take(4))
        {
            var horas = actividades.Where(a => a.IdProyecto == proyecto.Id).Sum(a => a.CantidadHoras);
            Proyectos.Add(new ProyectoColaboradorItem(proyecto.Nombre, proyecto.Codigo ?? "PROYECTO", $"{horas:0.##} h este mes"));
        }
        OnPropertyChanged(nameof(TieneProyectos));
    }

    [RelayCommand]
    private Task AbrirActividadesAsync() => Shell.Current.GoToAsync("//TimeReportPage");

    [RelayCommand]
    private Task AbrirCargaActividadesAsync() => Shell.Current.GoToAsync("//CargaActividadesPage");
}

public record ActividadRecienteItem(string Proyecto, string Descripcion, string Fecha, string Horas);
public record ProyectoColaboradorItem(string Nombre, string Codigo, string HorasMes);
