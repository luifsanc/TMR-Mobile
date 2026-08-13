// Infrastructure/Converters/DdMmYyyyDateConverter.cs
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

public class DdMmYyyyDateConverter : JsonConverter<DateTime?>
{
    private const string Format = "dd-MM-yyyy";

    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (string.IsNullOrWhiteSpace(value)) return null;

        return DateTime.TryParseExact(value, Format, CultureInfo.InvariantCulture,
            DateTimeStyles.None, out var result) ? result : null;
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value.HasValue) writer.WriteStringValue(value.Value.ToString(Format, CultureInfo.InvariantCulture));
        else writer.WriteNullValue();
    }
}