using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using tmr_mobile.Services;
using tmr_shared.DTOs.Proyectos;

namespace tmr_mobile.ViewModels;

public partial class ProyectosFormViewModel : BaseViewModel, IQueryAttributable
{
    private readonly ApiService _apiService;
    private ProyectoResponse? _proyectoOriginal;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EsEdicion))]
    public partial int? IdProyecto { get; set; }

    public bool EsEdicion => IdProyecto.HasValue && IdProyecto.Value > 0;

    [ObservableProperty]
    public partial string TituloPagina { get; set; } = "Nuevo Proyecto";

    [ObservableProperty]
    public partial string Nombre { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Cliente { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Estado { get; set; } = "Activo";

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
        "Activo",
        "Inactivo"
    };

    public ObservableCollection<string> Lideres { get; } = new();

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

            // Si es edición, cargar datos del proyecto
            if (EsEdicion && IdProyecto.HasValue)
            {
                var proyecto = await _apiService.GetAsync<ProyectoResponse>($"proyectos/{IdProyecto.Value}");
                if (proyecto != null)
                {
                    _proyectoOriginal = proyecto;
                    Nombre = proyecto.Nombre;
                    Cliente = proyecto.Cliente;
                    Estado = proyecto.Estado;
                    LiderAsignado = proyecto.LiderAsignado;
                    Recursos = proyecto.NumeroRecursos;
                    FechaInicio = proyecto.FechaInicio?.ToDateTime(TimeOnly.MinValue);
                    FechaFin = proyecto.FechaFin?.ToDateTime(TimeOnly.MinValue);
                }
                else
                {
                    ErrorMessage = "No se encontró el proyecto.";
                }
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
                IdCliente = _proyectoOriginal?.IdCliente,
                Cliente = Cliente.Trim(),
                IdTipoProyecto = _proyectoOriginal?.IdTipoProyecto,
                Tipo = _proyectoOriginal?.Tipo,
                Observacion = _proyectoOriginal?.Observacion,
                FechaInicioReal = _proyectoOriginal?.FechaInicioReal,
                FechaFinReal = _proyectoOriginal?.FechaFinReal,
                FechaInicioEspera = _proyectoOriginal?.FechaInicioEspera,
                FechaFinEspera = _proyectoOriginal?.FechaFinEspera,
                IdLider = _proyectoOriginal?.IdLider,
                Estado = Estado,
                Lider = LiderAsignado.Trim(),
                IdEstadoProyecto = _proyectoOriginal?.IdEstadoProyecto,
                FechaInicio = FechaInicio.HasValue ? DateOnly.FromDateTime(FechaInicio.Value) : (DateOnly?)null,
                FechaFin = FechaFin.HasValue ? DateOnly.FromDateTime(FechaFin.Value) : (DateOnly?)null,
                Presupuesto = _proyectoOriginal?.Presupuesto,
                Horas = _proyectoOriginal?.Horas,
                LiderCosto = _proyectoOriginal?.CostoHoraLider,
                LiderHoras = _proyectoOriginal?.HorasLider,
                Lideres = _proyectoOriginal?.Lideres.Select(l => new
                {
                    IdLider = string.Equals(l.Lider, LiderAsignado, StringComparison.OrdinalIgnoreCase)
                        ? l.IdLider
                        : null,
                    Lider = l == _proyectoOriginal.Lideres.First() ? LiderAsignado.Trim() : l.Lider,
                    LiderCosto = l.CostoHoraLider,
                    LiderHoras = l.HorasLider,
                    Recursos = l.Recursos.Select(r => new
                    {
                        r.IdEmpleado,
                        r.Tipo,
                        r.Nombre,
                        r.Rol,
                        r.Entrada,
                        r.Salida,
                        r.CostoHora,
                        r.Horas
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

        if (string.IsNullOrWhiteSpace(LiderAsignado))
        {
            ErrorMessage = "El líder asignado es requerido.";
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

        return true;
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
