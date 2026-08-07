namespace tmr_shared.Wrappers;

public record ApiError
{
    public string Code { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}

public record ApiResponse<T>
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public T? Data { get; init; }
    public IReadOnlyList<ApiError> Errors { get; init; } = [];
}