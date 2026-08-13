using System.Text.Json.Serialization;

namespace tmr_shared.DTOs.Proyectos;

public record ProyectoResponse
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("nombre")]
    public string Nombre { get; init; } = string.Empty;

    [JsonPropertyName("cliente")]
    public string Cliente { get; init; } = string.Empty;

    [JsonPropertyName("estado")]
    public string Estado { get; init; } = string.Empty;

    [JsonPropertyName("lider")]
    public string LiderAsignado { get; init; } = string.Empty;

    [JsonPropertyName("numeroRecursos")]
    public int Recursos { get; init; }

    [JsonPropertyName("fechaInicio")]
    [JsonConverter(typeof(DdMmYyyyDateConverter))]
    public DateTime? FechaInicio { get; init; }

    [JsonPropertyName("fechaFin")]
    [JsonConverter(typeof(DdMmYyyyDateConverter))]
    public DateTime? FechaFin { get; init; }
}