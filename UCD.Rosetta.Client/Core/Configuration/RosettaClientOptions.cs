namespace UCD.Rosetta.Client.Core.Configuration;

/// <summary>
/// Configuration options for the Rosetta API client.
/// </summary>
public class RosettaClientOptions
{
    /// <summary>
    /// The base URL for the Rosetta API.
    /// Example: https://iam-rosetta-dev-2y5rmy.jrfxkn.usa-w2.cloudhub.io/api/v1
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// The OAuth 2.0 token endpoint URL for obtaining access tokens.
    /// </summary>
    public string TokenUrl { get; set; } = string.Empty;

    /// <summary>
    /// The OAuth 2.0 client ID for authentication.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// The OAuth 2.0 client secret for authentication.
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// Optional: The API version to use (default is "v1").
    /// This will be used to replace {version} in the BaseUri template.
    /// </summary>
    public string ApiVersion { get; set; } = "v1";

    /// <summary>
    /// Optional: Timeout for HTTP requests in seconds (default is 30 seconds).
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Validates that all required configuration values are provided.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when required configuration is missing.</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(BaseUrl))
            throw new InvalidOperationException("RosettaClientOptions.BaseUrl is required.");

        if (string.IsNullOrWhiteSpace(TokenUrl))
            throw new InvalidOperationException("RosettaClientOptions.TokenUrl is required.");

        if (string.IsNullOrWhiteSpace(ClientId))
            throw new InvalidOperationException("RosettaClientOptions.ClientId is required.");

        if (string.IsNullOrWhiteSpace(ClientSecret))
            throw new InvalidOperationException("RosettaClientOptions.ClientSecret is required.");
    }
}
