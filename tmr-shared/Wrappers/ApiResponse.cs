namespace tmr_shared.Wrappers;

/// <summary>
/// Wrapper genérico que espeja la estructura ApiResponse<T> del backend.
/// Todos los endpoints de Auth devuelven esta estructura.
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; init; }
    public int StatusCode { get; init; }
    public string Message { get; init; } = string.Empty;
    public T? Data { get; init; }
    public IEnumerable<ApiError>? Errors { get; init; }
    public PaginationMeta? Meta { get; init; }
}

public class ApiError
{
    public string Field { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}

public class PaginationMeta
{
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int Total { get; init; }
}
