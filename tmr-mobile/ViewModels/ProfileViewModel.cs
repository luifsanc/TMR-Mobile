using CommunityToolkit.Mvvm.Input;
using tmr_mobile.Services;
using tmr_mobile.Views.Auth;

namespace tmr_mobile.ViewModels;

public partial class ProfileViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly IConfirmDialogService _confirmDialogService;

    public ProfileViewModel(
        IAuthService authService,
        IConfirmDialogService confirmDialogService)
    {
        _authService = authService;
        _confirmDialogService = confirmDialogService;
        Title = "Perfil";
    }

    public string UserName => _authService.CurrentUser?.Name ?? "Usuario TMR";

    public string Email => _authService.CurrentUser?.Email ?? "Sesión activa";

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
