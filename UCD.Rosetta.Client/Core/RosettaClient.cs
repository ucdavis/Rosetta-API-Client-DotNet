using UCD.Rosetta.Client.Core.Authentication;
using UCD.Rosetta.Client.Core.Configuration;
using UCD.Rosetta.Client.Generated;
using UCD.Rosetta.Client.GraphQL;

namespace UCD.Rosetta.Client.Core;

/// <summary>
/// Main client for interacting with the UC Davis IAM Rosetta API.
/// Provides automatic OAuth authentication and exposes the generated API client.
/// </summary>
public class RosettaClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly HttpClient _graphqlHttpClient;
    private readonly bool _disposeHttpClient;

    /// <summary>
    /// Gets the underlying API client that provides access to all Rosetta API endpoints.
    /// Use this property to call any API endpoint, for example:
    /// <code>
    /// var people = await rosettaClient.Api.PeopleAsync(iamid: "1234567890");
    /// var colleges = await rosettaClient.Api.CollegesAsync();
    /// var majors = await rosettaClient.Api.MajorsAsync(major_status: "A");
    /// var graphqlResult = await rosettaClient.Api.GraphqlAsync(new { query = "{ people(limit:10) { iam_id displayname } }" });
    /// </code>
    /// </summary>
    public IClient Api { get; }
    
    /// <summary>
    /// Gets the strongly-typed ZeroQL GraphQL client for querying the Rosetta GraphQL API.
    /// Provides compile-time-checked, LINQ-style queries over the <c>/graphql</c> endpoint.
    /// <code>
    /// var response = await rosettaClient.GraphQL.Query(
    ///     q => q.People(loginid: "jsmith", o => new { o.Iam_id, o.Displayname }));
    /// </code>
    /// </summary>
    public RosettaGraphQLClient GraphQL { get; }

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

        // Create a separate HttpClient for the GraphQL endpoint (owns its own OAuth handler)
        var graphqlOAuthHandler = new OAuthHandler(options) { InnerHandler = new HttpClientHandler() };
        _graphqlHttpClient = new HttpClient(graphqlOAuthHandler)
        {
            BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/graphql"),
            Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds)
        };
        GraphQL = new RosettaGraphQLClient(_graphqlHttpClient);
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

        // Create a separate HttpClient for the GraphQL endpoint (always owned by RosettaClient)
        var graphqlOAuthHandler = new OAuthHandler(options) { InnerHandler = new HttpClientHandler() };
        _graphqlHttpClient = new HttpClient(graphqlOAuthHandler)
        {
            BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/graphql"),
            Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds)
        };
        GraphQL = new RosettaGraphQLClient(_graphqlHttpClient);
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
        // _graphqlHttpClient is always owned by RosettaClient
        _graphqlHttpClient?.Dispose();
    }
}
