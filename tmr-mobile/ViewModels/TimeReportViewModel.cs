using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using tmr_mobile.Services;
using tmr_shared.DTOs.TimeReport;

namespace tmr_mobile.ViewModels;

public partial class TimeReportViewModel : BaseViewModel
{
    private readonly ApiService    _apiService;
    private readonly IAuthService  _authService;

    public ObservableCollection<CalendarioActividadDto> Actividades { get; } = new();

    [ObservableProperty]
    public partial int AnioActual { get; set; } = DateTime.Now.Year;

    [ObservableProperty]
    public partial int MesActual { get; set; } = DateTime.Now.Month;

    [ObservableProperty]
    public partial decimal TotalHorasMes { get; set; }

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

            Actividades.Clear();
            if (lista != null)
            {
                foreach (var a in lista)
                    Actividades.Add(a);

                TotalHorasMes = lista.Sum(a => a.CantidadHoras);
            }
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
        var nombre = await Shell.Current.DisplayPromptAsync("Nuevo Registro", "Ingresa el nombre del registro de tiempo:");
        if (string.IsNullOrWhiteSpace(nombre)) return;

        var descripcion = await Shell.Current.DisplayPromptAsync("Nuevo Registro", "Ingresa la descripción:");
        
        IsBusy = true;
        try
        {
            var req = new CrearRegistroTiempoRequest 
            { 
                Nombre = nombre, 
                Descripcion = descripcion ?? string.Empty 
            };
            
            // Llama al endpoint POST /api/time-report usando el ApiService (hacia DEV)
            var result = await _apiService.PostAsync<CrearRegistroTiempoRequest, RegistroTiempoResponse>("api/time-report", req);
            
            if (result != null)
            {
                await Shell.Current.DisplayAlertAsync("Éxito", $"Registro '{result.Nombre}' (ID: {result.Id}) creado en el servidor DEV.", "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", $"No se pudo crear el registro: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
