using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UCD.Rosetta.Client.Core.Converters;

/// <summary>
/// Custom JSON converter for handling collections of dynamic objects returned by the API.
/// Handles cases where the API returns strings instead of arrays for untyped collection properties.
/// </summary>
public class DynamicObjectCollectionConverter : JsonConverter<ICollection<object>>
{
    /// <inheritdoc />
    public override ICollection<object>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        // Handle string responses - API bug where it returns strings instead of arrays
        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();
            // Return list with the string as a single element
            return string.IsNullOrWhiteSpace(stringValue) 
                ? new List<object>() 
                : new List<object> { stringValue };
        }

        // Handle single object responses - API returns object instead of array
        if (reader.TokenType == JsonTokenType.StartObject)
        {
            var element = JsonSerializer.Deserialize<JsonElement>(ref reader, options);
            return new List<object> { element };
        }

        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException($"Expected StartArray, StartObject, or String token, got {reader.TokenType}");
        }

        var list = new List<object>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
            {
                return list;
            }

            // Deserialize each element as a dynamic object (JsonElement)
            var element = JsonSerializer.Deserialize<JsonElement>(ref reader, options);
            list.Add(element);
        }

        throw new JsonException("Unexpected end of JSON array");
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, ICollection<object> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        
        foreach (var item in value)
        {
            JsonSerializer.Serialize(writer, item, options);
        }
        
        writer.WriteEndArray();
    }
}
