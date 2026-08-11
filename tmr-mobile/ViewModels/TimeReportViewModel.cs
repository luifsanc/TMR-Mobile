using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using tmr_mobile.Services;
using tmr_shared.DTOs.TimeReport;
using tmr_mobile.Models;

namespace tmr_mobile.ViewModels;

public partial class TimeReportViewModel : BaseViewModel
{
    private readonly ApiService    _apiService;
    private readonly IAuthService  _authService;

    public ObservableCollection<CalendarioActividadDto> Actividades { get; } = new();
    public ObservableCollection<CalendarioActividadDto> ActividadesDelDia { get; } = new();
    public ObservableCollection<DayModel> Days { get; } = new();

    [ObservableProperty]
    public partial int AnioActual { get; set; } = DateTime.Now.Year;

    [ObservableProperty]
    public partial int MesActual { get; set; } = DateTime.Now.Month;

    [ObservableProperty]
    public partial ResumenHorasDto Resumen { get; set; }

    [ObservableProperty]
    public partial DateTime FechaSeleccionada { get; set; } = DateTime.Today;

    [ObservableProperty]
    public partial DayModel DiaSeleccionado { get; set; }

    public TimeReportViewModel(ApiService apiService, IAuthService authService)
    {
        _apiService   = apiService;
        _authService  = authService;
        Title = "Registro de Horas (Time Report)";
    }

    [RelayCommand]
    private async Task CargarActividadesAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var idEmpleado = _authService.CurrentUser?.IdEmpleado;
            if (idEmpleado == null)
            {
                ErrorMessage = "No se encontró el empleado vinculado a tu cuenta.";
                return;
            }

            // GET /api/time-report/actividades/calendario?idEmpleado=&anio=&mes=
            var url = $"api/time-report/actividades/calendario?idEmpleado={idEmpleado}&anio={AnioActual}&mes={MesActual}";
            var lista = await _apiService.GetAsync<List<CalendarioActividadDto>>(url);

            var urlResumen = $"api/time-report/actividades/resumen?idEmpleado={idEmpleado}&anio={AnioActual}&mes={MesActual}";
            var resumen = await _apiService.GetAsync<ResumenHorasDto>(urlResumen);

            if (resumen != null)
                Resumen = resumen;

            Actividades.Clear();
            if (lista != null)
            {
                foreach (var a in lista)
                    Actividades.Add(a);
            }

            GenerarCalendario();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al cargar actividades: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void GenerarCalendario()
    {
        Days.Clear();
        var primerDia = new DateTime(AnioActual, MesActual, 1);
        
        int diaSemanaPrimerDia = (int)primerDia.DayOfWeek; // Sun=0, Mon=1...
        int offset = diaSemanaPrimerDia == 0 ? 6 : diaSemanaPrimerDia - 1; // Mon=0

        var fechaActual = primerDia.AddDays(-offset);
        
        for (int i = 0; i < 42; i++)
        {
            var isCurrentMonth = fechaActual.Month == MesActual;
            var isToday = fechaActual.Date == DateTime.Today;
            var hasActivities = Actividades.Any(a => a.FechaActividad.ToDateTime(TimeOnly.MinValue).Date == fechaActual.Date);
            
            var dayModel = new DayModel
            {
                Date = fechaActual,
                IsCurrentMonth = isCurrentMonth,
                IsToday = isToday,
                HasActivities = hasActivities
            };

            if (fechaActual.Date == FechaSeleccionada.Date)
            {
                dayModel.IsSelected = true;
                DiaSeleccionado = dayModel;
            }

            Days.Add(dayModel);
            fechaActual = fechaActual.AddDays(1);
        }

        ActualizarActividadesDelDia();
    }

    [RelayCommand]
    private void SeleccionarDia(DayModel day)
    {
        if (day == null) return;
        
        if (DiaSeleccionado != null)
            DiaSeleccionado.IsSelected = false;
            
        day.IsSelected = true;
        DiaSeleccionado = day;
        FechaSeleccionada = day.Date;
        
        ActualizarActividadesDelDia();
    }

    private void ActualizarActividadesDelDia()
    {
        ActividadesDelDia.Clear();
        var listaDia = Actividades.Where(a => a.FechaActividad.ToDateTime(TimeOnly.MinValue).Date == FechaSeleccionada.Date);
        foreach (var act in listaDia)
        {
            ActividadesDelDia.Add(act);
        }
    }

    [RelayCommand]
    private async Task MesAnteriorAsync()
    {
        if (MesActual == 1)
        {
            MesActual  = 12;
            AnioActual--;
        }
        else
        {
            MesActual--;
        }
        await CargarActividadesAsync();
    }

    [RelayCommand]
    private async Task MesSiguienteAsync()
    {
        if (MesActual == 12)
        {
            MesActual  = 1;
            AnioActual++;
        }
        else
        {
            MesActual++;
        }
        await CargarActividadesAsync();
    }

    [RelayCommand]
    private async Task NuevaActividadAsync()
    {
        var query = new Dictionary<string, object>
        {
            { "FechaActividad", FechaSeleccionada }
        };
        await Shell.Current.GoToAsync("CrearActividadPage", query);
    }
}
