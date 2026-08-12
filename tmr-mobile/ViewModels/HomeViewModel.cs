using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using tmr_mobile.Services;
using System.Collections.Generic;

namespace tmr_mobile.ViewModels;

public class ModuloItem
{
    public string Nombre { get; set; } = string.Empty;
    public string Icono { get; set; } = string.Empty;
    public string Ruta { get; set; } = string.Empty;
}

public partial class HomeViewModel : ObservableObject
{
    private readonly ApiService _apiService;

    public ObservableCollection<ModuloItem> Modulos { get; } = new();

    public HomeViewModel(ApiService apiService)
    {
        _apiService = apiService;
        _ = CargarModulosAsync();
    }

    private async Task CargarModulosAsync()
    {
        try 
        {
            var response = await _apiService.GetAsync<List<ModuloItem>>("api/home/modulos");
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
        Modulos.Add(new ModuloItem { Nombre = "Proyectos", Icono = "icon_proyectos.png", Ruta = "ProyectosPage" });
        Modulos.Add(new ModuloItem { Nombre = "Time Report", Icono = "icon_timereport.png", Ruta = "TimeReportPage" });
        Modulos.Add(new ModuloItem { Nombre = "Carga Actividades", Icono = "icon_carga.png", Ruta = "CargaActividadesPage" });
        Modulos.Add(new ModuloItem { Nombre = "Reportes", Icono = "icon_reportes.png", Ruta = "ReportesPage" });
        Modulos.Add(new ModuloItem { Nombre = "Líderes", Icono = "icon_lideres.png", Ruta = "LideresPage" });
        Modulos.Add(new ModuloItem { Nombre = "Colaboradores", Icono = "icon_colaboradores.png", Ruta = "ColaboradoresPage" });
        Modulos.Add(new ModuloItem { Nombre = "Clientes", Icono = "icon_clientes.png", Ruta = "ClientesPage" });
    }

    [RelayCommand]
    private async Task NavigateAsync(string route)
    {
        if (!string.IsNullOrEmpty(route))
        {
            // Todos los módulos principales ahora son ShellItems raíz
            await Shell.Current.GoToAsync($"//{route}");
        }
    }
}
