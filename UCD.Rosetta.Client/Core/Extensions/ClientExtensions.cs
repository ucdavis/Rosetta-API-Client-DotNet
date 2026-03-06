using System.Diagnostics;
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

    partial void ProcessResponse(HttpClient client, HttpResponseMessage response)
    {
        if (DebugResponseMaxLength != 0 && response.Content != null)
        {
            // Read the response body - we need to buffer it so it can be read again
            var responseBody = response.Content.ReadAsStringAsync()
                .ConfigureAwait(false) // Avoid deadlocks in certain synchronization contexts
                .GetAwaiter()
                .GetResult();
            
            // Re-create the content with the buffered string so it can be read again during deserialization
            var newContent = new StringContent(responseBody, 
                System.Text.Encoding.UTF8, 
                response.Content.Headers.ContentType?.MediaType ?? "application/json");
            
            // Copy headers from original content
            foreach (var header in response.Content.Headers)
            {
                newContent.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
            
            response.Content = newContent;

            var body = DebugResponseMaxLength == -1 || responseBody.Length <= DebugResponseMaxLength
                ? responseBody
                : responseBody.Substring(0, DebugResponseMaxLength) + $"... (truncated, showing {DebugResponseMaxLength} of {responseBody.Length} chars)";

            var lines = new[]
            {
                "=== DEBUG: API Response ===",
                $"Request: {response.RequestMessage?.Method} {response.RequestMessage?.RequestUri?.PathAndQuery}",
                $"Status: {(int)response.StatusCode} {response.StatusCode}",
                $"Content-Type: {response.Content.Headers.ContentType}",
                $"Body Length: {responseBody.Length} characters",
                "Body:",
                body,
                "=========================\n"
            };

            // Write to Trace — visible in VS/VS Code Output > Debug window when debugging
            foreach (var line in lines)
                Trace.WriteLine(line);
        }
    }
    
    private static string? FindSolutionRoot()
    {
        try
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null)
            {
                if (dir.GetFiles("*.sln").Length > 0)
                    return dir.FullName;
                dir = dir.Parent;
            }
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"[RosettaClient] FindSolutionRoot failed: {ex.Message}");
        }
        return null;
    }

    static partial void UpdateJsonSerializerSettings(JsonSerializerOptions settings)
    {
        // Add any custom JSON serialization settings here
        settings.PropertyNameCaseInsensitive = true;
        
        // Gracefully handle ICollection<T> arrays that may contain null or unexpected
        // token types for some records in real API responses (e.g. student_association).
        settings.Converters.Add(new LenientTypedCollectionConverterFactory());
    }
}
