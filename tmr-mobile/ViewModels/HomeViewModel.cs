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
            new ModuloItem { Nombre = "Proyectos", Icono = "icon_proyectos.png", Ruta = "ProyectosPage" },
            new ModuloItem { Nombre = "Time Report", Icono = "icon_timereport.png", Ruta = "TimeReportPage" },
            new ModuloItem { Nombre = "Carga Actividades", Icono = "icon_carga.png", Ruta = "CargaActividadesPage" },
            new ModuloItem { Nombre = "Reportes", Icono = "icon_reportes.png", Ruta = "ReportesPage" },
            new ModuloItem { Nombre = "Líderes", Icono = "icon_lideres.png", Ruta = "LideresPage" },
            new ModuloItem { Nombre = "Colaboradores", Icono = "icon_colaboradores.png", Ruta = "ColaboradoresPage" },
            new ModuloItem { Nombre = "Clientes", Icono = "icon_clientes.png", Ruta = "ClientesPage" }
        };
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
