using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace tmr_mobile.Services;

public class DateOnlyJsonConverter : JsonConverter<DateOnly?>
{
    private static readonly string[] Formatos =
    {
        "dd-MM-yyyy",
        "dd/MM/yyyy",
        "yyyy-MM-dd",
        "yyyy-MM-ddTHH:mm:ss",
        "yyyy-MM-ddTHH:mm:ss.FFFFFFFK"
    };

    public override DateOnly? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        var valor = reader.GetString();
        if (string.IsNullOrWhiteSpace(valor))
            return null;

        if (DateOnly.TryParseExact(valor, Formatos, CultureInfo.InvariantCulture, DateTimeStyles.None, out var fecha))
            return fecha;

        if (DateTime.TryParse(valor, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var fechaHora))
            return DateOnly.FromDateTime(fechaHora);

        return null;
    }

    public override void Write(Utf8JsonWriter writer, DateOnly? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStringValue(value.Value.ToString("yyyy-MM-dd"));
    }
}
