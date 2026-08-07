using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using tmr_mobile.Services;
using tmr_shared.DTOs.Clientes;

namespace tmr_mobile.ViewModels;

public partial class ClientesViewModel : BaseViewModel
{
    private readonly ApiService _apiService;

    public ObservableCollection<ClienteListaResponse> Clientes { get; } = new();

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
            // GET /api/clientes  (requiere JWT)
            var lista = await _apiService.GetAsync<List<ClienteListaResponse>>("api/clientes");
            Clientes.Clear();
            if (lista != null)
                foreach (var c in lista)
                    Clientes.Add(c);
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

    [RelayCommand]
    private async Task BuscarAsync(string busqueda)
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var url = string.IsNullOrWhiteSpace(busqueda)
                ? "api/clientes"
                : $"api/clientes?busqueda={Uri.EscapeDataString(busqueda)}";

            var lista = await _apiService.GetAsync<List<ClienteListaResponse>>(url);
            Clientes.Clear();
            if (lista != null)
                foreach (var c in lista)
                    Clientes.Add(c);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al buscar clientes: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
