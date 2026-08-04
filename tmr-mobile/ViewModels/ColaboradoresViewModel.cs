using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace tmr_mobile.ViewModels;

public class ColaboradorItem
{
    public int Id { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public partial class ColaboradoresViewModel : BaseViewModel
{
    public ObservableCollection<ColaboradorItem> Colaboradores { get; } = new();

    public ColaboradoresViewModel()
    {
        Title = "Gestión de Colaboradores";
    }

    [RelayCommand]
    private async Task CargarColaboradoresAsync()
    {
        IsBusy = true;
        try
        {
            Colaboradores.Clear();
            Colaboradores.Add(new ColaboradorItem { Id = 1, NombreCompleto = "Carlos Mendoza", Cargo = "Senior .NET Developer", Email = "carlos@tmr.com" });
            Colaboradores.Add(new ColaboradorItem { Id = 2, NombreCompleto = "Ana Torres", Cargo = "Mobile Specialist (MAUI/Angular)", Email = "ana@tmr.com" });
        }
        finally
        {
            IsBusy = false;
        }
    }
}
