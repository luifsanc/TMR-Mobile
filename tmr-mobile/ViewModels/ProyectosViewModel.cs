using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace tmr_mobile.ViewModels;

public class ProyectoItem
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Cliente { get; set; } = string.Empty;
    public string Estado { get; set; } = "Activo";
}

public partial class ProyectosViewModel : BaseViewModel
{
    public ObservableCollection<ProyectoItem> Proyectos { get; } = new();

    public ProyectosViewModel()
    {
        Title = "Gestión de Proyectos";
    }

    [RelayCommand]
    private async Task CargarProyectosAsync()
    {
        IsBusy = true;
        try
        {
            Proyectos.Clear();
            Proyectos.Add(new ProyectoItem { Id = 1, Nombre = "Transformación Digital TMR", Cliente = "Banco Central", Estado = "Activo" });
            Proyectos.Add(new ProyectoItem { Id = 2, Nombre = "Migración .NET 10", Cliente = "Seguros Alfa", Estado = "En Proceso" });
        }
        finally
        {
            IsBusy = false;
        }
    }
}
