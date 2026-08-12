using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using tmr_mobile.Services;

namespace tmr_mobile.ViewModels;

public partial class ChangePasswordViewModel : BaseViewModel
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    public partial string OldPassword { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string NewPassword { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ConfirmPassword { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsOldPasswordHidden { get; set; } = true;

    [ObservableProperty]
    public partial bool IsNewPasswordHidden { get; set; } = true;

    [ObservableProperty]
    public partial bool IsConfirmPasswordHidden { get; set; } = true;

    [ObservableProperty]
    public partial string SuccessMessage { get; set; } = string.Empty;

    public ChangePasswordViewModel(IAuthService authService)
    {
        _authService = authService;
        Title = "Cambiar contraseña";
    }

    [RelayCommand]
    private void ToggleOldPassword() => IsOldPasswordHidden = !IsOldPasswordHidden;

    [RelayCommand]
    private void ToggleNewPassword() => IsNewPasswordHidden = !IsNewPasswordHidden;

    [RelayCommand]
    private void ToggleConfirmPassword() =>
        IsConfirmPasswordHidden = !IsConfirmPasswordHidden;

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (IsBusy)
            return;

        ErrorMessage = Validate();
        SuccessMessage = string.Empty;

        if (!string.IsNullOrEmpty(ErrorMessage))
            return;

        try
        {
            IsBusy = true;
            var result = await _authService.ChangePasswordAsync(
                OldPassword,
                NewPassword,
                ConfirmPassword);

            if (!result.Success)
            {
                ErrorMessage = string.IsNullOrWhiteSpace(result.Message)
                    ? "No se pudo cambiar la contraseña. Verifica los datos ingresados."
                    : result.Message;
                return;
            }

            OldPassword = string.Empty;
            NewPassword = string.Empty;
            ConfirmPassword = string.Empty;
            SuccessMessage = result.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task GoBackAsync() => await Shell.Current.GoToAsync("..");

    private string Validate()
    {
        if (string.IsNullOrWhiteSpace(OldPassword) ||
            string.IsNullOrWhiteSpace(NewPassword) ||
            string.IsNullOrWhiteSpace(ConfirmPassword))
            return "Completa todos los campos para continuar.";

        if (NewPassword.Length < 6)
            return "La nueva contraseña debe tener al menos 6 caracteres.";

        if (!string.Equals(NewPassword, ConfirmPassword, StringComparison.Ordinal))
            return "La nueva contraseña y su confirmación no coinciden.";

        if (string.Equals(OldPassword, NewPassword, StringComparison.Ordinal))
            return "La nueva contraseña debe ser diferente de la actual.";

        return string.Empty;
    }
}
