using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using tmr_mobile.Services;

namespace tmr_mobile.Services;

public class ApiService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new DateOnlyJsonConverter() }
    };

    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://dev.api.tmr2.dokploy.integritysolutions.com.ec/api/";

    public ApiService()
    {
        var handler = new HttpClientHandler();
        // Permitir certs en desarrollo local si es necesario
        handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
        _httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri(BaseUrl)
        };
    }

    private async Task AddAuthHeaderAsync()
    {
        var token = await SecureStorage.Default.GetAsync("auth_token");
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    public async Task<TResponse?> GetAsync<TResponse>(string endpoint)
    {
        await AddAuthHeaderAsync();
        var response = await _httpClient.GetAsync(endpoint);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<TResponse>(SerializerOptions);
        }

        var error = await response.Content.ReadAsStringAsync();
        throw new HttpRequestException($"GET {endpoint} failed ({response.StatusCode}): {error}");
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
    {
        await AddAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync(endpoint, data, SerializerOptions);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<TResponse>(SerializerOptions);
        }

        var error = await response.Content.ReadAsStringAsync();
        throw new HttpRequestException($"POST {endpoint} failed ({response.StatusCode}): {error}");
    }

    public async Task<bool> PutAsync<TRequest>(string endpoint, TRequest data)
    {
        await AddAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync(endpoint, data, SerializerOptions);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(string endpoint)
    {
        await AddAuthHeaderAsync();
        var response = await _httpClient.DeleteAsync(endpoint);
        return response.IsSuccessStatusCode;
    }
}
