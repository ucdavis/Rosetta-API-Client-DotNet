using UCD.Rosetta.Client.Core.Authentication;
using UCD.Rosetta.Client.Core.Configuration;
using UCD.Rosetta.Client.Generated;

namespace UCD.Rosetta.Client.Core;

/// <summary>
/// Main client for interacting with the UC Davis IAM Rosetta API.
/// Provides automatic OAuth authentication and exposes the generated API client.
/// </summary>
public class RosettaClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly bool _disposeHttpClient;

    /// <summary>
    /// Gets the underlying API client that provides access to all Rosetta API endpoints.
    /// Use this property to call any API endpoint, for example:
    /// <code>
    /// var identities = await rosettaClient.Api.IdentitiesAsync();
    /// var accounts = await rosettaClient.Api.AccountsAllAsync(iamid: "1234567890");
    /// var person = await rosettaClient.Api.PeopleAsync("1234567890");
    /// </code>
    /// </summary>
    public IClient Api { get; }
    
    /// <summary>
    /// Gets or sets the maximum number of characters to log from API responses.
    /// Set to 0 to disable logging (default), -1 for unlimited, or a positive number to limit output.
    /// Useful for troubleshooting API issues and seeing actual response data.
    /// </summary>
    public int DebugResponseMaxLength
    {
        get => ((Generated.Client)Api).DebugResponseMaxLength;
        set => ((Generated.Client)Api).DebugResponseMaxLength = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RosettaClient"/> class.
    /// Creates a new HttpClient with OAuth authentication configured automatically.
    /// </summary>
    /// <param name="options">Configuration options for the Rosetta API client.</param>
    public RosettaClient(RosettaClientOptions options)
    {
        if (options == null)
            throw new ArgumentNullException(nameof(options));

        options.Validate();

        // Create OAuth handler
        var oauthHandler = new OAuthHandler(options)
        {
            InnerHandler = new HttpClientHandler()
        };

        // Create HttpClient with OAuth handler
        _httpClient = new HttpClient(oauthHandler)
        {
            Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds)
        };

        // Create generated client and set base URL (replace {version} placeholder)
        var baseUrl = options.BaseUrl.Replace("{version}", options.ApiVersion);
        Api = new Generated.Client(_httpClient)
        {
            BaseUrl = baseUrl
        };
        _disposeHttpClient = true;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RosettaClient"/> class with a pre-configured HttpClient.
    /// Use this constructor when integrating with IHttpClientFactory in dependency injection scenarios.
    /// </summary>
    /// <param name="httpClient">Pre-configured HttpClient (should include OAuth handler via IHttpClientFactory).</param>
    /// <param name="options">Configuration options for the Rosetta API client.</param>
    public RosettaClient(HttpClient httpClient, RosettaClientOptions options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        
        if (options == null)
            throw new ArgumentNullException(nameof(options));

        options.Validate();

        // Create generated client and set base URL
        var baseUrl = options.BaseUrl.Replace("{version}", options.ApiVersion);
        Api = new Generated.Client(_httpClient)
        {
            BaseUrl = baseUrl
        };
        _disposeHttpClient = false; // Don't dispose HttpClient when using IHttpClientFactory
    }

    /// <summary>
    /// Disposes the RosettaClient and releases resources.
    /// </summary>
    public void Dispose()
    {
        if (_disposeHttpClient)
        {
            _httpClient?.Dispose();
        }
    }
}
