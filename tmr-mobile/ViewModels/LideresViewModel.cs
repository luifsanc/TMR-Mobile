using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using tmr_mobile.Services;
using tmr_shared.DTOs.Lideres;

namespace tmr_mobile.ViewModels;

public partial class LideresViewModel : BaseViewModel
{
    private readonly ApiService _apiService;

    public ObservableCollection<LiderResponse> Lideres { get; } = new();

    public LideresViewModel(ApiService apiService)
    {
        _apiService = apiService;
        Title = "Líderes de Proyecto";
    }

    [RelayCommand]
    private async Task CargarLideresAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            // GET /api/lideres
            var lista = await _apiService.GetAsync<List<LiderResponse>>("api/lideres");
            Lideres.Clear();
            if (lista != null)
                foreach (var l in lista)
                    Lideres.Add(l);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al cargar líderes: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
