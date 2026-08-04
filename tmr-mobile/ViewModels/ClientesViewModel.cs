using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace tmr_mobile.ViewModels;

public class ClienteItem
{
    public int Id { get; set; }
    public string RazónSocial { get; set; } = string.Empty;
    public string RUC { get; set; } = string.Empty;
    public string Contacto { get; set; } = string.Empty;
}

public partial class ClientesViewModel : BaseViewModel
{
    public ObservableCollection<ClienteItem> Clientes { get; } = new();

    public ClientesViewModel()
    {
        Title = "Gestión de Clientes";
    }

    [RelayCommand]
    private async Task CargarClientesAsync()
    {
        IsBusy = true;
        try
        {
            Clientes.Clear();
            Clientes.Add(new ClienteItem { Id = 1, RazónSocial = "Corporación Tecnológica S.A.", RUC = "20100012345", Contacto = "admin@corp.com" });
        }
        finally
        {
            IsBusy = false;
        }
    }
}
