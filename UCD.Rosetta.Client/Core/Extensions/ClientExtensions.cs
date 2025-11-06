using System.Text.Json;
using UCD.Rosetta.Client.Core.Converters;

namespace UCD.Rosetta.Client.Generated;

/// <summary>
/// Partial class extensions for custom behavior
/// </summary>
public partial class Client
{
    static partial void UpdateJsonSerializerSettings(JsonSerializerOptions settings)
    {
        // Add any custom JSON serialization settings here
        settings.PropertyNameCaseInsensitive = true;
        
        // Add custom converter for dynamic object collections
        settings.Converters.Add(new DynamicObjectCollectionConverter());
    }
}
