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
    private readonly ApiService _apiService;

    public ObservableCollection<ConfiguracionModuloItem> Modulos { get; } = new();

    public ConfiguracionViewModel(ApiService apiService)
    {
        _apiService = apiService;
        _ = CargarModulosAsync();
    }

    private async Task CargarModulosAsync()
    {
        try
        {
            var response = await _apiService.GetAsync<List<ConfiguracionModuloItem>>("api/configuracion/modulos");
            if (response != null && response.Count > 0)
            {
                Modulos.Clear();
                foreach (var item in response) Modulos.Add(item);
                return;
            }
        }
        catch { /* Ignorar error de red y usar fallback */ }

        // Fallback local si la API falla o no está encendida
        Modulos.Clear();
        Modulos.Add(new ConfiguracionModuloItem { Nombre = "Usuarios", Icono = "user_profile.png", Ruta = "UsuariosPage" });
        Modulos.Add(new ConfiguracionModuloItem { Nombre = "Roles", Icono = "rol.png", Ruta = "RolesPage" });
        Modulos.Add(new ConfiguracionModuloItem { Nombre = "Feriados", Icono = "feriado.png", Ruta = "FeriadosPage" });
        Modulos.Add(new ConfiguracionModuloItem { Nombre = "Catálogos", Icono = "catalogo.png", Ruta = "CatalogosPage" });
    }

    [RelayCommand]
    private async Task NavigateAsync(string route)
    {
        if (!string.IsNullOrEmpty(route))
        {
            await Shell.Current.GoToAsync($"//{route}");
        }
    }
}
