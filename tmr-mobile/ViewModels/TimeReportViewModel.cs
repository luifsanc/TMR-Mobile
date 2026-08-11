using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using tmr_mobile.Services;

namespace tmr_mobile.ViewModels;

public class ActividadItem
{
    public int Id { get; set; }
    public string Proyecto { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string TipoActividad { get; set; } = string.Empty;
    public decimal Horas { get; set; }
    public DateTime Fecha { get; set; }
}

public sealed class ResumenHorasApiResponse
{
    public decimal HorasPorRegistrar { get; set; }
    public decimal HorasRegistradas { get; set; }
    public decimal HorasSemana { get; set; }
    public decimal HorasMes { get; set; }
}

public sealed class ActividadCalendarioApiResponse
{
    public int Id { get; set; }
    public int IdEmpleado { get; set; }
    public int? IdProyecto { get; set; }
    public string NombreProyecto { get; set; } = string.Empty;
    public int? IdTipoActividad { get; set; }
    public string NombreTipoActividad { get; set; } = string.Empty;
    public string CodigoRequerimiento { get; set; } = string.Empty;
    public decimal CantidadHoras { get; set; }
    public DateOnly FechaActividad { get; set; }
    public string DescripcionActividad { get; set; } = string.Empty;
    public string Notas { get; set; } = string.Empty;
    public bool EsBillable { get; set; }
}

public partial class TimeReportViewModel : BaseViewModel
{
    private readonly ApiService _apiService;
    private readonly IAuthService _authService;

    [ObservableProperty]
    private decimal _horasPorRegistrar;

    [ObservableProperty]
    private decimal _horasRegistradasHoy;

    [ObservableProperty]
    private decimal _horasSemana;

    [ObservableProperty]
    private decimal _horasMes;

    public ObservableCollection<ActividadItem> Actividades { get; } = new();

    public TimeReportViewModel(ApiService apiService, IAuthService authService)
    {
        _apiService = apiService;
        _authService = authService;
        Title = "Registro de Horas (Time Report)";
    }

    [RelayCommand]
    private async Task CargarActividadesAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var empleadoId = _authService.CurrentUser?.IdEmpleado;
            if (empleadoId is null or <= 0)
            {
                ErrorMessage = "Inicia sesión para ver tus horas registradas.";
                return;
            }

            var hoy = DateTime.Today;
            var resumen = await _apiService.GetAsync<ResumenHorasApiResponse>($"time-report/actividades/resumen?idEmpleado={empleadoId}&anio={hoy.Year}&mes={hoy.Month}");
            if (resumen is not null)
            {
                HorasPorRegistrar = resumen.HorasPorRegistrar;
                HorasRegistradasHoy = resumen.HorasRegistradas;
                HorasSemana = resumen.HorasSemana;
                HorasMes = resumen.HorasMes;
            }

            var actividades = await _apiService.GetAsync<List<ActividadCalendarioApiResponse>>($"time-report/actividades/calendario?idEmpleado={empleadoId}&anio={hoy.Year}&mes={hoy.Month}");
            Actividades.Clear();
            foreach (var actividad in actividades ?? new List<ActividadCalendarioApiResponse>())
            {
                Actividades.Add(new ActividadItem
                {
                    Id = actividad.Id,
                    Proyecto = string.IsNullOrWhiteSpace(actividad.NombreProyecto) ? "Sin proyecto" : actividad.NombreProyecto,
                    Descripcion = string.IsNullOrWhiteSpace(actividad.DescripcionActividad) ? "Sin descripción" : actividad.DescripcionActividad,
                    TipoActividad = string.IsNullOrWhiteSpace(actividad.NombreTipoActividad) ? "Otro" : actividad.NombreTipoActividad,
                    Horas = actividad.CantidadHoras,
                    Fecha = actividad.FechaActividad.ToDateTime(TimeOnly.MinValue)
                });
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al cargar el time report: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
