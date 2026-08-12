using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using tmr_mobile.Services;
using tmr_shared.DTOs.TimeReport;

namespace tmr_mobile.ViewModels;

public partial class CrearActividadViewModel : BaseViewModel, IQueryAttributable
{
    private readonly ApiService _apiService;
    private readonly IAuthService _authService;

    [ObservableProperty]
    private DateTime _fechaActividad = DateTime.Today;

    [ObservableProperty]
    private ProyectoLookupDto _proyectoSeleccionado;

    [ObservableProperty]
    private TipoActividadDto _tipoActividadSeleccionada;

    [ObservableProperty]
    private decimal _cantidadHoras = 1;

    [ObservableProperty]
    private string _descripcionActividad = string.Empty;

    [ObservableProperty]
    private string _notas = string.Empty;

    [ObservableProperty]
    private string _codigoRequerimiento = string.Empty;

    [ObservableProperty]
    private bool _esBillable = true;

    [ObservableProperty]
    private bool _isRecurrente;

    [ObservableProperty]
    private DateTime _fechaInicio = DateTime.Today;

    [ObservableProperty]
    private DateTime _fechaFin = DateTime.Today.AddDays(4);

    [ObservableProperty]
    private decimal _horasPorDia = 4;

    [ObservableProperty]
    private bool _incluirFinesSemana = true;

    [ObservableProperty]
    private bool _incluirFeriados;

    public string MensajeRecurrente
    {
        get
        {
            if (!IsRecurrente) return string.Empty;
            int count = CalcularDiasRecurrentes();
            return $"Se crearán {count} actividades entre {FechaInicio:dd/MM/yyyy} y {FechaFin:dd/MM/yyyy}";
        }
    }

    private int CalcularDiasRecurrentes()
    {
        if (FechaFin < FechaInicio) return 0;
        int count = 0;
        for (var date = FechaInicio.Date; date <= FechaFin.Date; date = date.AddDays(1))
        {
            if (!IncluirFinesSemana && (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday))
                continue;
            count++;
        }
        return count;
    }

    partial void OnIsRecurrenteChanged(bool value) => OnPropertyChanged(nameof(MensajeRecurrente));
    partial void OnFechaInicioChanged(DateTime value) => OnPropertyChanged(nameof(MensajeRecurrente));
    partial void OnFechaFinChanged(DateTime value) => OnPropertyChanged(nameof(MensajeRecurrente));
    partial void OnIncluirFinesSemanaChanged(bool value) => OnPropertyChanged(nameof(MensajeRecurrente));
    partial void OnIncluirFeriadosChanged(bool value) => OnPropertyChanged(nameof(MensajeRecurrente));


    public ObservableCollection<ProyectoLookupDto> Proyectos { get; } = new();
    public ObservableCollection<TipoActividadDto> TiposActividad { get; } = new();

    public CrearActividadViewModel(ApiService apiService, IAuthService authService)
    {
        _apiService = apiService;
        _authService = authService;
        Title = "Registrar Actividad";
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.ContainsKey("FechaActividad") && query["FechaActividad"] is DateTime date)
        {
            FechaActividad = date;
        }
    }

    [RelayCommand]
    private async Task CargarDatosAsync()
    {
        IsBusy = true;
        try
        {
            var tiposTask = _apiService.GetAsync<List<TipoActividadDto>>("api/time-report/actividades/tipos-actividad");
            var proyectosTask = _apiService.GetAsync<List<ProyectoLookupDto>>("api/time-report/actividades/proyectos-disponibles");

            await Task.WhenAll(tiposTask, proyectosTask);

            TiposActividad.Clear();
            if (tiposTask.Result != null)
            {
                foreach (var tipo in tiposTask.Result)
                {
                    TiposActividad.Add(tipo);
                }
            }

            Proyectos.Clear();
            if (proyectosTask.Result != null)
            {
                foreach (var proy in proyectosTask.Result)
                {
                    Proyectos.Add(proy);
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al cargar datos: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task GuardarActividadAsync()
    {
        if (TipoActividadSeleccionada == null)
        {
            await Shell.Current.DisplayAlertAsync("Error", "Seleccione un tipo de actividad.", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(DescripcionActividad))
        {
            await Shell.Current.DisplayAlertAsync("Error", "Ingrese una descripción.", "OK");
            return;
        }

        if (CantidadHoras <= 0)
        {
            await Shell.Current.DisplayAlertAsync("Error", "La cantidad de horas debe ser mayor a 0.", "OK");
            return;
        }

        var idEmpleado = _authService.CurrentUser?.IdEmpleado;
        if (idEmpleado == null)
        {
            await Shell.Current.DisplayAlertAsync("Error", "No se encontró el empleado.", "OK");
            return;
        }

        IsBusy = true;
        try
        {
            if (IsRecurrente)
            {
                int count = 0;
                for (var date = FechaInicio.Date; date <= FechaFin.Date; date = date.AddDays(1))
                {
                    if (!IncluirFinesSemana && (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday))
                        continue;

                    var req = new CrearActividadDto(
                        IdEmpleado: idEmpleado.Value,
                        IdProyecto: ProyectoSeleccionado?.Id,
                        IdTipoActividad: TipoActividadSeleccionada.Id,
                        CodigoRequerimiento: string.IsNullOrWhiteSpace(CodigoRequerimiento) ? null : CodigoRequerimiento,
                        CantidadHoras: HorasPorDia,
                        FechaActividad: DateOnly.FromDateTime(date),
                        DescripcionActividad: DescripcionActividad,
                        Notas: string.IsNullOrWhiteSpace(Notas) ? null : Notas,
                        EsBillable: EsBillable
                    );
                    var result = await _apiService.PostAsync<CrearActividadDto, CalendarioActividadDto>("api/time-report/actividades", req);
                    if (result != null) count++;
                }

                if (count > 0)
                {
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    await Shell.Current.DisplayAlertAsync("Error", "No se pudo registrar las actividades.", "OK");
                }
            }
            else
            {
                var req = new CrearActividadDto(
                    IdEmpleado: idEmpleado.Value,
                    IdProyecto: ProyectoSeleccionado?.Id,
                    IdTipoActividad: TipoActividadSeleccionada.Id,
                    CodigoRequerimiento: string.IsNullOrWhiteSpace(CodigoRequerimiento) ? null : CodigoRequerimiento,
                    CantidadHoras: CantidadHoras,
                    FechaActividad: DateOnly.FromDateTime(FechaActividad),
                    DescripcionActividad: DescripcionActividad,
                    Notas: string.IsNullOrWhiteSpace(Notas) ? null : Notas,
                    EsBillable: EsBillable
                );

                var result = await _apiService.PostAsync<CrearActividadDto, CalendarioActividadDto>("api/time-report/actividades", req);
                
                if (result != null)
                {
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    await Shell.Current.DisplayAlertAsync("Error", "No se pudo registrar la actividad.", "OK");
                }
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", $"Error: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CancelarAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
