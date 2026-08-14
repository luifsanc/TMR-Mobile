using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using tmr_mobile.Models.Configuracion;
using tmr_mobile.Services;
using tmr_mobile.Views.Configuracion;

namespace tmr_mobile.ViewModels;

public partial class RolDetalleViewModel : BaseViewModel, IQueryAttributable
{
    private readonly IRolesService _rolesService;
    private int _idRol;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EsActivo))]
    [NotifyPropertyChangedFor(nameof(EsInactivo))]
    [NotifyPropertyChangedFor(nameof(TextoBotonEstado))]
    public partial RolListaItem? Rol { get; set; }

    public bool EsActivo => Rol?.Activo == true;
    public bool EsInactivo => Rol != null && !Rol.Activo;
    public string TextoBotonEstado => EsActivo ? "Desactivar" : "Activar";
    public ObservableCollection<RolModuloItem> ModulosHabilitados { get; } = new();

    public RolDetalleViewModel(IRolesService rolesService)
    {
        _rolesService = rolesService;
        Title = "Detalle del rol";
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("IdRol", out var idObj))
        {
            if (idObj is int id) _idRol = id;
            else if (idObj is string idText && int.TryParse(idText, out var parsedId)) _idRol = parsedId;
            else return;

            _ = CargarDetalleAsync();
        }
    }

    [RelayCommand]
    private async Task CargarDetalleAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            Rol = await _rolesService.ObtenerRolAsync(_idRol);
            ModulosHabilitados.Clear();

            if (Rol is not null)
            {
                if (Rol.Modulos is not null && Rol.Modulos.Count > 0)
                {
                    foreach (var mod in Rol.Modulos)
                    {
                        ModulosHabilitados.Add(mod);
                    }
                }
                else if (Rol.Modulosids is not null && Rol.Modulosids.Count > 0)
                {
                    var modulosTodos = await _rolesService.ObtenerModulosDisponiblesAsync();
                    foreach (var mod in modulosTodos.Where(m => Rol.Modulosids.Contains(m.Id)))
                    {
                        ModulosHabilitados.Add(mod);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "Ocurrió un error al cargar el detalle del rol.";
            System.Diagnostics.Debug.WriteLine($"[ROL-DETALLE][ERROR] Mensaje: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task EditarAsync()
    {
        if (Rol is null) return;

        var parameters = new Dictionary<string, object>
        {
            ["IdRol"] = Rol.Id,
            ["Rol"] = Rol
        };

        await Shell.Current.GoToAsync(nameof(RolFormPage), parameters);
    }

    [RelayCommand]
    private async Task CambiarEstadoAsync()
    {
        if (Rol is null || IsBusy) return;

        var confirm = await Shell.Current.DisplayAlertAsync(
            "Confirmación",
            $"¿Deseas {(EsActivo ? "desactivar" : "activar")} el rol {Rol.Nombre}?",
            "Sí",
            "No");

        if (!confirm) return;

        try
        {
            IsBusy = true;
            var success = await _rolesService.CambiarEstadoAsync(Rol.Id, !Rol.Activo);

            if (success)
            {
                await CargarDetalleAsync();
                await Shell.Current.DisplayAlertAsync(
                    "Estado actualizado",
                    $"El rol quedó {(EsActivo ? "activo" : "inactivo")}.",
                    "Aceptar");
            }
            else
            {
                ErrorMessage = "No se pudo cambiar el estado del rol.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "No se pudo cambiar el estado.";
            System.Diagnostics.Debug.WriteLine($"[ROL-DETALLE][ERROR] Mensaje: {ex.Message}");
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
