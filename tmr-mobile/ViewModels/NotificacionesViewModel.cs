using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using tmr_mobile.Services;
using tmr_shared.DTOs.Dashboard;

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
    }

    [RelayCommand]
    private async Task CargarNotificacionesAsync()
    {
        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;
            Notificaciones.Clear();

            var response = await _apiService.GetAsync<HorasIncompletasResponse>(
                "dashboard/mis-horas-incompletas?rango=mes");

            if (response?.TieneFaltantes != true)
            {
                return;
            }

            if (response.DiasIncompletos is { Count: > 0 })
            {
                foreach (var dia in response.DiasIncompletos.OrderByDescending(item => item.Fecha))
                {
                    Notificaciones.Add(new NotificacionItemViewModel
                    {
                        Titulo = "Registro de horas pendiente",
                        MensajePart1 = $"El {dia.Fecha:dd/MM/yyyy} registraste ",
                        MensajeHighlight = $"{dia.HorasRegistradas:0.#} h",
                        MensajePart2 = $" y te faltan {dia.HorasFaltantes:0.#} h para completar la jornada.",
                        IsAlert = true
                    });
                }

                return;
            }

            Notificaciones.Add(new NotificacionItemViewModel
            {
                Titulo = "Registro de horas pendiente",
                MensajePart1 = "Tienes ",
                MensajeHighlight = $"{response.HorasFaltantes:0.#} h pendientes",
                MensajePart2 = " de registrar durante este mes.",
                IsAlert = true
            });
        }
        catch (Exception ex)
        {
            ErrorMessage = "No se pudieron cargar las notificaciones.";
            System.Diagnostics.Debug.WriteLine($"Error al cargar notificaciones: {ex.Message}");
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
