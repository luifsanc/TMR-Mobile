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

public sealed class AuthOperationResult
{
    public bool Success { get; init; }

    public string Message { get; init; } = string.Empty;
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

    Task<AuthOperationResult> ChangePasswordAsync(
        string oldPassword,
        string newPassword,
        string confirmPassword
    );

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
        _apiService.SetRefreshTokenFunc(RefreshTokenAsync);
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
                    "api/auth/login",
                    payload
                );

            if (response?.Data?.AccessToken is not null)
            {
                await SecureStorage.Default.SetAsync(
                    TokenKey,
                    response.Data.AccessToken
                );

                await SecureStorage.Default.SetAsync(
                    ApiService.RefreshTokenKey,
                    response.Data.RefreshToken
                );

                await SecureStorage.Default.SetAsync(
                    ApiService.TokenFamilyKey,
                    response.Data.TokenFamilyId.ToString()
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
                    "api/auth/forgot-password",
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
        var refreshToken = await SecureStorage.Default.GetAsync(
            ApiService.RefreshTokenKey);

        try
        {
            var payload = new { refreshToken };
            var result = await _apiService.PostForResultAsync(
                "api/auth/logout",
                payload);

            if (!result.Success && !string.IsNullOrWhiteSpace(refreshToken))
            {
                await _apiService.PostForResultAsync(
                    "api/auth/logout-rt",
                    payload);
            }
        }
        catch (Exception ex)
        {
            // El cierre local siempre debe completarse, incluso sin conexión.
            System.Diagnostics.Debug.WriteLine($"Logout Error: {ex.Message}");
        }

        SecureStorage.Default.Remove(ApiService.TokenKey);
        SecureStorage.Default.Remove(ApiService.RefreshTokenKey);
        SecureStorage.Default.Remove(ApiService.TokenFamilyKey);

        CurrentUser = null;

        await Shell.Current.GoToAsync("//LoginPage");
    }

    public async Task<AuthOperationResult> ChangePasswordAsync(
        string oldPassword,
        string newPassword,
        string confirmPassword)
    {
        try
        {
            var result = await _apiService.PostForResultAsync(
                "api/auth/change-password",
                new
                {
                    oldPassword,
                    newPassword,
                    confirmPassword
                });

            return new AuthOperationResult
            {
                Success = result.Success,
                Message = result.Success
                    ? "Tu contraseña se actualizó correctamente."
                    : result.Message
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Change Password Error: {ex.Message}");
            return new AuthOperationResult
            {
                Success = false,
                Message = "No fue posible conectar con el servidor. Inténtalo nuevamente."
            };
        }
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

    private async Task<bool> RefreshTokenAsync()
    {
        try
        {
            var refreshToken = await SecureStorage.Default.GetAsync(
                ApiService.RefreshTokenKey);

            if (string.IsNullOrWhiteSpace(refreshToken))
                return false;

            var response = await _apiService.PostAnonymousAsync<object, ApiLoginResponse>(
                "api/auth/refresh-token",
                new { refreshToken });

            if (response?.Data?.AccessToken is null)
                return false;

            await SecureStorage.Default.SetAsync(
                ApiService.TokenKey,
                response.Data.AccessToken);
            await SecureStorage.Default.SetAsync(
                ApiService.RefreshTokenKey,
                response.Data.RefreshToken);
            await SecureStorage.Default.SetAsync(
                ApiService.TokenFamilyKey,
                response.Data.TokenFamilyId.ToString());

            CurrentUser = new UserResponse(
                response.Data.User.Id,
                response.Data.User.Email,
                response.Data.User.Name,
                response.Data.User.CreatedAt,
                response.Data.User.IdEmpleado > 0
                    ? response.Data.User.IdEmpleado
                    : null);

            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Refresh Token Error: {ex.Message}");
            return false;
        }
    }
}
