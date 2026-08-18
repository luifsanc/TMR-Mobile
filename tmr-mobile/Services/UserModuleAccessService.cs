using tmr_shared.Wrappers;

namespace tmr_mobile.Services;

public interface IUserModuleAccessService
{
    Task<HashSet<string>> ObtenerModulosAsync(CancellationToken ct = default);
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
        var response = await _apiService.GetAsync<ApiResponse<string[]>>(
            "auth/modules",
            ct);

        if (response?.Success != true || response.Data is null)
        {
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        return response.Data
            .Where(nombre => !string.IsNullOrWhiteSpace(nombre))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }
}
