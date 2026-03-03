using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SPBackend.Services.Outbox;

public static class OutboxJson
{
    public static readonly JsonSerializerOptions SerializerOptions = new()
    {
        Converters =
        {
            new JsonStringEnumConverter(),
            new TimeSpanConverter()
        }
    };

    private sealed class TimeSpanConverter : JsonConverter<TimeSpan>
    {
        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var value = reader.GetString();
                return TimeSpan.Parse(value ?? "00:00:00", CultureInfo.InvariantCulture);
            }

            if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt64(out var ticks))
            {
                return TimeSpan.FromTicks(ticks);
            }

            throw new JsonException("Invalid TimeSpan value.");
        }

        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("c", CultureInfo.InvariantCulture));
        }
    }
}
