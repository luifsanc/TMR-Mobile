using System.Text;
using System.Text.Json;
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

internal sealed class PasswordChangeProfileResponse
{
    public bool? DebeCambiarPassword { get; set; }
    public PasswordChangeProfileResponse? Data { get; set; }
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
    private const string CurrentUserKey = "auth_current_user";

    private readonly ApiService _apiService;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

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
                    "auth/login",
                    payload
                );

            if (response?.Data?.AccessToken is not null)
            {
                await SaveSessionAsync(response.Data);
                await ConfirmPasswordChangeRequirementAsync();

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
        var refreshToken = await SecureStorage.Default.GetAsync(
            ApiService.RefreshTokenKey);

        try
        {
            var payload = new { refreshToken };
            var result = await _apiService.PostForResultAsync(
                "auth/logout",
                payload);

            if (!result.Success && !string.IsNullOrWhiteSpace(refreshToken))
            {
                await _apiService.PostForResultAsync(
                    "auth/logout-rt",
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
        SecureStorage.Default.Remove(CurrentUserKey);

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
                "auth/change-password",
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
        var token = await SecureStorage.Default.GetAsync(TokenKey);

        if (IsAccessTokenUsable(token))
        {
            await RestoreCurrentUserAsync();
            await ConfirmPasswordChangeRequirementAsync();
            return true;
        }

        var refreshed = await RefreshTokenAsync();
        if (refreshed)
            await ConfirmPasswordChangeRequirementAsync();

        return refreshed;
    }

    public async Task<string?> GetTokenAsync()
    {
        return await SecureStorage.Default.GetAsync(TokenKey);
    }

    private async Task<bool> RefreshTokenAsync()
    {
        var tokenBeforeRefresh = await SecureStorage.Default.GetAsync(TokenKey);
        await _refreshLock.WaitAsync();

        try
        {
            var currentToken = await SecureStorage.Default.GetAsync(TokenKey);
            if (!string.Equals(currentToken, tokenBeforeRefresh, StringComparison.Ordinal) &&
                IsAccessTokenUsable(currentToken))
            {
                await RestoreCurrentUserAsync();
                return true;
            }

            var refreshToken = await SecureStorage.Default.GetAsync(
                ApiService.RefreshTokenKey);

            if (string.IsNullOrWhiteSpace(refreshToken))
                return false;

            var response = await _apiService.PostAnonymousAsync<object, ApiLoginResponse>(
                "auth/refresh-token",
                new { refreshToken });

            if (response?.Data?.AccessToken is null)
                return false;

            await SaveSessionAsync(response.Data);

            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Refresh Token Error: {ex.Message}");
            return false;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private async Task SaveSessionAsync(AuthResponse session)
    {
        await SecureStorage.Default.SetAsync(ApiService.TokenKey, session.AccessToken);
        await SecureStorage.Default.SetAsync(ApiService.RefreshTokenKey, session.RefreshToken);
        await SecureStorage.Default.SetAsync(ApiService.TokenFamilyKey, session.TokenFamilyId.ToString());

        CurrentUser = new UserResponse(
            session.User.Id,
            session.User.Email,
            session.User.Name,
            session.User.CreatedAt,
            session.User.IdEmpleado > 0 ? session.User.IdEmpleado : null,
            session.User.DebeCambiarPassword ||
            ReadBooleanClaim(session.AccessToken, "must_change_password"));

        await SecureStorage.Default.SetAsync(
            CurrentUserKey,
            JsonSerializer.Serialize(CurrentUser));
    }

    private async Task RestoreCurrentUserAsync()
    {
        if (CurrentUser is not null)
            return;

        try
        {
            var serializedUser = await SecureStorage.Default.GetAsync(CurrentUserKey);
            if (!string.IsNullOrWhiteSpace(serializedUser))
                CurrentUser = JsonSerializer.Deserialize<UserResponse>(serializedUser);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Restore User Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Replica el flujo web: confirma contra el perfil persistido si el
    /// administrador dejó pendiente el cambio obligatorio de contraseña.
    /// </summary>
    private async Task ConfirmPasswordChangeRequirementAsync()
    {
        if (CurrentUser is null || CurrentUser.Id <= 0)
            return;

        try
        {
            var profile = await _apiService.GetAsync<PasswordChangeProfileResponse>(
                $"configuracion/usuarios/{CurrentUser.Id}");

            var persistedValue = profile?.Data?.DebeCambiarPassword
                ?? profile?.DebeCambiarPassword;

            if (!persistedValue.HasValue)
                return;

            CurrentUser = CurrentUser with
            {
                DebeCambiarPassword = persistedValue.Value
            };

            await SecureStorage.Default.SetAsync(
                CurrentUserKey,
                JsonSerializer.Serialize(CurrentUser));

            System.Diagnostics.Debug.WriteLine(
                $"[AUTH] Cambio obligatorio confirmado: {persistedValue.Value}");
        }
        catch (Exception ex)
        {
            // Igual que la web: no reemplazar con false un true recibido en
            // login/JWT cuando la confirmación del perfil no esté disponible.
            System.Diagnostics.Debug.WriteLine(
                $"[AUTH] No se pudo confirmar el cambio obligatorio: {ex.Message}");
        }
    }

    private static bool IsAccessTokenUsable(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;

        try
        {
            var parts = token.Split('.');
            if (parts.Length < 2)
                return false;

            var payload = parts[1].Replace('-', '+').Replace('_', '/');
            payload = payload.PadRight(payload.Length + ((4 - payload.Length % 4) % 4), '=');

            using var document = JsonDocument.Parse(
                Encoding.UTF8.GetString(Convert.FromBase64String(payload)));

            if (!document.RootElement.TryGetProperty("exp", out var expiration) ||
                !expiration.TryGetInt64(out var expirationSeconds))
                return false;

            var expiresAt = DateTimeOffset.FromUnixTimeSeconds(expirationSeconds);
            return expiresAt > DateTimeOffset.UtcNow.AddMinutes(1);
        }
        catch
        {
            return false;
        }
    }

    private static bool ReadBooleanClaim(string? token, string claimName)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;

        try
        {
            var parts = token.Split('.');
            if (parts.Length < 2)
                return false;

            var payload = parts[1].Replace('-', '+').Replace('_', '/');
            payload = payload.PadRight(payload.Length + ((4 - payload.Length % 4) % 4), '=');

            using var document = JsonDocument.Parse(
                Encoding.UTF8.GetString(Convert.FromBase64String(payload)));

            if (!document.RootElement.TryGetProperty(claimName, out var claim))
                return false;

            return claim.ValueKind == JsonValueKind.True ||
                   (claim.ValueKind == JsonValueKind.String &&
                    bool.TryParse(claim.GetString(), out var value) && value);
        }
        catch
        {
            return false;
        }
    }
}
