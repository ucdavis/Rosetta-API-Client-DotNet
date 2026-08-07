using System.Text.Json;
using System.Text.Json.Serialization;

namespace UCD.Rosetta.Client.Core.Converters;

/// <summary>
/// A JsonConverterFactory that wraps ICollection&lt;T&gt; deserialization,
/// gracefully handling null tokens, scalar values, or unexpected element values rather than throwing.
/// This handles real-world API responses where arrays may contain null placeholders
/// or where collection properties may occasionally be returned as a single scalar value.
/// </summary>
public class LenientTypedCollectionConverterFactory : JsonConverterFactory
{
    /// <inheritdoc />
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsGenericType)
            return false;

        var genericDef = typeToConvert.GetGenericTypeDefinition();
        if (genericDef != typeof(ICollection<>))
            return false;

        return true;
    }

    /// <inheritdoc />
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var elementType = typeToConvert.GetGenericArguments()[0];
        var converterType = typeof(LenientTypedCollectionConverter<>).MakeGenericType(elementType);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}

/// <summary>
/// Deserializes ICollection&lt;T&gt; from a JSON array or compatible scalar value.
/// </summary>
internal class LenientTypedCollectionConverter<T> : JsonConverter<ICollection<T>>
{
    public override ICollection<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return [];

        if (reader.TokenType != JsonTokenType.StartArray)
            return ReadSingleValue(ref reader, options);

        // CanConvert only matches ICollection<T>, never T itself, so no recursion risk
        // when we call Deserialize<T> below.
        var list = new List<T>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
                return list;

            if (reader.TokenType == JsonTokenType.Null)
            {
                reader.Skip();
                continue;
            }

            if (!CanReadCurrentTokenAsElement(reader.TokenType))
            {
                reader.Skip();
                continue;
            }

            if (TryReadElement(ref reader, options, out var item))
                list.Add(item);
        }

        return list;
    }

    public override void Write(Utf8JsonWriter writer, ICollection<T> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (var item in value)
            JsonSerializer.Serialize(writer, item, options);
        writer.WriteEndArray();
    }

    private static ICollection<T> ReadSingleValue(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        if (!CanReadCurrentTokenAsElement(reader.TokenType))
        {
            reader.Skip();
            return new List<T>();
        }

        return TryReadElement(ref reader, options, out var item)
            ? [item]
            : [];
    }

    private static bool CanReadCurrentTokenAsElement(JsonTokenType tokenType)
    {
        if (typeof(T) == typeof(string))
            return tokenType == JsonTokenType.String;

        if (typeof(T).IsClass || Nullable.GetUnderlyingType(typeof(T)) != null)
            return tokenType == JsonTokenType.StartObject;

        return tokenType
            is JsonTokenType.Number
            or JsonTokenType.True
            or JsonTokenType.False;
    }

    private static bool TryReadElement(ref Utf8JsonReader reader, JsonSerializerOptions options, out T item)
    {
        item = default!;

        try
        {
            var value = JsonSerializer.Deserialize<T>(ref reader, options);
            if (value == null)
                return false;

            item = value;
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
