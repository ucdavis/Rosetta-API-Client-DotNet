using System.Text.Json;
using UCD.Rosetta.Client.Core.Converters;

namespace UCD.Rosetta.Client.Generated;

/// <summary>
/// Partial class extensions for custom behavior
/// </summary>
public partial class Client
{
    /// <summary>
    /// Gets or sets the maximum number of characters to log from API responses.
    /// Set to 0 to disable logging, -1 for unlimited, or a positive number to limit output.
    /// Default is 0 (disabled).
    /// </summary>
    public int DebugResponseMaxLength { get; set; } = 0;

    partial void ProcessResponse(System.Net.Http.HttpClient client, System.Net.Http.HttpResponseMessage response)
    {
        if (DebugResponseMaxLength != 0)
        {
            // Log response details to console
            Console.WriteLine("=== DEBUG: API Response ===");
            Console.WriteLine($"Status: {(int)response.StatusCode} {response.StatusCode}");
            Console.WriteLine($"Content-Type: {response.Content.Headers.ContentType}");
            
            // Read and log the response body
            var responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            Console.WriteLine($"Body Length: {responseBody.Length} characters");
            Console.WriteLine("Body:");
            
            if (DebugResponseMaxLength == -1 || responseBody.Length <= DebugResponseMaxLength)
            {
                // Show full response
                Console.WriteLine(responseBody);
            }
            else
            {
                // Truncate to specified length
                Console.WriteLine(responseBody.Substring(0, DebugResponseMaxLength) + $"... (truncated, showing {DebugResponseMaxLength} of {responseBody.Length} chars)");
            }
            
            Console.WriteLine("=========================\n");
        }
    }
    
    static partial void UpdateJsonSerializerSettings(JsonSerializerOptions settings)
    {
        // Add any custom JSON serialization settings here
        settings.PropertyNameCaseInsensitive = true;
        
        // Add custom converter for dynamic object collections
        settings.Converters.Add(new DynamicObjectCollectionConverter());
    }
}
