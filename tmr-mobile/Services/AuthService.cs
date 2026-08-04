using tmr_shared.DTOs.Auth;

namespace tmr_mobile.Services;

public interface IAuthService
{
    Task<bool> LoginAsync(string username, string password);
    Task LogoutAsync();
    Task<bool> IsAuthenticatedAsync();
    Task<string?> GetTokenAsync();
    UserResponse? CurrentUser { get; }
}

public class AuthService : IAuthService
{
    private const string TokenKey = "auth_token";
    private const string UserKey = "auth_user";
    private readonly ApiService _apiService;

    public UserResponse? CurrentUser { get; private set; }

    public AuthService(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        try
        {
            var request = new LoginRequest(username, password);
            var response = await _apiService.PostAsync<LoginRequest, AuthResponse>("api/auth/login", request);

            if (response != null && !string.IsNullOrEmpty(response.AccessToken))
            {
                await SecureStorage.Default.SetAsync(TokenKey, response.AccessToken);
                CurrentUser = response.User;
                return true;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Login Error: {ex.Message}");
        }

        return false;
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
