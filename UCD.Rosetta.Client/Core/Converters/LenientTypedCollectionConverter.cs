using System.Text.Json;
using System.Text.Json.Serialization;

namespace UCD.Rosetta.Client.Core.Converters;

/// <summary>
/// A JsonConverterFactory that wraps ICollection&lt;T&gt; deserialization for model types,
/// gracefully skipping null tokens or unexpected non-object values rather than throwing.
/// This handles real-world API responses where arrays may contain null placeholders
/// or unexpected primitives for entries with no data.
/// </summary>
public class LenientTypedCollectionConverterFactory : JsonConverterFactory
{
    /// <inheritdoc />
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsGenericType)
            return false;

        var genericDef = typeToConvert.GetGenericTypeDefinition();
        if (genericDef != typeof(ICollection<>) && genericDef != typeof(List<>))
            return false;

        var elementType = typeToConvert.GetGenericArguments()[0];

        // Only handle concrete classes that are not primitives, strings, or object itself;
        // those are handled by the default System.Text.Json converters.
        return elementType.IsClass
            && elementType != typeof(string)
            && elementType != typeof(object);
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
/// Deserializes ICollection&lt;T&gt; from a JSON array, skipping any elements that are not
/// JSON objects (e.g. null tokens, strings, or numbers) instead of throwing.
/// </summary>
internal class LenientTypedCollectionConverter<T> : JsonConverter<ICollection<T>> where T : class
{
    public override ICollection<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType != JsonTokenType.StartArray)
        {
            // Non-array value where an array was expected; skip and return empty
            reader.Skip();
            return new List<T>();
        }

        // CanConvert only matches ICollection<T>, never T itself, so no recursion risk
        // when we call Deserialize<T> below.
        var list = new List<T>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
                return list;

            if (reader.TokenType == JsonTokenType.Null
                || reader.TokenType != JsonTokenType.StartObject)
            {
                // Skip unexpected tokens (null elements, strings, numbers, booleans)
                reader.Skip();
                continue;
            }

            var item = JsonSerializer.Deserialize<T>(ref reader, options);
            if (item != null)
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
}
