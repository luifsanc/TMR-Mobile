using System.Text;
using System.Text.Json;
using tmr_shared.Wrappers;

namespace tmr_mobile.Services;

public interface IUserModuleAccessService
{
    Task<HashSet<string>> ObtenerModulosAsync(CancellationToken ct = default);
    Task<bool> EsColaboradorAsync(CancellationToken ct = default);
}

public sealed class UserModuleAccessService : IUserModuleAccessService
{
    private readonly ApiService _apiService;

    public UserModuleAccessService(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<HashSet<string>> ObtenerModulosAsync(CancellationToken ct = default)
    {
        ApiResponse<string[]>? response = null;
        try
        {
            response = await _apiService.GetAsync<ApiResponse<string[]>>(
                "auth/modules",
                ct);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"No se pudieron consultar los módulos: {ex.Message}");
        }

        var modulos = response?.Success == true && response.Data is not null
            ? response.Data
                .Where(nombre => !string.IsNullOrWhiteSpace(nombre))
                .ToHashSet(StringComparer.OrdinalIgnoreCase)
            : new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var roles = await ObtenerRolesTokenAsync();

        if (roles.Any(rol => rol is "ADMINISTRADOR" or "GERENTE" or "LIDER"))
        {
            modulos.Add("Carga Actividades");
        }

        // Un colaborador no puede acceder a la carga masiva, aunque un rol
        // heredado o una respuesta antigua del servidor incluyan el módulo.
        if (roles.Contains("COLABORADOR"))
        {
            modulos.Remove("Carga Actividades");
            modulos.Remove("Actividades");
        }

        if (roles.Any(rol => rol is "ADMINISTRADOR" or "GERENTE" or "LIDER" or "RECURSOS HUMANOS" or "ADMINISTRATIVO"))
        {
            modulos.Add("Dashboard");
        }

        return modulos;
    }

    public async Task<bool> EsColaboradorAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var roles = await ObtenerRolesTokenAsync();
        ct.ThrowIfCancellationRequested();
        return roles.Contains("COLABORADOR");
    }

    private static async Task<HashSet<string>> ObtenerRolesTokenAsync()
    {
        var roles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var token = await SecureStorage.Default.GetAsync(ApiService.TokenKey);

        if (string.IsNullOrWhiteSpace(token))
        {
            return roles;
        }

        try
        {
            var partes = token.Split('.');
            if (partes.Length < 2)
            {
                return roles;
            }

            var payload = partes[1].Replace('-', '+').Replace('_', '/');
            payload = payload.PadRight(payload.Length + ((4 - payload.Length % 4) % 4), '=');

            using var documento = JsonDocument.Parse(
                Encoding.UTF8.GetString(Convert.FromBase64String(payload)));

            foreach (var nombreClaim in new[]
                     {
                         "role",
                         "roles",
                         "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
                     })
            {
                if (!documento.RootElement.TryGetProperty(nombreClaim, out var claim))
                {
                    continue;
                }

                if (claim.ValueKind == JsonValueKind.Array)
                {
                    foreach (var valor in claim.EnumerateArray())
                    {
                        AgregarRol(roles, valor.GetString());
                    }
                }
                else if (claim.ValueKind == JsonValueKind.String)
                {
                    AgregarRol(roles, claim.GetString());
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"No se pudieron leer los roles del token: {ex.Message}");
        }

        return roles;
    }

    private static void AgregarRol(HashSet<string> roles, string? rol)
    {
        if (!string.IsNullOrWhiteSpace(rol))
        {
            roles.Add(rol.Trim().ToUpperInvariant());
        }
    }
}
