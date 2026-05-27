using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Custom JSON converter để serialize/deserialize DateOnly <-> "yyyy-MM-dd" string
/// ASP.NET Core System.Text.Json không hỗ trợ DateOnly mặc định trước .NET 7.
/// </summary>
public class DateOnlyJsonConverter : JsonConverter<DateOnly>
{
    private const string Format = "yyyy-MM-dd";

    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (string.IsNullOrWhiteSpace(value))
            return default;

        return DateOnly.ParseExact(value, Format, null);
    }

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(Format));
    }
}

/// <summary>
/// Custom JSON converter cho TimeOnly <-> "HH:mm" string
/// </summary>
public class TimeOnlyJsonConverter : JsonConverter<TimeOnly>
{
    private const string Format = "HH:mm";

    public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (string.IsNullOrWhiteSpace(value))
            return default;
        return TimeOnly.ParseExact(value, Format, null);
    }

    public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(Format));
    }
}

/// <summary>
/// Custom JSON converter cho nullable TimeOnly?
/// </summary>
public class NullableTimeOnlyJsonConverter : JsonConverter<TimeOnly?>
{
    private const string Format = "HH:mm";

    public override TimeOnly? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null) return null;
        var value = reader.GetString();
        if (string.IsNullOrWhiteSpace(value)) return null;
        return TimeOnly.ParseExact(value, Format, null);
    }

    public override void Write(Utf8JsonWriter writer, TimeOnly? value, JsonSerializerOptions options)
    {
        if (value is null)
            writer.WriteNullValue();
        else
            writer.WriteStringValue(value.Value.ToString(Format));
    }
}
