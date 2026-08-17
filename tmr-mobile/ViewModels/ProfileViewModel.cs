using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using tmr_mobile.Services;
using tmr_mobile.Views.Auth;
using tmr_shared.DTOs.Dashboard;

namespace tmr_mobile.ViewModels;

public partial class ProfileViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly IConfirmDialogService _confirmDialogService;
    private readonly ApiService _apiService;

    public ProfileViewModel(
        IAuthService authService,
        IConfirmDialogService confirmDialogService,
        ApiService apiService)
    {
        _authService = authService;
        _confirmDialogService = confirmDialogService;
        _apiService = apiService;
        Title = "Perfil";
    }

    [ObservableProperty]
    public partial bool TieneNotificaciones { get; set; }

    public string UserName => _authService.CurrentUser?.Name ?? "Usuario TMR";

    public string Email => _authService.CurrentUser?.Email ?? "Sesión activa";

    [RelayCommand]
    private async Task CargarEstadoNotificacionesAsync()
    {
        try
        {
            var response = await _apiService.GetAsync<HorasIncompletasResponse>(
                "dashboard/mis-horas-incompletas?rango=mes");
            TieneNotificaciones = response?.TieneFaltantes == true;
        }
        catch
        {
            TieneNotificaciones = false;
        }
    }

    [RelayCommand]
    private async Task ChangePasswordAsync()
    {
        if (IsBusy)
            return;

        await Shell.Current.GoToAsync(nameof(ChangePasswordPage));
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        if (IsBusy)
            return;

        var confirmed = await _confirmDialogService.ShowAsync(
            "Cerrar sesión",
            "¿Deseas cerrar tu sesión en este dispositivo?",
            "Cerrar sesión",
            "Cancelar",
            "↪");

        if (!confirmed)
            return;

        try
        {
            IsBusy = true;
            await _authService.LogoutAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }
}
