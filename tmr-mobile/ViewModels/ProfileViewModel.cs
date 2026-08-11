using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using tmr_mobile.Services;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace tmr_mobile.ViewModels;

public partial class ProfileViewModel : BaseViewModel
{
    private readonly IAuthService _authService;

    public ProfileViewModel(IAuthService authService)
    {
        _authService = authService;
        Title = "Perfil";
    }

    public string UserName => _authService.CurrentUser?.Name ?? "Teofilo";
    
    public string Email => _authService.CurrentUser?.Email ?? "admin@tmr.com";

    [RelayCommand]
    private async Task ChangePasswordAsync()
    {
        // Placeholder temporal para no romper funcionalidad actual
        if (Shell.Current != null)
        {
            await Shell.Current.DisplayAlert("Info", "Opción Cambiar Contraseña (próximamente)", "OK");
        }
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        // Placeholder temporal para no romper funcionalidad actual
        if (Shell.Current != null)
        {
            await Shell.Current.DisplayAlert("Info", "Opción Cerrar Sesión (próximamente)", "OK");
        }
    }
}
