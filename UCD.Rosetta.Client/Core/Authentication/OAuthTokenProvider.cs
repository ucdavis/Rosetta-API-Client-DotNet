using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using UCD.Rosetta.Client.Core.Configuration;

namespace UCD.Rosetta.Client.Core.Authentication;

/// <summary>
/// Manages OAuth 2.0 token acquisition and caching for the Rosetta API.
/// Separating this from <see cref="OAuthHandler"/> allows a single token cache
/// to be shared across multiple handler instances (e.g. REST and GraphQL clients),
/// avoiding redundant token requests at cold start.
/// </summary>
public sealed class OAuthTokenProvider : IDisposable
{
    private readonly RosettaClientOptions _options;
    private readonly SemaphoreSlim _tokenLock = new(1, 1);
    private string? _cachedToken;
    private DateTimeOffset _tokenExpiration = DateTimeOffset.MinValue;

    /// <summary>
    /// Initializes a new instance of <see cref="OAuthTokenProvider"/>.
    /// </summary>
    /// <param name="options">The Rosetta client configuration options.</param>
    public OAuthTokenProvider(RosettaClientOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _options.Validate();
    }

    /// <summary>
    /// Returns a valid Bearer token, acquiring a new one from the token endpoint if
    /// the cached token is absent or within 5 minutes of expiry.
    /// </summary>
    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        // Fast path — valid cached token available
        if (!string.IsNullOrEmpty(_cachedToken) && DateTimeOffset.UtcNow < _tokenExpiration)
            return _cachedToken;

        await _tokenLock.WaitAsync(cancellationToken);
        try
        {
            // Re-check inside the lock (double-checked locking)
            if (!string.IsNullOrEmpty(_cachedToken) && DateTimeOffset.UtcNow < _tokenExpiration)
                return _cachedToken;

            return await RequestNewTokenAsync(cancellationToken);
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    private async Task<string> RequestNewTokenAsync(CancellationToken cancellationToken)
    {
        using var tokenClient = new HttpClient();

        var credentials = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{_options.ClientId}:{_options.ClientSecret}"));

        var request = new HttpRequestMessage(HttpMethod.Post, _options.TokenUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

        var formParams = new List<KeyValuePair<string, string>>
        {
            new("grant_type", "client_credentials")
        };
        if (!string.IsNullOrWhiteSpace(_options.Scope))
            formParams.Add(new("scope", _options.Scope));

        request.Content = new FormUrlEncodedContent(formParams);

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
            throw new InvalidOperationException("OAuth token response did not contain an access_token.");

        _cachedToken = tokenData.AccessToken;

        // Apply 5-minute buffer before actual expiry; default to 24 hours if not specified
        var expiresInSeconds = tokenData.ExpiresIn ?? 86400;
        _tokenExpiration = DateTimeOffset.UtcNow.AddSeconds(expiresInSeconds - 300);

        return _cachedToken;
    }

    /// <inheritdoc />
    public void Dispose() => _tokenLock.Dispose();

    private sealed class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("expires_in")]
        public int? ExpiresIn { get; set; }

        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }
    }
}
