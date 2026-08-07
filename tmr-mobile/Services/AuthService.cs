using tmr_shared.DTOs.Auth;

namespace tmr_mobile.Services;

public sealed class ApiLoginResponse
{
    public bool Success { get; set; }

    public int StatusCode { get; set; }

    public string Message { get; set; } = string.Empty;

    public AuthResponse? Data { get; set; }
}

public sealed class ApiForgotPasswordResponse
{
    public bool Success { get; set; }

    public int StatusCode { get; set; }

    public string Message { get; set; } = string.Empty;

    public ForgotPasswordData? Data { get; set; }
}

public sealed class ForgotPasswordData
{
    public string? Message { get; set; }

    public string? ExpirationTime { get; set; }
}

public sealed class ForgotPasswordResult
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;
}

public interface IAuthService
{
    Task<bool> LoginAsync(
        string username,
        string password
    );
    
    Task<ForgotPasswordResult> ForgotPasswordAsync(
        string email
    );

    Task LogoutAsync();

    Task<bool> IsAuthenticatedAsync();

    Task<string?> GetTokenAsync();

    UserResponse? CurrentUser { get; }
}

public class AuthService : IAuthService
{
    private const string TokenKey = "auth_token";

    private readonly ApiService _apiService;

    public UserResponse? CurrentUser { get; private set; }

    public AuthService(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<bool> LoginAsync(
        string username,
        string password)
    {
        try
        {
            var payload = new
            {
                user = username,
                password
            };

            var response =
                await _apiService.PostAsync<
                    object,
                    ApiLoginResponse
                >(
                    "auth/login",
                    payload
                );

            if (response?.Data?.AccessToken is not null)
            {
                await SecureStorage.Default.SetAsync(
                    TokenKey,
                    response.Data.AccessToken
                );

                CurrentUser = new UserResponse(
                    response.Data.User.Id,
                    response.Data.User.Email,
                    response.Data.User.Name,
                    response.Data.User.CreatedAt,
                    response.Data.User.IdEmpleado > 0
                        ? response.Data.User.IdEmpleado
                        : null
                );

                return true;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"Login Error: {ex.Message}"
            );
        }

        return false;
    }

    public async Task<ForgotPasswordResult> ForgotPasswordAsync(
        string email)
    {
        try
        {
            var payload = new
            {
                email
            };

            var response =
                await _apiService.PostAsync<
                    object,
                    ApiForgotPasswordResponse
                >(
                    "auth/forgot-password",
                    payload
                );

            if (response is null)
            {
                return new ForgotPasswordResult
                {
                    Success = false,
                    Message =
                        "No se pudo enviar el enlace de recuperación."
                };
            }

            return new ForgotPasswordResult
            {
                Success = response.Success,

                Message =
                    response.Data?.Message
                    ?? response.Message
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"Forgot Password Error: {ex.Message}"
            );

            return new ForgotPasswordResult
            {
                Success = false,
                Message =
                    "Error de conexión con el servidor."
            };
        }
    }

    public async Task LogoutAsync()
    {
        SecureStorage.Default.Remove(TokenKey);

        CurrentUser = null;

        await Shell.Current.GoToAsync("//LoginPage");
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token =
            await SecureStorage.Default.GetAsync(TokenKey);

        return !string.IsNullOrEmpty(token);
    }

    public async Task<string?> GetTokenAsync()
    {
        return await SecureStorage.Default.GetAsync(TokenKey);
    }
}