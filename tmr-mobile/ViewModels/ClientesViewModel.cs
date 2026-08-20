using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using tmr_mobile.Models.Operaciones;
using tmr_mobile.Services;
using tmr_mobile.Views.Operaciones;

namespace tmr_mobile.ViewModels;

public partial class ClientesViewModel : BaseViewModel
{
    private readonly IClientesService _clientesService;
    
    private readonly List<ClienteModel> _todosClientes = new();

    public ObservableCollection<ClienteModel> Clientes { get; } = new();

    [ObservableProperty]
    public partial string Busqueda { get; set; } = string.Empty;

    // Resumen
    [ObservableProperty]
    public partial int TotalClientes { get; set; }

    [ObservableProperty]
    public partial int TotalActivos { get; set; }

    [ObservableProperty]
    public partial int TotalInactivos { get; set; }

    public ClientesViewModel(IClientesService clientesService)
    {
        _clientesService = clientesService;
        Title = "Clientes";
    }

    public async Task InicializarAsync()
    {
        // Evitamos recargar todo si ya tenemos datos, 
        // a menos que queramos forzar un refresh. 
        // En un flujo real, si venimos de editar/crear podríamos querer recargar.
        // Por ahora recargamos siempre que la colección esté vacía
        if (Clientes.Count == 0)
        {
            await CargarClientesAsync();
        }
        else
        {
            // Forzar recarga si venimos de otra pantalla (ej. crear/editar)
            // Esto asegura que la lista esté actualizada siempre
            await CargarClientesAsync();
        }
    }

    [RelayCommand]
    private async Task CargarClientesAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            var lista = await _clientesService.ObtenerClientesAsync(Busqueda);
            
            _todosClientes.Clear();
            _todosClientes.AddRange(lista.OrderByDescending(c => c.Id));

            ActualizarListadoYResumen();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Error al cargar clientes.";
            System.Diagnostics.Debug.WriteLine(ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnBusquedaChanged(string value)
    {
        // Opcional: Podríamos filtrar en memoria o llamar a la API
        // Si queremos llamar a la API:
        // CargarClientesAsync().Wait(); // Cuidado con .Wait() - mejor usar un Command
        // Por simplicidad, filtraremos en memoria para búsquedas rápidas, 
        // o invocamos a la API. Optaremos por filtro en memoria sobre la lista completa 
        // si la API no está paginada, o hacemos un delay y llamamos a la API.
        
        AplicarFiltroLocal();
    }

    [ObservableProperty] public partial string FiltroEstadoTexto { get; set; } = "Todos";
    private bool? _filtroActivo = null;

    private void AplicarFiltroLocal()
    {
        IEnumerable<ClienteModel> resultado = _todosClientes;

        if (_filtroActivo.HasValue)
        {
            resultado = resultado.Where(x => x.Activo == _filtroActivo.Value);
        }

        if (!string.IsNullOrWhiteSpace(Busqueda))
        {
            var texto = Busqueda.Trim().ToLowerInvariant();
            resultado = resultado.Where(x => 
                x.NombreComercial.ToLowerInvariant().Contains(texto) ||
                x.NumeroIdentificacion.ToLowerInvariant().Contains(texto) ||
                x.Email.ToLowerInvariant().Contains(texto));
        }

        Clientes.Clear();
        foreach (var c in resultado)
        {
            Clientes.Add(c);
        }
    }

    [RelayCommand]
    private async Task CambiarFiltroEstadoAsync()
    {
        var opcion = await Shell.Current.DisplayActionSheetAsync(
            "Filtrar por estado",
            "Cancelar",
            null,
            "Todos",
            "Activos",
            "Inactivos");

        if (string.IsNullOrEmpty(opcion) || opcion == "Cancelar") return;

        if (opcion == "Todos")
        {
            _filtroActivo = null;
            FiltroEstadoTexto = "Todos";
        }
        else if (opcion == "Activos")
        {
            _filtroActivo = true;
            FiltroEstadoTexto = "Activos";
        }
        else if (opcion == "Inactivos")
        {
            _filtroActivo = false;
            FiltroEstadoTexto = "Inactivos";
        }

        AplicarFiltroLocal();
    }

    [RelayCommand]
    private async Task DescargarAsync()
    {
        var encabezados = new[] { "Identificación", "Nombre comercial", "Correo", "Teléfono", "Estado" };
        var filas = Clientes.Select(c => new[]
        {
            c.NumeroIdentificacion,
            c.NombreComercial,
            c.Email,
            c.Telefono,
            c.EstadoTexto
        }).ToList();

        await ReportService.SeleccionarYExportarAsync(
            "Reporte de Clientes", encabezados, filas, "Clientes");
    }

    private void ActualizarListadoYResumen()
    {
        TotalClientes = _todosClientes.Count;
        TotalActivos = _todosClientes.Count(x => x.Activo);
        TotalInactivos = _todosClientes.Count(x => !x.Activo);

        AplicarFiltroLocal();
    }

    [RelayCommand]
    private async Task NuevoAsync()
    {
        await Shell.Current.GoToAsync(nameof(ClienteFormPage));
    }

    [RelayCommand]
    private async Task AbrirDetalleAsync(ClienteModel cliente)
    {
        if (cliente is null) return;

        var parametros = new Dictionary<string, object>
        {
            ["IdCliente"] = cliente.Id
        };

        await Shell.Current.GoToAsync(nameof(ClienteDetallePage), parametros);
    }
}
