using tmr_shared.DTOs.Auth;
using tmr_shared.Wrappers;

namespace tmr_mobile.Services;

public interface IAuthService
{
    Task<bool> LoginAsync(string username, string password);
    Task<bool> RefreshTokenAsync();
    Task LogoutAsync();
    Task<bool> IsAuthenticatedAsync();
    Task<string?> GetTokenAsync();
    UserResponse? CurrentUser { get; }
}

public class AuthService : IAuthService
{
    private readonly ApiService _apiService;

    public UserResponse? CurrentUser { get; private set; }

    public AuthService(ApiService apiService)
    {
        _apiService = apiService;

        // Registrar el refresh callback en ApiService para manejar 401 automáticamente
        // sin crear un ciclo de dependencia circular.
        _apiService.SetRefreshTokenFunc(RefreshTokenAsync);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Login
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Autentica al usuario contra POST /api/auth/login.
    /// El backend devuelve ApiResponse&lt;AuthResponse&gt;, NO AuthResponse directamente.
    /// Guarda AccessToken, RefreshToken y TokenFamilyId en SecureStorage.
    /// </summary>
    public async Task<bool> LoginAsync(string username, string password)
    {
        try
        {
            var request = new LoginRequest(username, password);
            Console.WriteLine($"[AuthService] LOGIN → usuario: '{username}', endpoint: api/auth/login");

            // El backend envuelve la respuesta en ApiResponse<AuthResponse>
            var response = await _apiService.PostAsync<LoginRequest, ApiResponse<AuthResponse>>(
                "api/auth/login", request);

            Console.WriteLine($"[AuthService] LOGIN respuesta: {(response == null ? "NULL" : $"Success={response.Success}, HasData={response.Data != null}, Msg={response.Message}")}");

            if (response?.Success == true && response.Data != null)
            {
                var auth = response.Data;
                Console.WriteLine($"[AuthService] LOGIN OK → user={auth.User?.Email}, AT={auth.AccessToken?[..Math.Min(20, auth.AccessToken?.Length ?? 0)]}...");
                
                await SecureStorage.Default.SetAsync(ApiService.TokenKey,        auth.AccessToken ?? string.Empty);
                await SecureStorage.Default.SetAsync(ApiService.RefreshTokenKey, auth.RefreshToken ?? string.Empty);
                await SecureStorage.Default.SetAsync(ApiService.TokenFamilyKey,  auth.TokenFamilyId.ToString());
                
                CurrentUser = auth.User;
                return true;
            }

            Console.WriteLine($"[AuthService] LOGIN FALLÓ → response={response?.Message ?? "null response"}. Errors={string.Join(", ", response?.Errors?.Select(e => e.Message) ?? [])}");
            // Login fallido: limpiar por seguridad

            ClearStorage();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AuthService] Login error: {ex.Message}");
        }

        return false;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Refresh Token
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Renueva el AccessToken usando el RefreshToken guardado.
    /// POST /api/auth/refresh-token — no requiere Authorization header.
    /// </summary>
    public async Task<bool> RefreshTokenAsync()
    {
        try
        {
            var refreshToken = await SecureStorage.Default.GetAsync(ApiService.RefreshTokenKey);
            if (string.IsNullOrEmpty(refreshToken))
                return false;

            var response = await _apiService.PostAsync<object, ApiResponse<AuthResponse>>(
                "api/auth/refresh-token",
                new { RefreshToken = refreshToken });

            if (response?.Success == true && response.Data != null)
            {
                var auth = response.Data;
                await SecureStorage.Default.SetAsync(ApiService.TokenKey,        auth.AccessToken ?? string.Empty);
                await SecureStorage.Default.SetAsync(ApiService.RefreshTokenKey, auth.RefreshToken ?? string.Empty);
                await SecureStorage.Default.SetAsync(ApiService.TokenFamilyKey,  auth.TokenFamilyId.ToString());
                CurrentUser = auth.User;
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AuthService] Refresh error: {ex.Message}");
        }

        return false;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Logout
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Cierra sesión usando POST /api/auth/logout-rt (no requiere AT válido).
    /// Limpia SecureStorage y redirige al login.
    /// </summary>
    public async Task LogoutAsync()
    {
        try
        {
            var refreshToken = await SecureStorage.Default.GetAsync(ApiService.RefreshTokenKey);
            if (!string.IsNullOrEmpty(refreshToken))
            {
                // Usar logout-rt que no requiere AT (útil si el AT ya expiró)
                await _apiService.PostAsync<object, object>(
                    "api/auth/logout-rt",
                    new { RefreshToken = refreshToken });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AuthService] Logout error: {ex.Message}");
        }
        finally
        {
            ClearStorage();
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await SecureStorage.Default.GetAsync(ApiService.TokenKey);
        return !string.IsNullOrEmpty(token);
    }

    public async Task<string?> GetTokenAsync()
        => await SecureStorage.Default.GetAsync(ApiService.TokenKey);

    private void ClearStorage()
    {
        SecureStorage.Default.Remove(ApiService.TokenKey);
        SecureStorage.Default.Remove(ApiService.RefreshTokenKey);
        SecureStorage.Default.Remove(ApiService.TokenFamilyKey);
        CurrentUser = null;
    }
}
