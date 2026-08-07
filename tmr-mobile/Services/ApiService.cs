using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using tmr_shared.Wrappers;

namespace tmr_mobile.Services;

/// <summary>
/// Servicio centralizado para consumir el backend TMR.
/// - Singleton: una sola instancia de HttpClient en toda la app.
/// - Agrega Authorization header automáticamente en cada petición.
/// - Maneja 401: intenta renovar el AccessToken antes de fallar.
/// - Timeout de 30 segundos por defecto.
/// </summary>
public class ApiService
{
    private readonly HttpClient _httpClient;

    // ── Configuración de URL ─────────────────────────────────────────────────
    // Apuntando al entorno de desarrollo remoto para evitar problemas de loopback local
    private const string BaseUrl = "https://dev.api.tmr2.dokploy.integritysolutions.com.ec/";

    // Claves en SecureStorage
    internal const string TokenKey        = "auth_token";
    internal const string RefreshTokenKey = "refresh_token";
    internal const string TokenFamilyKey  = "token_family_id";

    // Para evitar ciclo: el AuthService se inyecta lazily desde el exterior
    private Func<Task<bool>>? _refreshTokenFunc;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public ApiService()
    {
        var handler = new HttpClientHandler();

#if DEBUG
        // Aceptar certificados auto-firmados solo en desarrollo
        handler.ServerCertificateCustomValidationCallback =
            (_, _, _, _) => true;
#endif

        _httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri(BaseUrl),
            Timeout     = TimeSpan.FromSeconds(30)
        };

        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
            
        // Agregar User-Agent por defecto para el control de sesiones del backend
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("TMR-Mobile-App/1.0");
    }

    /// <summary>
    /// Registra la función de refresh token para que ApiService pueda
    /// renovar automáticamente el AT al recibir un 401, sin ciclo de dependencia.
    /// Llamar desde AuthService después de construirlo.
    /// </summary>
    public void SetRefreshTokenFunc(Func<Task<bool>> refreshFunc)
        => _refreshTokenFunc = refreshFunc;

    // ─────────────────────────────────────────────────────────────────────────
    // Métodos públicos
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<TResponse?> GetAsync<TResponse>(string endpoint,
        CancellationToken ct = default)
    {
        await AddAuthHeaderAsync();
        var response = await _httpClient.GetAsync(endpoint, ct);
        return await HandleResponseAsync<TResponse>(response, endpoint, "GET", null, ct);
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data,
        CancellationToken ct = default)
    {
        await AddAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync(endpoint, data, JsonOptions, ct);
        return await HandleResponseAsync<TResponse>(response, endpoint, "POST", data, ct);
    }

    public async Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest data,
        CancellationToken ct = default)
    {
        await AddAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync(endpoint, data, JsonOptions, ct);
        return await HandleResponseAsync<TResponse>(response, endpoint, "PUT", data, ct);
    }

    public async Task<bool> DeleteAsync(string endpoint,
        CancellationToken ct = default)
    {
        await AddAuthHeaderAsync();
        var response = await _httpClient.DeleteAsync(endpoint, ct);
        return response.IsSuccessStatusCode;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Internals
    // ─────────────────────────────────────────────────────────────────────────

    private async Task AddAuthHeaderAsync()
    {
        var token = await SecureStorage.Default.GetAsync(TokenKey);
        if (!string.IsNullOrEmpty(token))
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        else
            _httpClient.DefaultRequestHeaders.Authorization = null;
    }

    private async Task<TResponse?> HandleResponseAsync<TResponse>(
        HttpResponseMessage response,
        string endpoint,
        string method,
        object? body,
        CancellationToken ct)
    {
        // ── 401: intentar renovar token y reintentar UNA VEZ ─────────────────
        if (response.StatusCode == HttpStatusCode.Unauthorized && _refreshTokenFunc != null)
        {
            Log($"[ApiService] 401 en {method} {endpoint} — intentando refresh...");
            var refreshed = await _refreshTokenFunc();
            if (refreshed)
            {
                await AddAuthHeaderAsync();
                // Reintentar la petición original
                HttpResponseMessage retry;
                if (method == "GET")
                    retry = await _httpClient.GetAsync(endpoint, ct);
                else if (method == "POST")
                    retry = await _httpClient.PostAsJsonAsync(endpoint, body, JsonOptions, ct);
                else
                    retry = await _httpClient.PutAsJsonAsync(endpoint, body, JsonOptions, ct);

                return await DeserializeAsync<TResponse>(retry);
            }

            // No se pudo renovar: sesión expirada
            Log("[ApiService] Refresh fallido — redirigiendo a login.");
            await MainThread.InvokeOnMainThreadAsync(async () =>
                await Shell.Current.GoToAsync("//LoginPage"));
            return default;
        }

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            Log($"[ApiService] Error {(int)response.StatusCode} en {method} {endpoint}: {error}");
            return default;
        }

        return await DeserializeAsync<TResponse>(response);
    }

    private async Task<TResponse?> DeserializeAsync<TResponse>(HttpResponseMessage response)
    {
        try
        {
            return await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions);
        }
        catch (JsonException ex)
        {
            Log($"[ApiService] Error de deserialización: {ex.Message}");
            return default;
        }
    }

    private static void Log(string message)
        => System.Diagnostics.Debug.WriteLine(message);
}
