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
    private readonly bool _disposeGraphqlHttpClient;
    private readonly OAuthTokenProvider? _ownedTokenProvider;

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

        // Single token provider shared by both HTTP clients — one token request on cold start
        var tokenProvider = new OAuthTokenProvider(options);
        _ownedTokenProvider = tokenProvider;

        // Create OAuth handler
        var oauthHandler = new OAuthHandler(tokenProvider)
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

        // GraphQL client reuses the same token provider — no second token request
        var graphqlOAuthHandler = new OAuthHandler(tokenProvider) { InnerHandler = new HttpClientHandler() };
        _graphqlHttpClient = new HttpClient(graphqlOAuthHandler)
        {
            BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/graphql"),
            Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds)
        };
        _disposeGraphqlHttpClient = true;
        GraphQL = new RosettaGraphQLClient(_graphqlHttpClient);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RosettaClient"/> class with pre-configured HttpClients.
    /// Use this constructor when integrating with <see cref="IHttpClientFactory"/> in dependency injection scenarios
    /// so that both the REST and GraphQL clients benefit from pooled handler management.
    /// </summary>
    /// <param name="httpClient">Pre-configured HttpClient for REST calls (should include OAuth handler via IHttpClientFactory).</param>
    /// <param name="graphqlHttpClient">Pre-configured HttpClient for GraphQL calls, with <c>BaseAddress</c> pointing at the <c>/graphql</c> endpoint.</param>
    /// <param name="options">Configuration options for the Rosetta API client.</param>
    public RosettaClient(HttpClient httpClient, HttpClient graphqlHttpClient, RosettaClientOptions options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _graphqlHttpClient = graphqlHttpClient ?? throw new ArgumentNullException(nameof(graphqlHttpClient));

        if (options == null)
            throw new ArgumentNullException(nameof(options));

        options.Validate();

        var baseUrl = options.BaseUrl.Replace("{version}", options.ApiVersion);
        Api = new Generated.Client(_httpClient)
        {
            BaseUrl = baseUrl
        };

        _disposeHttpClient = false;        // IHttpClientFactory owns the REST client
        _disposeGraphqlHttpClient = false; // IHttpClientFactory owns the GraphQL client
        GraphQL = new RosettaGraphQLClient(_graphqlHttpClient);
    }

    /// <summary>
    /// Disposes the RosettaClient and releases resources.
    /// </summary>
    public void Dispose()
    {
        if (_disposeHttpClient)
            _httpClient?.Dispose();

        if (_disposeGraphqlHttpClient)
            _graphqlHttpClient?.Dispose();

        _ownedTokenProvider?.Dispose();
    }
}
