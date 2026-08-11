using System.Text.Json.Serialization;

namespace tmr_mobile.Services;

public sealed class ApiResponse<T>
{
    [JsonPropertyName("success")]
    public bool Success { get; init; }

    [JsonPropertyName("statusCode")]
    public int StatusCode { get; init; }

    [JsonPropertyName("message")]
    public string Message { get; init; } = string.Empty;

    [JsonPropertyName("data")]
    public T? Data { get; init; }

    [JsonPropertyName("errors")]
    public object? Errors { get; init; }
}
