using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using tmr_mobile.Models.Operaciones;
using tmr_mobile.Services;
using tmr_mobile.Views.Operaciones;

namespace tmr_mobile.ViewModels;

public partial class ColaboradoresViewModel : BaseViewModel
{
    private readonly IColaboradoresService _colaboradoresService;
    private readonly List<ColaboradorModel> _todosColaboradores = new();

    public ObservableCollection<ColaboradorModel> Colaboradores { get; } = new();

    [ObservableProperty]
    public partial string Busqueda { get; set; } = string.Empty;

    // Resumen
    [ObservableProperty] public partial int TotalColaboradores { get; set; }
    [ObservableProperty] public partial int TotalActivos { get; set; }
    [ObservableProperty] public partial int TotalInactivos { get; set; }
    [ObservableProperty] public partial int TotalAsignados { get; set; }
    [ObservableProperty] public partial int TotalNoAsignados { get; set; }

    // Filtro de Estado Seleccionado: null (Todos), true (Activos), false (Inactivos)
    [ObservableProperty] public partial string FiltroEstadoTexto { get; set; } = "Todos";
    private bool? _filtroActivo = null;

    public ColaboradoresViewModel(IColaboradoresService colaboradoresService)
    {
        _colaboradoresService = colaboradoresService;
        Title = "Colaboradores";
    }

    public async Task InicializarAsync()
    {
        await CargarColaboradoresAsync();
    }

    [RelayCommand]
    private async Task CargarColaboradoresAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            var lista = await _colaboradoresService.ObtenerColaboradoresAsync();
            
            _todosColaboradores.Clear();
            _todosColaboradores.AddRange(lista);

            ActualizarListadoYResumen();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Error al cargar colaboradores.";
            System.Diagnostics.Debug.WriteLine(ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnBusquedaChanged(string value)
    {
        AplicarFiltroLocal();
    }

    private void AplicarFiltroLocal()
    {
        IEnumerable<ColaboradorModel> resultado = _todosColaboradores;

        if (_filtroActivo.HasValue)
        {
            resultado = resultado.Where(x => x.Activo == _filtroActivo.Value);
        }

        if (!string.IsNullOrWhiteSpace(Busqueda))
        {
            var texto = Busqueda.Trim().ToLowerInvariant();
            resultado = resultado.Where(x => 
                x.NombreCompleto.ToLowerInvariant().Contains(texto) ||
                x.NumeroIdentificacion.ToLowerInvariant().Contains(texto) ||
                x.Email.ToLowerInvariant().Contains(texto) ||
                x.Cargo.ToLowerInvariant().Contains(texto));
        }

        Colaboradores.Clear();
        foreach (var c in resultado)
        {
            Colaboradores.Add(c);
        }
    }

    private void ActualizarListadoYResumen()
    {
        TotalColaboradores = _todosColaboradores.Count;
        TotalActivos = _todosColaboradores.Count(x => x.Activo);
        TotalInactivos = _todosColaboradores.Count(x => !x.Activo);
        TotalAsignados = _todosColaboradores.Count(x => x.NumProyectos > 0);
        TotalNoAsignados = _todosColaboradores.Count(x => x.NumProyectos == 0);

        AplicarFiltroLocal();
    }

    [RelayCommand]
    private async Task CambiarFiltroEstadoAsync()
    {
        // Alternar entre Todos -> Activos -> Inactivos -> Todos
        if (_filtroActivo == null)
        {
            _filtroActivo = true;
            FiltroEstadoTexto = "Activos";
        }
        else if (_filtroActivo == true)
        {
            _filtroActivo = false;
            FiltroEstadoTexto = "Inactivos";
        }
        else
        {
            _filtroActivo = null;
            FiltroEstadoTexto = "Todos";
        }

        AplicarFiltroLocal();
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task DescargarAsync()
    {
        var encabezados = new[] { "Código", "Identificación", "Nombre", "Correo", "Cargo", "Proyectos", "Estado" };
        var filas = Colaboradores.Select(c => new[]
        {
            c.CodigoEmpleado,
            c.NumeroIdentificacion,
            c.NombreCompleto,
            c.Email,
            c.Cargo,
            c.NumProyectos.ToString(),
            c.EstadoTexto
        }).ToList();

        await ReportService.SeleccionarYExportarAsync(
            "Reporte de Colaboradores", encabezados, filas, "Colaboradores");
    }

    [RelayCommand]
    private async Task NuevoAsync()
    {
        await Shell.Current.GoToAsync(nameof(ColaboradorFormPage));
    }

    [RelayCommand]
    private async Task AbrirDetalleAsync(ColaboradorModel colaborador)
    {
        if (colaborador is null) return;

        var parametros = new Dictionary<string, object>
        {
            ["IdColaborador"] = colaborador.Id
        };

        await Shell.Current.GoToAsync(nameof(ColaboradorDetallePage), parametros);
    }
}
