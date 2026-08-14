using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using tmr_mobile.Models.Configuracion;
using tmr_mobile.Services;

namespace tmr_mobile.ViewModels;

public partial class RolFormViewModel : BaseViewModel, IQueryAttributable
{
    private readonly IRolesService _rolesService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EsEdicion))]
    public partial int? IdRol { get; set; }

    [ObservableProperty]
    public partial RolListaItem? RolTarget { get; set; }

    [ObservableProperty]
    public partial string Nombre { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Descripcion { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool Activo { get; set; } = true;

    [ObservableProperty]
    public partial string TituloPagina { get; set; } = "Nuevo rol";

    public bool EsEdicion => IdRol.HasValue;
    public string TextoBoton => EsEdicion ? "Guardar cambios" : "Guardar";

    public ObservableCollection<RolModuloItem> Modulos { get; } = new();

    public RolFormViewModel(IRolesService rolesService)
    {
        _rolesService = rolesService;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Rol", out var rObj) && rObj is RolListaItem r)
        {
            RolTarget = r;
            IdRol = r.Id;
            Nombre = r.Nombre;
            Descripcion = r.Descripcion;
            Activo = r.Activo;
            TituloPagina = "Editar rol";
        }
        else if (query.TryGetValue("IdRol", out var idObj))
        {
            if (idObj is int id)
                IdRol = id;
            else if (idObj is string idText && int.TryParse(idText, out var parsedId))
                IdRol = parsedId;
            else
                IdRol = null;

            TituloPagina = IdRol.HasValue ? "Editar rol" : "Crear Nuevo Rol";
        }
        else
        {
            IdRol = null;
            TituloPagina = "Crear Nuevo Rol";
        }

        _ = CargarDatosInicialesAsync();
    }

    private async Task CargarDatosInicialesAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            var modulosDisponibles = await _rolesService.ObtenerModulosDisponiblesAsync();
            Modulos.Clear();

            List<int> modulosAsignadosIds = new();

            if (EsEdicion && IdRol.HasValue)
            {
                if (RolTarget is null)
                {
                    RolTarget = await _rolesService.ObtenerRolAsync(IdRol.Value);
                }

                if (RolTarget is not null)
                {
                    Nombre = RolTarget.Nombre;
                    Descripcion = RolTarget.Descripcion;
                    Activo = RolTarget.Activo;

                    if (RolTarget.Modulosids is not null && RolTarget.Modulosids.Count > 0)
                    {
                        modulosAsignadosIds = RolTarget.Modulosids;
                    }
                    else if (RolTarget.Modulos is not null && RolTarget.Modulos.Count > 0)
                    {
                        modulosAsignadosIds = RolTarget.Modulos.Select(m => m.Id).ToList();
                    }
                }
            }

            foreach (var mod in modulosDisponibles)
            {
                bool estaSeleccionado = modulosAsignadosIds.Contains(mod.Id) ||
                    (RolTarget?.Modulos != null && RolTarget.Modulos.Any(m => m.Id == mod.Id || string.Equals(m.Nombre, mod.Nombre, StringComparison.OrdinalIgnoreCase)));

                mod.Selected = estaSeleccionado;
                Modulos.Add(mod);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "Ocurrió un error al cargar el formulario del rol.";
            System.Diagnostics.Debug.WriteLine($"[ROL-FORM][ERROR] Mensaje: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void ToggleModulo(RolModuloItem modulo)
    {
        if (modulo is null) return;
        modulo.Selected = !modulo.Selected;

        var index = Modulos.IndexOf(modulo);
        if (index >= 0)
        {
            Modulos[index] = new RolModuloItem
            {
                Id = modulo.Id,
                Nombre = modulo.Nombre,
                Descripcion = modulo.Descripcion,
                Activo = modulo.Activo,
                Selected = modulo.Selected
            };
        }
    }

    [RelayCommand]
    private async Task GuardarAsync()
    {
        if (string.IsNullOrWhiteSpace(Nombre))
        {
            ErrorMessage = "El nombre del rol es requerido.";
            return;
        }

        if (string.IsNullOrWhiteSpace(Descripcion))
        {
            ErrorMessage = "La descripción del rol es requerida.";
            return;
        }

        var seleccionadosIds = Modulos.Where(m => m.Selected).Select(m => m.Id).ToList();
        if (seleccionadosIds.Count == 0)
        {
            ErrorMessage = "Debes seleccionar al menos un módulo accesible.";
            return;
        }

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            if (EsEdicion && IdRol.HasValue)
            {
                var updateRequest = new UpdateRolRequest
                {
                    Nombre = Nombre.Trim(),
                    Descripcion = Descripcion.Trim(),
                    Modulosids = seleccionadosIds
                };

                var success = await _rolesService.ActualizarRolAsync(IdRol.Value, updateRequest);
                if (success)
                {
                    await Shell.Current.DisplayAlertAsync(
                        "Rol actualizado",
                        "El rol ha sido actualizado correctamente.",
                        "Aceptar");
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    ErrorMessage = "No se pudo actualizar el rol.";
                }
            }
            else
            {
                var createRequest = new CreateRolRequest
                {
                    Nombre = Nombre.Trim(),
                    Descripcion = Descripcion.Trim(),
                    Modulosids = seleccionadosIds
                };

                var nuevoRol = await _rolesService.CrearRolAsync(createRequest);
                if (nuevoRol is not null)
                {
                    await Shell.Current.DisplayAlertAsync(
                        "Rol creado",
                        "El rol ha sido creado correctamente.",
                        "Aceptar");
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    ErrorMessage = "No se pudo crear el rol.";
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "Ocurrió un error al guardar el rol.";
            System.Diagnostics.Debug.WriteLine($"[ROL-FORM][ERROR] Mensaje: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RegresarAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
