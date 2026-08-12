using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using tmr_mobile.Services;

namespace tmr_mobile.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    public partial string User { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Password { get; set; } = string.Empty;

    public LoginViewModel(IAuthService authService)
    {
        _authService = authService;
        Title = "Iniciar Sesión";
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(User) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Por favor ingrese usuario y contraseña.";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;

        var success = await _authService.LoginAsync(User, Password);

        IsBusy = false;

        if (success)
        {
            await Shell.Current.GoToAsync("//home");
        }
        else
        {
            ErrorMessage = "Credenciales inválidas o error de conexión.";
        }
    }
}
