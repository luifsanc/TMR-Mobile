using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using tmr_mobile.Services;

namespace tmr_mobile.ViewModels;

public class ClienteItem
{
    public int Id { get; set; }
    public string NombreComercial { get; set; } = string.Empty;
    public string NumeroIdentificacion { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public bool Activo { get; set; }
}

public sealed class ClienteApiResponse
{
    public int Id { get; set; }
    public string NombreComercial { get; set; } = string.Empty;
    public string NumeroIdentificacion { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public bool Activo { get; set; }
}

public partial class ClientesViewModel : BaseViewModel
{
    private readonly ApiService _apiService;

    public ObservableCollection<ClienteItem> Clientes { get; } = new();

    public ClientesViewModel(ApiService apiService)
    {
        _apiService = apiService;
        Title = "Gestión de Clientes";
    }

    [RelayCommand]
    private async Task CargarClientesAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var clientes = await _apiService.GetAsync<List<ClienteApiResponse>>("clientes");
            Clientes.Clear();

            foreach (var cliente in clientes ?? new List<ClienteApiResponse>())
            {
                Clientes.Add(new ClienteItem
                {
                    Id = cliente.Id,
                    NombreComercial = cliente.NombreComercial,
                    NumeroIdentificacion = cliente.NumeroIdentificacion,
                    Email = cliente.Email,
                    Telefono = cliente.Telefono,
                    Activo = cliente.Activo
                });
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al cargar clientes: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
