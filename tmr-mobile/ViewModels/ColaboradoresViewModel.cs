using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using tmr_mobile.Services;
using tmr_shared.DTOs.Colaboradores;

namespace tmr_mobile.ViewModels;

public partial class ColaboradoresViewModel : BaseViewModel
{
    private readonly ApiService _apiService;

    public ObservableCollection<ColaboradorListaResponse> Colaboradores { get; } = new();

    public ColaboradoresViewModel(ApiService apiService)
    {
        _apiService = apiService;
        Title = "Gestión de Colaboradores";
    }

    [RelayCommand]
    private async Task CargarColaboradoresAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            // GET /api/colaboradores  (requiere JWT)
            var lista = await _apiService.GetAsync<List<ColaboradorListaResponse>>("api/colaboradores");
            Colaboradores.Clear();
            if (lista != null)
                foreach (var c in lista)
                    Colaboradores.Add(c);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al cargar colaboradores: {ex.Message}";
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
                ? "api/colaboradores"
                : $"api/colaboradores?busqueda={Uri.EscapeDataString(busqueda)}";

            var lista = await _apiService.GetAsync<List<ColaboradorListaResponse>>(url);
            Colaboradores.Clear();
            if (lista != null)
                foreach (var c in lista)
                    Colaboradores.Add(c);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al buscar: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
