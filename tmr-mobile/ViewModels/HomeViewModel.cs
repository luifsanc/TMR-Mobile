using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace tmr_mobile.ViewModels;

public class ModuloItem
{
    public string Nombre { get; set; } = string.Empty;
    public string Icono { get; set; } = string.Empty;
    public string Ruta { get; set; } = string.Empty;
}

public partial class HomeViewModel : ObservableObject
{
    public ObservableCollection<ModuloItem> Modulos { get; }

    public HomeViewModel()
    {
        Modulos = new ObservableCollection<ModuloItem>
        {
            new ModuloItem { Nombre = "Proyectos", Icono = "icon_proyectos.png", Ruta = "proyectos" },
            new ModuloItem { Nombre = "Time Report", Icono = "icon_timereport.png", Ruta = "timereport" },
            new ModuloItem { Nombre = "Carga Actividades", Icono = "icon_carga.png", Ruta = "carga" },
            new ModuloItem { Nombre = "Reportes", Icono = "icon_reportes.png", Ruta = "reportes" },
            new ModuloItem { Nombre = "Líderes", Icono = "icon_lideres.png", Ruta = "lideres" },
            new ModuloItem { Nombre = "Colaboradores", Icono = "icon_colaboradores.png", Ruta = "colaboradores" },
            new ModuloItem { Nombre = "Clientes", Icono = "icon_clientes.png", Ruta = "clientes" }
        };
    }

    [RelayCommand]
    private async Task NavigateAsync(string route)
    {
        if (!string.IsNullOrEmpty(route))
        {
            // The original logic in HomePage was `await Shell.Current.GoToAsync($"//{route}");` for bottom nav items (like "dashboard" or "home")
            // But for subpages (like "proyectos"), typically `await Shell.Current.GoToAsync(route)` is used.
            // Let's try root navigation first if it is one of the bottom tabs, else regular navigation.
            if (route == "dashboard" || route == "home")
            {
                await Shell.Current.GoToAsync($"//{route}");
            }
            else
            {
                try 
                {
                    await Shell.Current.GoToAsync(route);
                }
                catch
                {
                    // Fallback just in case
                    await Shell.Current.GoToAsync($"//{route}");
                }
            }
        }
    }
}
