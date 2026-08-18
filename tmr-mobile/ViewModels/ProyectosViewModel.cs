using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using tmr_mobile.Models.Operaciones;
using tmr_mobile.Services;
using tmr_mobile.Views.Operaciones;
using tmr_shared.DTOs.Proyectos;

namespace tmr_mobile.ViewModels;

public partial class ProyectosViewModel : BaseViewModel
{
    private readonly ApiService _apiService;

    public ObservableCollection<ProyectoResponse> Proyectos { get; } = new();

    [ObservableProperty]
    private string textoBusqueda = string.Empty;

    [ObservableProperty]
    private bool mostrarFiltroEstado = false;

    private List<ProyectoResponse> _listaCompleta = new();
    private string _filtroEstado = "Todos";

    // Caché con timestamp para evitar solicitudes repetidas
    private List<ProyectoResponse>? _cachedProyectos;
    private DateTime _cacheTimestamp = DateTime.MinValue;
    private const int CacheDurationMinutes = 5;

    public ProyectosViewModel(ApiService apiService)
    {
        _apiService = apiService;
        Title = "Gestión de Proyectos";
    }

    [RelayCommand]
    private async Task CargarProyectosAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            // Verificar si el caché es válido (menos de 5 minutos)
            if (_cachedProyectos != null &&
                (DateTime.UtcNow - _cacheTimestamp).TotalMinutes < CacheDurationMinutes)
            {
                Console.WriteLine($"[ProyectosViewModel] Usando caché (edad: {(DateTime.UtcNow - _cacheTimestamp).TotalSeconds:F1}s)");
                _listaCompleta = new List<ProyectoResponse>(_cachedProyectos);
                Proyectos.Clear();
                foreach (var p in _listaCompleta)
                    Proyectos.Add(p);

                if (_listaCompleta.Count == 0)
                    ErrorMessage = "No hay proyectos disponibles.";

                return;
            }

            // Cargar desde la API directamente (sin Task.Run para evitar latencia)
            var lista = await _apiService.GetAsync<List<ProyectoResponse>>("proyectos");
            _listaCompleta = lista ?? new();
            _cachedProyectos = new List<ProyectoResponse>(_listaCompleta);
            _cacheTimestamp = DateTime.UtcNow;

            Console.WriteLine($"[ProyectosViewModel] GET /api/proyectos -> {_listaCompleta.Count} items (desde API)");

            Proyectos.Clear();
            if (_listaCompleta.Count == 0)
            {
                ErrorMessage = "No hay proyectos disponibles.";
                Console.WriteLine("[ProyectosViewModel] Lista vacía (count=0)");
                return;
            }

            foreach (var p in _listaCompleta)
                Proyectos.Add(p);
        }
        catch (InvalidOperationException ex)
        {
            ErrorMessage = ex.Message;
            Console.WriteLine($"[ProyectosViewModel] Sesión inválida cargando proyectos: {ex}");
        }
        catch (Exception ex)
        {
            ErrorMessage = "No se pudo cargar la información de proyectos. Revisa tu conexión o vuelve a iniciar sesión.";
            Console.WriteLine($"[ProyectosViewModel] Error cargando proyectos: {ex}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task BuscarAsync(string texto)
    {
        TextoBusqueda = texto ?? string.Empty;
        FiltrarProyectos();
    }

    [RelayCommand]
    private async Task FiltrarEstadoAsync()
    {
        var opcion = await Shell.Current.DisplayActionSheetAsync(
            "Filtrar por estado",
            "Cancelar",
            null,
            "Todos",
            "Activos",
            "Inactivos");

        if (string.IsNullOrWhiteSpace(opcion) || opcion == "Cancelar") return;

        _filtroEstado = opcion;
        FiltrarProyectos();
    }

    [RelayCommand]
    private async Task DescargarAsync()
    {
        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            var encabezados = new[] { "Código", "Nombre", "Cliente", "Líder", "Estado", "Inicio", "Fin" };
            var filas = Proyectos.Select(p => new[]
            {
                p.Codigo,
                p.Nombre,
                p.Cliente,
                p.LiderAsignado,
                p.Estado,
                p.FechaInicio?.ToString("dd/MM/yyyy") ?? "-",
                p.FechaFin?.ToString("dd/MM/yyyy") ?? "-"
            }).ToList();

            await ReportService.SeleccionarYExportarAsync(
                "Reporte de Proyectos", encabezados, filas, "Proyectos");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al descargar: {ex.Message}";
            await Application.Current!.MainPage!.DisplayAlert("Error", ErrorMessage, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void FiltrarProyectos()
    {
        var filtrados = _listaCompleta.AsEnumerable();

        // Filtrar por búsqueda de texto
        if (!string.IsNullOrWhiteSpace(TextoBusqueda))
        {
            var texto = TextoBusqueda.ToLower();
            filtrados = filtrados.Where(p =>
                p.Nombre.ToLower().Contains(texto) ||
                p.Cliente.ToLower().Contains(texto)
            );
        }

        if (_filtroEstado == "Activos")
            filtrados = filtrados.Where(p => p.Activo || p.Estado.Equals("Activo", StringComparison.OrdinalIgnoreCase));
        else if (_filtroEstado == "Inactivos")
            filtrados = filtrados.Where(p => !p.Activo || p.Estado.Equals("Inactivo", StringComparison.OrdinalIgnoreCase));

        Proyectos.Clear();
        foreach (var p in filtrados)
        {
            Proyectos.Add(p);
        }
    }

    [RelayCommand]
    private async Task NuevoAsync()
    {
        InvalidarCache();
        await Shell.Current.GoToAsync(nameof(ProyectosFormPage));
    }

    [RelayCommand]
    private async Task AbrirDetalleAsync(ProyectoResponse proyecto)
    {
        if (proyecto == null) return;

        InvalidarCache();
        var parametros = new Dictionary<string, object>
        {
            ["IdProyecto"] = proyecto.Id
        };

        await Shell.Current.GoToAsync(nameof(ProyectosFormPage), parametros);
    }

    public async Task InactivarProyectoAsync(int id)
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            // Intentamos llamar al endpoint de inactivación. Si el backend no tiene este endpoint,
            // el resultado será null y mostraremos el mensaje de error.
            var eliminado = await _apiService.DeleteAsync($"proyectos/{id}");

            if (eliminado)
            {
                InvalidarCache();
                await CargarProyectosAsync();
            }
            else
            {
                ErrorMessage = "No se pudo inactivar el proyecto.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "Error al inactivar el proyecto.";
            Console.WriteLine(ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    public void AbrirEditar(ProyectoItem proyecto)
    {
        if (proyecto is null) return;

        InvalidarCache();
        _ = Shell.Current.GoToAsync(nameof(ProyectosFormPage), new Dictionary<string, object>
        {
            ["IdProyecto"] = proyecto.Id
        });
    }

    public Task InactivarProyectoAsync(ProyectoItem proyecto) =>
        proyecto is null ? Task.CompletedTask : InactivarProyectoAsync(proyecto.Id);

    private void InvalidarCache()
    {
        _cachedProyectos = null;
        _cacheTimestamp = DateTime.MinValue;
    }
}
