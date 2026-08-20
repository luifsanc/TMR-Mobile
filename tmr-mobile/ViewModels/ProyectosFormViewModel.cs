using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using tmr_mobile.Services;
using tmr_shared.DTOs.Proyectos;

namespace tmr_mobile.ViewModels;

public class ProyectoLiderFormItem
{
    public ObservableCollection<LookupItem> LideresDisponibles { get; } = new();
    public LookupItem? LiderSeleccionado { get; set; }
    public ObservableCollection<ProyectoRecursoFormItem> RecursosAsignados { get; } = new();
    public ICommand AgregarRecursoCommand { get; }
    public ICommand EliminarRecursoCommand { get; }

    public ProyectoLiderFormItem(IEnumerable<LookupItem> recursos)
    {
        foreach (var recurso in recursos)
            RecursosDisponibles.Add(recurso);

        AgregarRecursoCommand = new Command(AgregarRecurso);
        EliminarRecursoCommand = new Command<ProyectoRecursoFormItem>(EliminarRecurso);
        AgregarRecurso();
    }

    private ObservableCollection<LookupItem> RecursosDisponibles { get; } = new();

    private void AgregarRecurso()
    {
        RecursosAsignados.Add(new ProyectoRecursoFormItem(RecursosDisponibles, EliminarRecurso));
    }

    public void EliminarRecurso(ProyectoRecursoFormItem recurso)
    {
        if (RecursosAsignados.Count > 1)
            RecursosAsignados.Remove(recurso);
    }
}

public class ProyectoRecursoFormItem
{
    public ObservableCollection<LookupItem> Opciones { get; } = new();
    public LookupItem? Seleccionado { get; set; }

    public ICommand EliminarCommand { get; }

    public ProyectoRecursoFormItem(IEnumerable<LookupItem> opciones, Action<ProyectoRecursoFormItem> eliminar)
    {
        foreach (var opcion in opciones)
            Opciones.Add(opcion);

        EliminarCommand = new Command(() => eliminar(this));
    }

    public int Id { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string Departamento { get; init; } = string.Empty;
}

public partial class ProyectosFormViewModel : BaseViewModel, IQueryAttributable
{
    private readonly ApiService _apiService;
    private ProyectoResponse? _proyectoOriginal;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EsEdicion))]
    [NotifyPropertyChangedFor(nameof(EsNuevo))]
    public partial int? IdProyecto { get; set; }

    public bool EsEdicion => IdProyecto.HasValue && IdProyecto.Value > 0;
    public bool EsNuevo => !EsEdicion;

    [ObservableProperty]
    public partial string TituloPagina { get; set; } = "Nuevo Proyecto";

    [ObservableProperty]
    public partial string Nombre { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Cliente { get; set; } = string.Empty;

    [ObservableProperty]
    public partial LookupItem? ClienteSeleccionado { get; set; }

    [ObservableProperty]
    public partial string Estado { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string LiderAsignado { get; set; } = string.Empty;

    [ObservableProperty]
    public partial int Recursos { get; set; }

    [ObservableProperty]
    public partial DateTime? FechaInicio { get; set; }

    [ObservableProperty]
    public partial DateTime? FechaFin { get; set; }

    // Catálogos
    public ObservableCollection<string> Estados { get; } = new()
    {
        "Planificación",
        "En progreso",
        "En riesgo",
        "Pendiente",
        "Pausado",
        "Completado",
        "Cancelado",
        "Activo",
        "Inactivo"
    };

    public ObservableCollection<string> Lideres { get; } = new();

    public ObservableCollection<LookupItem> LideresDisponibles { get; } = new();
    public ObservableCollection<LookupItem> ClientesDisponibles { get; } = new();
    public ObservableCollection<LookupItem> RecursosDisponibles { get; } = new();
    public ObservableCollection<ProyectoLiderFormItem> LideresAsignados { get; } = new();

    public ProyectosFormViewModel(ApiService apiService)
    {
        _apiService = apiService;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("IdProyecto", out var idObj) && idObj is int id)
        {
            IdProyecto = id;
            TituloPagina = "Editar Proyecto";
        }

        _ = CargarDatosInicialesAsync();
    }

    [RelayCommand]
    private async Task CargarDatosInicialesAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            var lookups = await _apiService.GetAsync<ProyectoLookupsResponse>("proyectos/lookups");
            if (lookups != null)
            {
                LideresDisponibles.Clear();
                ClientesDisponibles.Clear();
                RecursosDisponibles.Clear();

                foreach (var lider in lookups.Lideres)
                    LideresDisponibles.Add(lider);

                foreach (var cliente in lookups.Clientes)
                    ClientesDisponibles.Add(cliente);

                foreach (var recurso in lookups.Empleados)
                    RecursosDisponibles.Add(recurso);
            }

            // Si es edición, cargar datos del proyecto
            if (EsEdicion && IdProyecto.HasValue)
            {
                var proyecto = await _apiService.GetAsync<ProyectoResponse>($"proyectos/{IdProyecto.Value}");
                if (proyecto != null)
                {
                    _proyectoOriginal = proyecto;
                    Nombre = proyecto.Nombre;
                    Cliente = proyecto.Cliente;
                    ClienteSeleccionado = lookups?.Clientes.FirstOrDefault(c => c.Nombre.Equals(proyecto.Cliente, StringComparison.OrdinalIgnoreCase));
                    Estado = proyecto.Estado;
                    LiderAsignado = proyecto.LiderAsignado;
                    Recursos = proyecto.NumeroRecursos;
                    FechaInicio = proyecto.FechaInicio?.ToDateTime(TimeOnly.MinValue);
                    FechaFin = proyecto.FechaFin?.ToDateTime(TimeOnly.MinValue);

                    LideresAsignados.Clear();
                    foreach (var lider in proyecto.Lideres)
                    {
                        var item = new ProyectoLiderFormItem(RecursosDisponibles)
                        {
                            LiderSeleccionado = LideresDisponibles.FirstOrDefault(l => l.Id == lider.IdLider)
                        };

                        CopiarRecursosDisponibles(item);

                        foreach (var recurso in lider.Recursos)
                        {
                            var lookup = RecursosDisponibles.FirstOrDefault(r => r.Id == recurso.IdEmpleado);
                            if (lookup != null)
                            {
                                var recursoItem = item.RecursosAsignados.LastOrDefault();
                                if (recursoItem != null)
                                    recursoItem.Seleccionado = lookup;
                            }

                            if (recurso != lider.Recursos.Last())
                                item.RecursosAsignados.Add(new ProyectoRecursoFormItem(RecursosDisponibles, item.EliminarRecurso));
                        }

                        LideresAsignados.Add(item);
                    }

                    if (LideresAsignados.Count == 0)
                        AgregarLider();
                }
                else
                {
                    ErrorMessage = "No se encontró el proyecto.";
                }
            }
            else if (LideresAsignados.Count == 0)
            {
                AgregarLider();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "Error cargando proyecto.";
            System.Diagnostics.Debug.WriteLine(ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task GuardarAsync()
    {
        if (!ValidarFormulario()) return;

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            var request = new
            {
                Codigo = _proyectoOriginal?.Codigo,
                Nombre = Nombre.Trim(),
                Descripcion = _proyectoOriginal?.Descripcion,
                IdCliente = ClienteSeleccionado?.Id ?? _proyectoOriginal?.IdCliente,
                Cliente = ClienteSeleccionado?.Nombre ?? Cliente.Trim(),
                IdTipoProyecto = _proyectoOriginal?.IdTipoProyecto,
                Tipo = _proyectoOriginal?.Tipo,
                Observacion = _proyectoOriginal?.Observacion,
                FechaInicioReal = _proyectoOriginal?.FechaInicioReal,
                FechaFinReal = _proyectoOriginal?.FechaFinReal,
                FechaInicioEspera = _proyectoOriginal?.FechaInicioEspera,
                FechaFinEspera = _proyectoOriginal?.FechaFinEspera,
                IdLider = LideresAsignados.First(l => l.LiderSeleccionado != null).LiderSeleccionado!.Id,
                Estado = Estado,
                Lider = LideresAsignados.First(l => l.LiderSeleccionado != null).LiderSeleccionado!.Nombre,
                IdEstadoProyecto = _proyectoOriginal?.IdEstadoProyecto,
                FechaInicio = FechaInicio.HasValue ? DateOnly.FromDateTime(FechaInicio.Value) : (DateOnly?)null,
                FechaFin = FechaFin.HasValue ? DateOnly.FromDateTime(FechaFin.Value) : (DateOnly?)null,
                Presupuesto = _proyectoOriginal?.Presupuesto,
                Horas = _proyectoOriginal?.Horas,
                LiderCosto = _proyectoOriginal?.CostoHoraLider,
                LiderHoras = _proyectoOriginal?.HorasLider,
                Lideres = LideresAsignados
                    .Where(l => l.LiderSeleccionado != null)
                    .Select(l => new
                {
                    IdLider = l.LiderSeleccionado!.Id,
                    Lider = l.LiderSeleccionado.Nombre,
                    LiderCosto = (decimal?)null,
                    LiderHoras = (decimal?)null,
                    Recursos = l.RecursosAsignados.Where(r => r.Seleccionado != null).Select(r => new
                    {
                        IdEmpleado = (int?)r.Seleccionado!.Id,
                        Tipo = string.Empty,
                        Nombre = r.Seleccionado.Nombre,
                        Rol = string.Empty,
                        Entrada = (DateOnly?)null,
                        Salida = (DateOnly?)null,
                        CostoHora = (decimal?)null,
                        Horas = (decimal?)null
                    }).ToList()
                }).ToList()
            };

            if (!EsEdicion)
            {
                var created = await _apiService.PostAsync<object, ProyectoResponse>("proyectos", request);
                if (created != null)
                {
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    ErrorMessage = "Error al crear el proyecto.";
                }
            }
            else
            {
                var updated = await _apiService.PutAsync<object, ProyectoResponse>($"proyectos/{IdProyecto}", request);
                if (updated != null)
                {
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    ErrorMessage = "Error al actualizar el proyecto.";
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al guardar: {ex.Message}";
            System.Diagnostics.Debug.WriteLine(ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool ValidarFormulario()
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Nombre))
        {
            ErrorMessage = "El nombre es requerido.";
            return false;
        }

        if (Nombre.Length > 100)
        {
            ErrorMessage = "El nombre no puede superar los 100 caracteres.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(Cliente))
        {
            ErrorMessage = "El cliente es requerido.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(Estado))
        {
            ErrorMessage = "El estado es requerido.";
            return false;
        }

        if (FechaInicio == null)
        {
            ErrorMessage = "La fecha de inicio es requerida.";
            return false;
        }

        if (FechaFin == null)
        {
            ErrorMessage = "La fecha de fin es requerida.";
            return false;
        }

        if (FechaFin < FechaInicio)
        {
            ErrorMessage = "La fecha de fin no puede ser anterior a la fecha de inicio.";
            return false;
        }

        if (LideresAsignados.Count == 0 || LideresAsignados.Any(l => l.LiderSeleccionado == null))
        {
            ErrorMessage = "Debe asignar al menos un líder.";
            return false;
        }

        return true;
    }

    [RelayCommand]
    private void AgregarLider()
    {
        var item = new ProyectoLiderFormItem(RecursosDisponibles);
        CopiarRecursosDisponibles(item);
        LideresAsignados.Add(item);
    }

    [RelayCommand]
    private void EliminarLider(ProyectoLiderFormItem item)
    {
        if (item != null && LideresAsignados.Count > 1)
            LideresAsignados.Remove(item);
    }

    private void CopiarRecursosDisponibles(ProyectoLiderFormItem item)
    {
        item.LideresDisponibles.Clear();
        foreach (var lider in LideresDisponibles)
            item.LideresDisponibles.Add(lider);

        // Las opciones se copian al crear cada fila de recurso.
    }

    [RelayCommand]
    private async Task CancelarAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task RegresarAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
