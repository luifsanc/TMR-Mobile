using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using tmr_mobile.Services;
using System.Collections.Generic;
using tmr_mobile.Resources.Styles;

namespace tmr_mobile.ViewModels;

public class ConfiguracionModuloItem
{
    public string Nombre { get; set; } = string.Empty;
    public string Icono { get; set; } = string.Empty;
    public string IconoOscuro => ThemeImages.DarkVariant(Icono);
    public string Ruta { get; set; } = string.Empty;
}

public partial class ConfiguracionViewModel : ObservableObject
{
    private readonly IUserModuleAccessService _moduleAccessService;
    private HashSet<string> _modulosPermitidos = new(StringComparer.OrdinalIgnoreCase);

    public ObservableCollection<ConfiguracionModuloItem> Modulos { get; } = new();

    public ConfiguracionViewModel(IUserModuleAccessService moduleAccessService)
    {
        _moduleAccessService = moduleAccessService;
        _ = CargarModulosAsync();
    }

    private async Task CargarModulosAsync()
    {
        try
        {
            _modulosPermitidos = await _moduleAccessService.ObtenerModulosAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"No se pudieron cargar los módulos de configuración: {ex.Message}");
            return;
        }

        Modulos.Clear();
        AgregarSiPermitido("Usuarios", "Usuarios", "user_profile.png", "UsuariosPage");
        AgregarSiPermitido("Roles", "Roles", "rol.png", "RolesPage");
        AgregarSiPermitido("Dias Festivos", "Feriados", "feriado.png", "FeriadosPage");
        AgregarSiPermitido("Configuracion", "Catálogos", "catalogo.png", "CatalogosPage");
    }

    private void AgregarSiPermitido(string permiso, string nombre, string icono, string ruta)
    {
        if (_modulosPermitidos.Contains(permiso))
        {
            Modulos.Add(new ConfiguracionModuloItem { Nombre = nombre, Icono = icono, Ruta = ruta });
        }
    }

    [RelayCommand]
    private async Task NavigateAsync(string route)
    {
        var permitido = route switch
        {
            "UsuariosPage" => _modulosPermitidos.Contains("Usuarios"),
            "RolesPage" => _modulosPermitidos.Contains("Roles"),
            "FeriadosPage" => _modulosPermitidos.Contains("Dias Festivos"),
            "CatalogosPage" => _modulosPermitidos.Contains("Configuracion"),
            _ => false
        };

        if (!string.IsNullOrEmpty(route) && permitido)
        {
            await Shell.Current.GoToAsync($"//{route}");
        }
    }
}
