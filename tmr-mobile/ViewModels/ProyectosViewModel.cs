using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using tmr_mobile.Services;
using tmr_shared.DTOs.Proyectos;

namespace tmr_mobile.ViewModels;

public partial class ProyectosViewModel : BaseViewModel
{
    private readonly ApiService _apiService;

    public ObservableCollection<ProyectoResponse> Proyectos { get; } = new();

    public ProyectosViewModel(ApiService apiService)
    {
        _apiService = apiService;
        Title = "Gestión de Proyectos";
    }

    [RelayCommand]
    private async Task CargarProyectosAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            // GET /api/proyectos
            var lista = await _apiService.GetAsync<List<ProyectoResponse>>("api/proyectos");
            Proyectos.Clear();
            if (lista != null)
                foreach (var p in lista)
                    Proyectos.Add(p);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al cargar proyectos: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
