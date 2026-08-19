using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using tmr_mobile.Models.Configuracion;
using tmr_mobile.Services;
using tmr_mobile.Views.Configuracion;

namespace tmr_mobile.ViewModels;

public partial class RolesConfigViewModel : BaseViewModel
{
    private readonly IRolesService _rolesService;

    public ObservableCollection<RolListaItem> Roles { get; } = new();

    [ObservableProperty]
    public partial string Busqueda { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string FiltroEstado { get; set; } = "Todos";

    [ObservableProperty]
    public partial int TotalRoles { get; set; }

    [ObservableProperty]
    public partial int TotalActivos { get; set; }

    [ObservableProperty]
    public partial int TotalInactivos { get; set; }

    public RolesConfigViewModel(IRolesService rolesService)
    {
        _rolesService = rolesService;
        Title = "Configuración - Roles y Permisos";
    }

    public async Task InicializarAsync()
    {
        await CargarRolesAsync();
    }

    partial void OnBusquedaChanged(string value)
    {
        _ = CargarRolesAsync();
    }

    partial void OnFiltroEstadoChanged(string value)
    {
        _ = CargarRolesAsync();
    }

    [RelayCommand]
    private async Task CargarRolesAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            bool? activo = FiltroEstado switch
            {
                "Activos" => true,
                "Inactivos" => false,
                _ => null,
            };

            var resultados = await _rolesService.ObtenerRolesAsync(Busqueda, activo);

            Roles.Clear();
            foreach (var rol in resultados)
            {
                Roles.Add(rol);
            }

            TotalRoles = Roles.Count;
            TotalActivos = Roles.Count(x => x.Activo);
            TotalInactivos = Roles.Count(x => !x.Activo);
        }
        catch (Exception ex)
        {
            ErrorMessage = "No se pudieron cargar los roles.";
            System.Diagnostics.Debug.WriteLine($"[ROLES][ERROR] Mensaje: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task NuevoAsync()
    {
        await Shell.Current.GoToAsync(nameof(RolFormPage));
    }

    [RelayCommand]
    private async Task AbrirDetalleAsync(RolListaItem rol)
    {
        if (rol is null) return;

        var parametros = new Dictionary<string, object>
        {
            ["IdRol"] = rol.Id
        };

        await Shell.Current.GoToAsync(nameof(RolDetallePage), parametros);
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

        FiltroEstado = opcion;
        await CargarRolesAsync();
    }
}
