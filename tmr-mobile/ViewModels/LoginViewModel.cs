using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using tmr_mobile.Services;

namespace tmr_mobile.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly IUserModuleAccessService _moduleAccessService;

    [ObservableProperty]
    public partial string User { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Password { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsPasswordHidden { get; set; } = true;

    [ObservableProperty]
    public partial string PasswordToggleIcon { get; set; } = "password_eye.png";

    public LoginViewModel(
        IAuthService authService,
        IUserModuleAccessService moduleAccessService)
    {
        _authService = authService;
        _moduleAccessService = moduleAccessService;

        Title = "Iniciar Sesión";
    }

    [RelayCommand]
    private void TogglePasswordVisibility()
    {
        IsPasswordHidden = !IsPasswordHidden;
        PasswordToggleIcon = IsPasswordHidden
            ? "password_eye.png"
            : "password_eye_off.png";
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(User) ||
            string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage =
                "Por favor ingrese usuario y contraseña.";

            return;
        }

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            var success =
                await _authService.LoginAsync(
                    User,
                    Password
                );

            if (success)
            {
                if (_authService.CurrentUser?.DebeCambiarPassword == true)
                {
                    await Shell.Current.GoToAsync(
                        nameof(Views.Auth.ChangePasswordPage),
                        new Dictionary<string, object> { ["required"] = true });
                    return;
                }

                if (await _moduleAccessService.EsColaboradorAsync())
                {
                    await Shell.Current.GoToAsync("//ColaboradorDashboardPage");
                    return;
                }

                var modulos = await _moduleAccessService.ObtenerModulosAsync();
                var rutaInicial = modulos.Contains("Dashboard")
                    ? "DashboardPage"
                    : modulos.Contains("Actividades") || modulos.Contains("Time Report")
                        ? "TimeReportPage"
                        : "HomePage";

                await Shell.Current.GoToAsync($"//{rutaInicial}");

                return;
            }

            ErrorMessage =
                "Credenciales inválidas o error de conexión.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
