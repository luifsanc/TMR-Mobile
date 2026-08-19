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
    private const string BaseUrl = "https://dev.api.tmr2.dokploy.integritysolutions.com.ec/api/";

    // Claves en SecureStorage
    internal const string TokenKey = "auth_token";
    internal const string RefreshTokenKey = "refresh_token";
    internal const string TokenFamilyKey = "token_family_id";

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
            Timeout = TimeSpan.FromSeconds(30)
        };

        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        // Agregar User-Agent por defecto para el control de sesiones del backend
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("TMR-Mobile-App/1.0");
    }

    private string NormalizeEndpoint(string endpoint)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
            return string.Empty;

        // Aceptar formatos: "proyectos", "/proyectos", "api/proyectos", "/api/proyectos"
        var e = endpoint.TrimStart('/');

        // Si BaseAddress ya termina en "api/" y el endpoint comienza con "api/", evitar duplicado
        if (_httpClient?.BaseAddress != null &&
            _httpClient.BaseAddress.AbsoluteUri.TrimEnd('/').EndsWith("/api", StringComparison.OrdinalIgnoreCase) &&
            e.StartsWith("api/", StringComparison.OrdinalIgnoreCase))
        {
            e = e.Substring(4); // quitar "api/"
        }

        return e;
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
        endpoint = NormalizeEndpoint(endpoint);

        var hasAuthHeader = !string.IsNullOrWhiteSpace(_httpClient.DefaultRequestHeaders.Authorization?.ToString());
        if (!hasAuthHeader && !endpoint.StartsWith("auth/", StringComparison.OrdinalIgnoreCase))
        {
            var message = "No hay sesión activa. Inicia sesión nuevamente.";
            Log($"[ApiService] Bloqueado GET {endpoint}: {message}");
            throw new InvalidOperationException(message);
        }

        try
        {
            var response = await _httpClient.GetAsync(endpoint, ct);
            Log($"[ApiService] GET {endpoint} -> {(int)response.StatusCode} {response.ReasonPhrase}");
            return await HandleResponseAsync<TResponse>(response, endpoint, "GET", null, ct);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Log($"[ApiService] Exception en GET {endpoint}: {ex.Message}");
            throw new InvalidOperationException("No se pudo cargar la información del servidor. Inténtalo nuevamente.", ex);
        }
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data,
        CancellationToken ct = default)
    {
        await AddAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync(endpoint, data, JsonOptions, ct);
        return await HandleResponseAsync<TResponse>(response, endpoint, "POST", data, ct);
    }

    /// <summary>
    /// POST sin credenciales ni reintento de autenticación. Se usa únicamente
    /// para endpoints públicos como refresh-token.
    /// </summary>
    internal async Task<TResponse?> PostAnonymousAsync<TRequest, TResponse>(
        string endpoint,
        TRequest data,
        CancellationToken ct = default)
    {
        endpoint = NormalizeEndpoint(endpoint);
        using var response = await _httpClient.PostAsJsonAsync(endpoint, data, JsonOptions, ct);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            Log($"[ApiService] Error {(int)response.StatusCode} en POST {endpoint}: {error}");
            return default;
        }

        return await DeserializeAsync<TResponse>(response);
    }

    /// <summary>
    /// Ejecuta un POST cuyo resultado puede no tener cuerpo (por ejemplo, HTTP 204)
    /// y conserva el mensaje de error devuelto por el backend.
    /// </summary>
    public async Task<ApiOperationResult> PostForResultAsync<TRequest>(
        string endpoint,
        TRequest data,
        CancellationToken ct = default)
    {
        await AddAuthHeaderAsync();
        endpoint = NormalizeEndpoint(endpoint);
        using var response = await _httpClient.PostAsJsonAsync(endpoint, data, JsonOptions, ct);

        if (response.StatusCode == HttpStatusCode.Unauthorized && _refreshTokenFunc != null)
        {
            Log($"[ApiService] 401 en POST {endpoint} — intentando refresh...");
            var refreshed = await _refreshTokenFunc();

            if (refreshed)
            {
                await AddAuthHeaderAsync();
                using var retry = await _httpClient.PostAsJsonAsync(endpoint, data, JsonOptions, ct);
                var retryContent = await retry.Content.ReadAsStringAsync(ct);

                return retry.IsSuccessStatusCode
                    ? ApiOperationResult.Ok()
                    : ApiOperationResult.Fail(ExtractErrorMessage(retryContent), retry.StatusCode);
            }
        }

        var content = await response.Content.ReadAsStringAsync(ct);

        if (response.IsSuccessStatusCode)
            return ApiOperationResult.Ok();

        Log($"[ApiService] Error {(int)response.StatusCode} en POST {endpoint}: {content}");
        return ApiOperationResult.Fail(ExtractErrorMessage(content), response.StatusCode);
    }

    public async Task<ApiOperationResult> PutForResultAsync<TRequest>(
        string endpoint,
        TRequest data,
        CancellationToken ct = default)
    {
        await AddAuthHeaderAsync();
        endpoint = NormalizeEndpoint(endpoint);
        using var response = await _httpClient.PutAsJsonAsync(endpoint, data, JsonOptions, ct);

        if (response.StatusCode == HttpStatusCode.Unauthorized && _refreshTokenFunc != null)
        {
            Log($"[ApiService] 401 en PUT {endpoint} — intentando refresh...");
            var refreshed = await _refreshTokenFunc();

            if (refreshed)
            {
                await AddAuthHeaderAsync();
                using var retry = await _httpClient.PutAsJsonAsync(endpoint, data, JsonOptions, ct);
                var retryContent = await retry.Content.ReadAsStringAsync(ct);

                return retry.IsSuccessStatusCode
                    ? ApiOperationResult.Ok()
                    : ApiOperationResult.Fail(ExtractErrorMessage(retryContent), retry.StatusCode);
            }
        }

        var content = await response.Content.ReadAsStringAsync(ct);

        if (response.IsSuccessStatusCode)
            return ApiOperationResult.Ok();

        Log($"[ApiService] Error {(int)response.StatusCode} en PUT {endpoint}: {content}");
        return ApiOperationResult.Fail(ExtractErrorMessage(content), response.StatusCode);
    }

    public async Task<ApiOperationResult> DeleteForResultAsync(
        string endpoint,
        CancellationToken ct = default)
    {
        await AddAuthHeaderAsync();
        endpoint = NormalizeEndpoint(endpoint);
        using var response = await _httpClient.DeleteAsync(endpoint, ct);

        if (response.StatusCode == HttpStatusCode.Unauthorized && _refreshTokenFunc != null)
        {
            Log($"[ApiService] 401 en DELETE {endpoint} — intentando refresh...");
            var refreshed = await _refreshTokenFunc();

            if (refreshed)
            {
                await AddAuthHeaderAsync();
                using var retry = await _httpClient.DeleteAsync(endpoint, ct);
                var retryContent = await retry.Content.ReadAsStringAsync(ct);

                return retry.IsSuccessStatusCode
                    ? ApiOperationResult.Ok()
                    : ApiOperationResult.Fail(ExtractErrorMessage(retryContent), retry.StatusCode);
            }
        }

        var content = await response.Content.ReadAsStringAsync(ct);

        if (response.IsSuccessStatusCode)
            return ApiOperationResult.Ok();

        Log($"[ApiService] Error {(int)response.StatusCode} en DELETE {endpoint}: {content}");
        return ApiOperationResult.Fail(ExtractErrorMessage(content), response.StatusCode);
    }

    public async Task<TResponse?> PostFileAsync<TResponse>(string endpoint, byte[] fileBytes, string fileName,
        CancellationToken ct = default)
    {
        await AddAuthHeaderAsync();
        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        content.Add(fileContent, "file", fileName);

        endpoint = NormalizeEndpoint(endpoint);
        var response = await _httpClient.PostAsync(endpoint, content, ct);
        return await HandleResponseAsync<TResponse>(response, endpoint, "POST", null, ct);
    }

    public async Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest data,
        CancellationToken ct = default)
    {
        await AddAuthHeaderAsync();
        endpoint = NormalizeEndpoint(endpoint);
        var response = await _httpClient.PutAsJsonAsync(endpoint, data, JsonOptions, ct);
        return await HandleResponseAsync<TResponse>(response, endpoint, "PUT", data, ct);
    }

    public async Task<TResponse?> PatchAsync<TRequest, TResponse>(string endpoint, TRequest data,
        CancellationToken ct = default)
    {
        await AddAuthHeaderAsync();
        endpoint = NormalizeEndpoint(endpoint);
        var response = await _httpClient.PatchAsJsonAsync(endpoint, data, JsonOptions, ct);
        return await HandleResponseAsync<TResponse>(response, endpoint, "PATCH", data, ct);
    }

    public async Task<bool> PutAsync<TRequest>(string endpoint, TRequest data,
        CancellationToken ct = default)
    {
        await AddAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync(endpoint, data, JsonOptions, ct);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(string endpoint,
        CancellationToken ct = default)
    {
        await AddAuthHeaderAsync();
        endpoint = NormalizeEndpoint(endpoint);
        var response = await _httpClient.DeleteAsync(endpoint, ct);
        return response.IsSuccessStatusCode;
    }

    //metodo para descargar archivos binarios desde el backend
    public async Task<byte[]?> GetFileBytesAsync(string endpoint, CancellationToken ct = default)
    {
        await AddAuthHeaderAsync();
        endpoint = NormalizeEndpoint(endpoint);
        var response = await _httpClient.GetAsync(endpoint, ct);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            Log($"[ApiService] Error {(int)response.StatusCode} descargando {endpoint}: {error}");
            return null;
        }

        return await response.Content.ReadAsByteArrayAsync(ct);
    }

    //metodo para subir archivos binarios al backend y recibir una respuesta con detalles
    public async Task<TResponse?> PostFileWithDetailsAsync<TResponse>(
    string endpoint, byte[] fileBytes, string fileName, string contentType,
    CancellationToken ct = default)
    {
        await AddAuthHeaderAsync();

        HttpContent BuildContent()
        {
            var content = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(fileBytes);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            content.Add(fileContent, "file", fileName);
            return content;
        }

        endpoint = NormalizeEndpoint(endpoint);
        var response = await _httpClient.PostAsync(endpoint, BuildContent(), ct);

        if (response.StatusCode == HttpStatusCode.Unauthorized && _refreshTokenFunc != null)
        {
            var refreshed = await _refreshTokenFunc();
            if (!refreshed)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                    await Shell.Current.GoToAsync("//LoginPage"));
                return default;
            }

            await AddAuthHeaderAsync();
            // El content anterior ya se envió y no se puede reenviar; se arma de nuevo.
            response = await _httpClient.PostAsync(endpoint, BuildContent(), ct);
        }

        // A diferencia de HandleResponseAsync, aquí SIEMPRE deserializamos el body,
        // sin importar el status code: el endpoint devuelve el mismo shape de
        // respuesta tanto en éxito (200) como en error de validación (400).
        try
        {
            return await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, ct);
        }
        catch (JsonException ex)
        {
            Log($"[ApiService] Error de deserialización en {endpoint}: {ex.Message}");
            return default;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Internals
    // ─────────────────────────────────────────────────────────────────────────

    private async Task AddAuthHeaderAsync()
    {
        var token = await SecureStorage.Default.GetAsync(TokenKey);
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
            Log("[ApiService] Token presente en SecureStorage (no se muestra).");
        }
        else
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
            Log("[ApiService] No hay token en SecureStorage.");
        }
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
                else if (method == "PATCH")
                    retry = await _httpClient.PatchAsJsonAsync(endpoint, body, JsonOptions, ct);
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
            var content = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrWhiteSpace(content))
            {
                var sample = content.Length > 1000 ? content.Substring(0, 1000) + "..." : content;
                Log($"[ApiService] Body ({response.RequestMessage?.RequestUri}): {sample}");
            }

            if (string.IsNullOrWhiteSpace(content))
                return default;

            return JsonSerializer.Deserialize<TResponse>(content, JsonOptions);
        }
        catch (JsonException ex)
        {
            Log($"[ApiService] Error de deserialización: {ex.Message}");
            return default;
        }
        catch (Exception ex)
        {
            Log($"[ApiService] Error leyendo respuesta: {ex.Message}");
            return default;
        }
    }

    private static void Log(string message)
    {
        System.Diagnostics.Debug.WriteLine(message);
        Console.WriteLine(message);
    }

    private static string ExtractErrorMessage(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return "No se pudo completar la solicitud.";

        try
        {
            using var document = JsonDocument.Parse(content);
            var root = document.RootElement;

            foreach (var propertyName in new[] { "detail", "message", "mensaje", "title" })
            {
                if (root.TryGetProperty(propertyName, out var property) &&
                    property.ValueKind == JsonValueKind.String &&
                    !string.IsNullOrWhiteSpace(property.GetString()))
                    return property.GetString()!;
            }

            if (root.TryGetProperty("errors", out var errors) &&
                errors.ValueKind == JsonValueKind.Array)
            {
                foreach (var error in errors.EnumerateArray())
                {
                    if (error.ValueKind == JsonValueKind.String)
                        return error.GetString()!;

                    if (error.ValueKind == JsonValueKind.Object &&
                        error.TryGetProperty("message", out var message) &&
                        message.ValueKind == JsonValueKind.String)
                        return message.GetString()!;
                }
            }
        }
        catch (JsonException)
        {
            // Algunos proxies devuelven texto plano; se muestra tal cual.
        }

        return content.Trim('"', ' ', '\r', '\n');
    }
}

public sealed record ApiOperationResult(
    bool Success,
    string Message,
    HttpStatusCode StatusCode)
{
    public static ApiOperationResult Ok() =>
        new(true, string.Empty, HttpStatusCode.OK);

    public static ApiOperationResult Fail(string message, HttpStatusCode statusCode) =>
        new(false, message, statusCode);
}
