using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
 
namespace tmr_shared.DTOs.Dashboard;
 
/// <summary>
/// El endpoint de dashboard devuelve fechas como "15-01-2025" (dd-MM-yyyy),
/// que no es un formato ISO 8601 reconocido por System.Text.Json por defecto.
/// Este converter parsea ese formato (y tolera null/"" -> null).
/// </summary>
public sealed class DdMmYyyyDateTimeConverter : JsonConverter<DateTime?>
{
    private const string Format = "dd-MM-yyyy";
 
    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;
 
        var value = reader.GetString();
 
        if (string.IsNullOrWhiteSpace(value))
            return null;
 
        if (DateTime.TryParseExact(value, Format, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var parsed))
        {
            return parsed;
        }
 
        // Fallback por si el backend cambia el formato sin avisar.
        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var fallback))
            return fallback;
 
        return null;
    }
 
    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
            writer.WriteStringValue(value.Value.ToString(Format, CultureInfo.InvariantCulture));
        else
            writer.WriteNullValue();
    }
}