using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace tmr_shared.DTOs.Proyectos;

public class DdMmYyyyDateOnlyConverter : JsonConverter<DateOnly?>
{
    private const string Format = "dd-MM-yyyy";

    public override DateOnly? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        var value = reader.GetString();
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (DateOnly.TryParseExact(value, Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var exact))
            return exact;

        if (DateOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            return parsed;

        return null;
    }

    public override void Write(Utf8JsonWriter writer, DateOnly? value, JsonSerializerOptions options)
    {
        if (!value.HasValue)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStringValue(value.Value.ToString(Format, CultureInfo.InvariantCulture));
    }
}
