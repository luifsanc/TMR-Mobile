using System.Net.Mail;
using System.Windows.Input;
using tmr_mobile.Services;

namespace tmr_mobile.ViewModels;

public class ForgotPasswordViewModel : BaseViewModel
{
    private readonly IAuthService _authService;

    private string _email = string.Empty;
    private string _emailError = string.Empty;
    private bool _isSuccess;
    private string _successMessage = string.Empty;

    public string Email
    {
        get => _email;
        set
        {
            if (SetProperty(ref _email, value))
            {
                EmailError = string.Empty;
            }
        }
    }

    public string EmailError
    {
        get => _emailError;
        set
        {
            if (SetProperty(ref _emailError, value))
            {
                OnPropertyChanged(nameof(HasEmailError));
            }
        }
    }

    public bool HasEmailError =>
        !string.IsNullOrWhiteSpace(EmailError);

    public bool IsSuccess
    {
        get => _isSuccess;
        set
        {
            if (SetProperty(ref _isSuccess, value))
            {
                OnPropertyChanged(nameof(IsFormVisible));
            }
        }
    }

    public bool IsFormVisible => !IsSuccess;

    public string SuccessMessage
    {
        get => _successMessage;
        set => SetProperty(ref _successMessage, value);
    }

    public ICommand SendLinkCommand { get; }

    public ICommand GoBackCommand { get; }

    public ForgotPasswordViewModel(IAuthService authService)
    {
        _authService = authService;

        Title = "Recuperar contraseña";

        SendLinkCommand = new Command(
            async () => await SendLinkAsync()
        );

        GoBackCommand = new Command(
            async () => await GoBackAsync()
        );
    }

    private async Task SendLinkAsync()
    {
        if (IsBusy)
            return;

        EmailError = string.Empty;
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Email))
        {
            EmailError = "Email es requerido";
            return;
        }

        if (!IsValidEmail(Email))
        {
            EmailError = "Email no válido";
            return;
        }

        try
        {
            IsBusy = true;

            var result = await _authService.ForgotPasswordAsync(
                Email.Trim()
            );

            if (!result.Success)
            {
                ErrorMessage =
                    string.IsNullOrWhiteSpace(result.Message)
                        ? "No se pudo enviar el enlace de recuperación."
                        : result.Message;

                return;
            }

            SuccessMessage =
                string.IsNullOrWhiteSpace(result.Message)
                    ? "Revisa tu correo electrónico para continuar con la recuperación."
                    : result.Message;

            IsSuccess = true;

            await Task.Delay(3000);

            await Shell.Current.GoToAsync("//LoginPage");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"ForgotPassword Error: {ex.Message}"
            );

            ErrorMessage =
                "Ocurrió un error al procesar la solicitud.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("//LoginPage");
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var mailAddress = new MailAddress(email.Trim());

            return mailAddress.Address == email.Trim();
        }
        catch
        {
            return false;
        }
    }
}