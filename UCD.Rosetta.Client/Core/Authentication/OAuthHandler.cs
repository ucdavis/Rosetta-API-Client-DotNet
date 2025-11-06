using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using UCD.Rosetta.Client.Core.Configuration;

namespace UCD.Rosetta.Client.Core.Authentication;

/// <summary>
/// DelegatingHandler that automatically handles OAuth 2.0 client credentials authentication
/// for Rosetta API requests. Manages token acquisition, caching, and automatic refresh.
/// </summary>
public class OAuthHandler : DelegatingHandler
{
    private readonly RosettaClientOptions _options;
    private readonly SemaphoreSlim _tokenLock = new(1, 1);
    private string? _cachedToken;
    private DateTimeOffset _tokenExpiration = DateTimeOffset.MinValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="OAuthHandler"/> class.
    /// </summary>
    /// <param name="options">The Rosetta client configuration options.</param>
    public OAuthHandler(RosettaClientOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _options.Validate();
    }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Get valid access token (will acquire new one if needed)
        var token = await GetAccessTokenAsync(cancellationToken);

        // Add Bearer token to request
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, cancellationToken);
    }

    /// <summary>
    /// Gets a valid access token, either from cache or by requesting a new one.
    /// </summary>
    private async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        // Check if we have a valid cached token
        if (!string.IsNullOrEmpty(_cachedToken) && DateTimeOffset.UtcNow < _tokenExpiration)
        {
            return _cachedToken;
        }

        // Use semaphore to ensure only one token request at a time
        await _tokenLock.WaitAsync(cancellationToken);
        try
        {
            // Double-check after acquiring lock
            if (!string.IsNullOrEmpty(_cachedToken) && DateTimeOffset.UtcNow < _tokenExpiration)
            {
                return _cachedToken;
            }

            // Request new token
            return await RequestNewTokenAsync(cancellationToken);
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    /// <summary>
    /// Requests a new OAuth 2.0 access token using client credentials flow.
    /// </summary>
    private async Task<string> RequestNewTokenAsync(CancellationToken cancellationToken)
    {
        using var tokenClient = new HttpClient();

        // Prepare client credentials as Basic auth
        var credentials = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{_options.ClientId}:{_options.ClientSecret}"));

        // Build token request (matching Python example)
        var request = new HttpRequestMessage(HttpMethod.Post, _options.TokenUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        request.Content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "client_credentials")
        });

        var response = await tokenClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"Failed to obtain OAuth token. Status: {response.StatusCode}, Error: {errorContent}");
        }

        var tokenResponse = await response.Content.ReadAsStringAsync(cancellationToken);
        var tokenData = JsonSerializer.Deserialize<TokenResponse>(tokenResponse);

        if (string.IsNullOrEmpty(tokenData?.AccessToken))
        {
            throw new InvalidOperationException("OAuth token response did not contain an access_token.");
        }

        // Cache the token (default is 24 hours per Python example, but we'll use expires_in if provided)
        _cachedToken = tokenData.AccessToken;
        
        // Set expiration with 5 minute buffer to ensure we refresh before it actually expires
        var expiresInSeconds = tokenData.ExpiresIn ?? 86400; // Default to 24 hours if not specified
        _tokenExpiration = DateTimeOffset.UtcNow.AddSeconds(expiresInSeconds - 300);

        return _cachedToken;
    }

    /// <summary>
    /// Represents the OAuth 2.0 token response.
    /// </summary>
    private class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("expires_in")]
        public int? ExpiresIn { get; set; }

        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _tokenLock.Dispose();
        }

        base.Dispose(disposing);
    }
}
