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
        return TryReadElement(ref reader, options, out var item)
            ? [item]
            : [];
    }

    private static bool TryReadElement(ref Utf8JsonReader reader, JsonSerializerOptions options, out T item)
    {
        item = default!;
        var readerCopy = reader;

        try
        {
            var value = JsonSerializer.Deserialize<T>(ref readerCopy, options);
            if (value == null)
            {
                reader.Skip();
                return false;
            }

            item = value;
            reader = readerCopy;
            return true;
        }
        catch (JsonException)
        {
            reader.Skip();
            return false;
        }
    }
}
