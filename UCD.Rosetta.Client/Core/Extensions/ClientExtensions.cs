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
        if (DebugResponseMaxLength != 0 && response.Content != null)
        {
            // Read the response body - we need to buffer it so it can be read again
            var responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            
            // Re-create the content with the buffered string so it can be read again during deserialization
            var newContent = new System.Net.Http.StringContent(responseBody, 
                System.Text.Encoding.UTF8, 
                response.Content.Headers.ContentType?.MediaType ?? "application/json");
            
            // Copy headers from original content
            foreach (var header in response.Content.Headers)
            {
                newContent.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
            
            response.Content = newContent;
            
            // Log response details to console
            Console.WriteLine("=== DEBUG: API Response ===");
            Console.WriteLine($"Request: {response.RequestMessage?.Method} {response.RequestMessage?.RequestUri?.PathAndQuery}");
            Console.WriteLine($"Status: {(int)response.StatusCode} {response.StatusCode}");
            Console.WriteLine($"Content-Type: {response.Content.Headers.ContentType}");
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
