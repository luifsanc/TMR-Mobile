using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
            // GET /api/proyectos
            var lista = await _apiService.GetAsync<List<ProyectoResponse>>("api/proyectos");
            _listaCompleta = lista ?? new();
            Proyectos.Clear();
            Console.WriteLine($"[ProyectosViewModel] GET /api/proyectos -> {(lista == null ? "null" : lista.Count.ToString())} items");
            if (lista != null)
            {
                foreach (var p in lista)
                    Proyectos.Add(p);
                if (lista.Count == 0)
                {
                    ErrorMessage = "No hay proyectos disponibles.";
                    Console.WriteLine("[ProyectosViewModel] Lista vacía (count=0)");
                }
            }
            else
            {
                // Mostrar mensaje informativo cuando la respuesta viene vacía
                ErrorMessage = "No se recibieron proyectos del servidor.";
                Console.WriteLine("[ProyectosViewModel] No se recibieron proyectos del servidor (null)");
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al cargar proyectos: {ex.Message}";
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
        MostrarFiltroEstado = !MostrarFiltroEstado;
    }

    [RelayCommand]
    private async Task DescargarAsync()
    {
        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            // Crear CSV con los proyectos
            var csv = "ID,Nombre,Cliente,Estado,Fechas\n";
            foreach (var p in Proyectos)
            {
                csv += $"{p.Id},\"{p.Nombre}\",\"{p.Cliente}\",{p.Estado},\"{p.FechaInicio:dd/MM/yyyy} - {p.FechaFin:dd/MM/yyyy}\"\n";
            }

            // Guardar archivo en documentos
            var rutaDocumentos = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var nombreArchivo = $"Proyectos_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var rutaArchivo = Path.Combine(rutaDocumentos, nombreArchivo);
            
            await File.WriteAllTextAsync(rutaArchivo, csv);
            
            await Application.Current!.MainPage!.DisplayAlert("Éxito", $"Archivo descargado en:\n{rutaArchivo}", "OK");
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

        Proyectos.Clear();
        foreach (var p in filtrados)
        {
            Proyectos.Add(p);
        }
    }

    [RelayCommand]
    private async Task NuevoAsync()
    {
        await Shell.Current.GoToAsync(nameof(ProyectosFormPage));
    }

    [RelayCommand]
    private async Task AbrirDetalleAsync(ProyectoResponse proyecto)
    {
        if (proyecto == null) return;
        
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
            var result = await _apiService.PostForResultAsync<object>($"api/proyectos/{id}/inactivate", new { });

            if (result.Success)
            {
                // Refrescar lista
                CargarProyectosCommand.Execute(null);
            }
            else
            {
                ErrorMessage = string.IsNullOrEmpty(result.Message) ? "No se pudo inactivar el proyecto." : result.Message;
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
}
