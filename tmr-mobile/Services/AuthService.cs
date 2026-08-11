using tmr_shared.DTOs.Auth;

namespace tmr_mobile.Services;

public interface IAuthService
{
    Task<bool> LoginAsync(string username, string password);
    Task LogoutAsync();
    Task<bool> IsAuthenticatedAsync();
    Task<string?> GetTokenAsync();
    tmr_shared.DTOs.Auth.UserResponse? CurrentUser { get; }
}

public class AuthService : IAuthService
{
    private const string TokenKey = "auth_token";
    private readonly ApiService _apiService;

    public tmr_shared.DTOs.Auth.UserResponse? CurrentUser { get; private set; }

    public AuthService(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        try
        {
            var request = new LoginRequest(username, password);
            var response = await _apiService.PostAsync<LoginRequest, ApiResponse<AuthResponse>>("auth/login", request);

            if (response is { Success: true, Data: { } authData })
            {
                await PersistSessionAsync(authData.AccessToken, authData.User);
                return true;
            }

            System.Diagnostics.Debug.WriteLine($"[Login Error] Login failed: {response?.Message ?? "No response"}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Login Error] Type: {ex.GetType().Name}");
            System.Diagnostics.Debug.WriteLine($"[Login Error] Message: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[Login Error] StackTrace: {ex.StackTrace}");
        }

        return false;
    }

    private async Task PersistSessionAsync(string accessToken, UserResponse user)
    {
        await SecureStorage.Default.SetAsync(TokenKey, accessToken);
        CurrentUser = new UserResponse(
            user.Id,
            user.Email,
            user.Name,
            user.CreatedAt,
            user.IdEmpleado > 0 ? user.IdEmpleado : null);
    }

    public async Task LogoutAsync()
    {
        SecureStorage.Default.Remove(TokenKey);
        CurrentUser = null;
        await Shell.Current.GoToAsync("//LoginPage");
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await SecureStorage.Default.GetAsync(TokenKey);
        return !string.IsNullOrEmpty(token);
    }

    public async Task<string?> GetTokenAsync()
    {
        return await SecureStorage.Default.GetAsync(TokenKey);
    }
}
