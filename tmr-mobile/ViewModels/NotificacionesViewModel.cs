using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using tmr_mobile.Services;

namespace tmr_mobile.ViewModels;

public partial class NotificacionesViewModel : BaseViewModel
{
    private readonly ApiService _apiService;

    [ObservableProperty]
    public partial ObservableCollection<NotificacionItemViewModel> Notificaciones { get; set; } = new();

    public NotificacionesViewModel(ApiService apiService)
    {
        _apiService = apiService;
        Title = "Notificaciones";
        _ = CargarNotificacionesAsync();
    }

    [RelayCommand]
    private async Task CargarNotificacionesAsync()
    {
        try
        {
            IsBusy = true;
            var response = await _apiService.GetAsync<List<NotificacionItemViewModel>>("api/notificaciones");
            if (response != null)
            {
                Notificaciones.Clear();
                foreach (var item in response)
                {
                    Notificaciones.Add(item);
                }
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}

public class NotificacionItemViewModel
{
    public string Titulo { get; set; } = string.Empty;
    public string MensajePart1 { get; set; } = string.Empty;
    public string MensajeHighlight { get; set; } = string.Empty;
    public string MensajePart2 { get; set; } = string.Empty;
    public bool IsAlert { get; set; }
}
