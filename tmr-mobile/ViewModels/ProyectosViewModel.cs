using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using tmr_mobile.Models.Operaciones;
using tmr_mobile.Services;
using tmr_shared.DTOs.Proyectos;

namespace tmr_mobile.ViewModels;

public class ProyectoFormModel
{
    public int? Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Cliente { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string Lider { get; set; } = string.Empty;
    public string Estado { get; set; } = "Activo";
    public DateOnly? FechaInicio { get; set; }
    public DateOnly? FechaFin { get; set; }
    public decimal? Presupuesto { get; set; }
    public decimal? Horas { get; set; }
    public string Observacion { get; set; } = string.Empty;
    public DateOnly? FechaInicioReal { get; set; }
    public DateOnly? FechaFinReal { get; set; }
    public DateOnly? FechaInicioEspera { get; set; }
    public DateOnly? FechaFinEspera { get; set; }
    public int? IdEstadoProyecto { get; set; }

    public record LookupDto(int Id, string Nombre);
    public record CargoLookupDto(int Id, string Nombre, int? IdDepartamento);
}

public partial class ProyectosViewModel : ObservableObject
{
    private readonly ApiService _apiService;

    // NOTA: Title, IsBusy y ErrorMessage no estaban declaradas en el archivo original.
    // Si ya tienes una BaseViewModel con estas propiedades, elimina este bloque
    // y haz que la clase herede de ella en vez de ObservableObject.
    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public ObservableCollection<ProyectoItem> Proyectos { get; } = new();
    public ObservableCollection<ProyectoItem> ProyectosFiltrados { get; } = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<string> _estadoOpciones = new();

    [ObservableProperty]
    private ObservableCollection<string> _estadoFormOpciones = new();

    [ObservableProperty]
    private ObservableCollection<string> _tipoOpciones = new();

    [ObservableProperty]
    private ObservableCollection<string> _tipoFormOpciones = new();

    [ObservableProperty]
    private ObservableCollection<string> _tipoFiltroOpciones = new();

    [ObservableProperty]
    private ObservableCollection<ProyectoFormModel.LookupDto> _seguimientoOpciones = new();

    [ObservableProperty]
    private ProyectoFormModel.LookupDto? _selectedSeguimiento;

    [ObservableProperty]
    private string? _selectedEstado;

    [ObservableProperty]
    private string? _selectedTipo;

    [ObservableProperty]
    private int _proyectosCount;

    [ObservableProperty]
    private int _filtradosCount;

    [ObservableProperty]
    private bool _isFormVisible;

    [ObservableProperty]
    private string _formTitle = string.Empty;

    [ObservableProperty]
    private ProyectoFormModel _proyectoForm = new();

    public ProyectosViewModel(ApiService apiService)
    {
        _apiService = apiService;
        Title = "Gestión de Proyectos";
        EstadoOpciones = new ObservableCollection<string> { "Todos", "Activo", "Inactivo" };
        EstadoFormOpciones = new ObservableCollection<string> { "Activo", "Inactivo" };
        TipoOpciones = new ObservableCollection<string> { "Todos" };
        TipoFormOpciones = new ObservableCollection<string>();
        SelectedEstado = "Todos";
        SelectedTipo = "Todos";
    }

    partial void OnSearchTextChanged(string value) => AplicarFiltros();
    partial void OnSelectedEstadoChanged(string? value) => AplicarFiltros();
    partial void OnSelectedTipoChanged(string? value) => AplicarFiltros();
    partial void OnSelectedSeguimientoChanged(ProyectoFormModel.LookupDto? value)
    {
        ProyectoForm.IdEstadoProyecto = value?.Id;
    }

    private record ProyectosLookupsResponse(List<ProyectoFormModel.LookupDto> Clientes, List<ProyectoFormModel.LookupDto> Lideres, List<ProyectoFormModel.LookupDto> Empleados, List<ProyectoFormModel.LookupDto> Estados, List<ProyectoFormModel.LookupDto> Tipos, List<ProyectoFormModel.LookupDto> Departamentos, List<ProyectoFormModel.CargoLookupDto> Cargos);

    [RelayCommand]
    private async Task CargarProyectosAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            await CargarProyectosLookupsAsync();
            var proyectos = await _apiService.GetAsync<List<ProyectoResponse>>("proyectos");
            Proyectos.Clear();
            ProyectosFiltrados.Clear();

            foreach (var proyecto in proyectos ?? new List<ProyectoResponse>())
            {
                var item = new ProyectoItem(
                    proyecto.Id,
                    proyecto.Codigo,
                    proyecto.Nombre,
                    proyecto.Cliente,
                    proyecto.Estado ?? "Activo",
                    proyecto.Presupuesto ?? 0m,
                    proyecto.Horas ?? 0m,
                    proyecto.Lider,
                    proyecto.NumeroRecursos,
                    proyecto.Tipo,
                    proyecto.FechaInicio?.ToString("dd/MM/yyyy") ?? string.Empty,
                    proyecto.FechaFin?.ToString("dd/MM/yyyy") ?? string.Empty
                );
                Proyectos.Add(item);
            }

            ProyectosCount = Proyectos.Count;
            AplicarFiltros();

            if (Proyectos.Count == 0)
            {
                ErrorMessage = "No se encontraron proyectos para el filtro actual.";
                IsFormVisible = false;
            }
            else
            {
                ErrorMessage = string.Empty;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al cargar proyectos: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void AplicarFiltros()
    {
        ProyectosFiltrados.Clear();
        ProyectosCount = Proyectos.Count;

        var query = Proyectos.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var q = SearchText.Trim().ToLower();
            query = query.Where(p =>
                (p.Codigo ?? string.Empty).ToLower().Contains(q) ||
                (p.Nombre ?? string.Empty).ToLower().Contains(q) ||
                (p.Cliente ?? string.Empty).ToLower().Contains(q)
            );
        }

        if (!string.IsNullOrWhiteSpace(SelectedEstado) && SelectedEstado != "Todos")
        {
            query = query.Where(p => string.Equals(p.Estado, SelectedEstado, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(SelectedTipo) && SelectedTipo != "Todos")
        {
            query = query.Where(p => string.Equals(p.Tipo, SelectedTipo, StringComparison.OrdinalIgnoreCase));
        }

        foreach (var p in query)
            ProyectosFiltrados.Add(p);

        FiltradosCount = ProyectosFiltrados.Count;
    }

    [RelayCommand]
    private void LimpiarFiltros()
    {
        SearchText = string.Empty;
        SelectedEstado = "Todos";
        SelectedTipo = "Todos";
        AplicarFiltros();
    }

    [RelayCommand]
    private void CrearProyecto()
    {
        ProyectoForm = new ProyectoFormModel();
        FormTitle = "Nuevo Proyecto";
        IsFormVisible = true;
    }

    [RelayCommand]
    private void CancelarFormulario()
    {
        IsFormVisible = false;
    }

    [RelayCommand]
    private async Task GuardarProyectoAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            // Se asume endpoint POST /api/proyectos que devuelve el proyecto creado
            var crearReq = new
            {
                Codigo = ProyectoForm.Codigo,
                Nombre = ProyectoForm.Nombre,
                Cliente = ProyectoForm.Cliente,
                Tipo = ProyectoForm.Tipo,
                IdLider = (int?)null,
                Lider = ProyectoForm.Lider,
                IdEstadoProyecto = ProyectoForm.IdEstadoProyecto,
                Estado = ProyectoForm.Estado,
                FechaInicio = ProyectoForm.FechaInicio,
                FechaFin = ProyectoForm.FechaFin,
                Presupuesto = ProyectoForm.Presupuesto,
                Horas = ProyectoForm.Horas,
                Observacion = ProyectoForm.Observacion,
                FechaInicioReal = ProyectoForm.FechaInicioReal,
                FechaFinReal = ProyectoForm.FechaFinReal,
                FechaInicioEspera = ProyectoForm.FechaInicioEspera,
                FechaFinEspera = ProyectoForm.FechaFinEspera
            };

            if (ProyectoForm.Id.HasValue && ProyectoForm.Id.Value > 0)
            {
                var putOk = await _apiService.PutAsync($"proyectos/{ProyectoForm.Id.Value}", crearReq);
                if (putOk)
                {
                    await CargarProyectosAsync();
                    IsFormVisible = false;
                }
                else
                {
                    ErrorMessage = "No se pudo actualizar el proyecto.";
                }
            }
            else
            {
                await _apiService.PostAsync<object, ProyectoResponse>("proyectos", crearReq);
                await CargarProyectosAsync();
                IsFormVisible = false;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al guardar proyecto: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task CargarProyectosLookupsAsync()
    {
        try
        {
            var lookups = await _apiService.GetAsync<ProyectosLookupsResponse>("proyectos/lookups");
            if (lookups?.Tipos is not null)
            {
                TipoOpciones.Clear();
                TipoOpciones.Add("Todos");
                foreach (var tipo in lookups.Tipos)
                {
                    if (!string.IsNullOrWhiteSpace(tipo.Nombre) && !TipoOpciones.Contains(tipo.Nombre))
                    {
                        TipoOpciones.Add(tipo.Nombre);
                    }
                }
            }

            if (lookups?.Estados is not null)
            {
                EstadoOpciones.Clear();
                EstadoOpciones.Add("Todos");
                foreach (var estado in lookups.Estados)
                {
                    if (!string.IsNullOrWhiteSpace(estado.Nombre) && !EstadoOpciones.Contains(estado.Nombre))
                    {
                        EstadoOpciones.Add(estado.Nombre);
                    }
                }
            }

            if (lookups?.Estados is not null)
            {
                SeguimientoOpciones.Clear();
                foreach (var estado in lookups.Estados)
                {
                    if (!string.IsNullOrWhiteSpace(estado.Nombre) && estado.Nombre != "Activo")
                    {
                        SeguimientoOpciones.Add(estado);
                    }
                }
            }
        }
        catch
        {
            // Ignorar errores de lookups y mantener opciones por defecto.
        }
    }

    public void AbrirEditar(ProyectoItem item)
    {
        if (item == null) return;

        ProyectoForm = new ProyectoFormModel
        {
            Id = item.Id,
            Codigo = item.Codigo,
            Nombre = item.Nombre,
            Cliente = item.Cliente,
            Tipo = item.Tipo,
            Lider = item.Lider,
            Estado = item.Estado,
            FechaInicio = null,
            FechaFin = null,
            Presupuesto = item.Presupuesto,
            Horas = item.Horas,
            Observacion = string.Empty,
            FechaInicioReal = null,
            FechaFinReal = null,
            FechaInicioEspera = null,
            FechaFinEspera = null,
            IdEstadoProyecto = null
        };

        FormTitle = "Editar Proyecto";
        IsFormVisible = true;
    }

    public async Task InactivarProyectoAsync(ProyectoItem item)
    {
        if (item == null) return;
        try
        {
            IsBusy = true;
            var ok = await _apiService.DeleteAsync($"proyectos/{item.Id}");
            if (ok)
            {
                await CargarProyectosAsync();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al cambiar estado del proyecto: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}