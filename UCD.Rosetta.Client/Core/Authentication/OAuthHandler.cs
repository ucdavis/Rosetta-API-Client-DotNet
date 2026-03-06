using System.Net.Http.Headers;
using UCD.Rosetta.Client.Core.Configuration;

namespace UCD.Rosetta.Client.Core.Authentication;

/// <summary>
/// DelegatingHandler that automatically handles OAuth 2.0 client credentials authentication
/// for Rosetta API requests. Token acquisition and caching are delegated to an
/// <see cref="OAuthTokenProvider"/> instance, which may be shared across multiple handler
/// instances (e.g. REST and GraphQL clients) to avoid redundant token requests.
/// </summary>
public class OAuthHandler : DelegatingHandler
{
    private readonly OAuthTokenProvider _tokenProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="OAuthHandler"/> class with its own
    /// private <see cref="OAuthTokenProvider"/>. Use this overload when a single handler
    /// is sufficient and token-cache sharing is not required.
    /// </summary>
    /// <param name="options">The Rosetta client configuration options.</param>
    public OAuthHandler(RosettaClientOptions options)
        : this(new OAuthTokenProvider(options)) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="OAuthHandler"/> class with a shared
    /// <see cref="OAuthTokenProvider"/>. Use this overload when multiple handler instances
    /// (e.g. REST and GraphQL) should share a single token cache so that only one token
    /// request is made on cold start.
    /// </summary>
    /// <param name="tokenProvider">Shared token provider.</param>
    public OAuthHandler(OAuthTokenProvider tokenProvider)
    {
        _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
    }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = await _tokenProvider.GetAccessTokenAsync(cancellationToken);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await base.SendAsync(request, cancellationToken);
    }
}
