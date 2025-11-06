using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using UCD.Rosetta.Client.Core.Authentication;
using UCD.Rosetta.Client.Core.Configuration;

namespace UCD.Rosetta.Client.Core.Extensions;

/// <summary>
/// Extension methods for configuring Rosetta API client services in dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Rosetta API client to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure the Rosetta client options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRosettaClient(
        this IServiceCollection services,
        Action<RosettaClientOptions> configureOptions)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        if (configureOptions == null)
            throw new ArgumentNullException(nameof(configureOptions));

        // Register options
        services.Configure(configureOptions);

        // Register RosettaClient
        services.AddSingleton<RosettaClient>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<RosettaClientOptions>>().Value;
            return new RosettaClient(options);
        });

        return services;
    }

    /// <summary>
    /// Adds the Rosetta API client to the service collection using IHttpClientFactory.
    /// This is the recommended approach for ASP.NET Core applications as it provides
    /// better HttpClient lifecycle management.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure the Rosetta client options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRosettaClientWithFactory(
        this IServiceCollection services,
        Action<RosettaClientOptions> configureOptions)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        if (configureOptions == null)
            throw new ArgumentNullException(nameof(configureOptions));

        // Register options
        services.Configure(configureOptions);

        // Register named HttpClient with OAuth handler
        services.AddHttpClient<RosettaClient>("RosettaClient", (sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<RosettaClientOptions>>().Value;
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        })
        .AddHttpMessageHandler(sp =>
        {
            var options = sp.GetRequiredService<IOptions<RosettaClientOptions>>().Value;
            return new OAuthHandler(options);
        });

        // Register RosettaClient
        services.AddScoped<RosettaClient>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<RosettaClientOptions>>().Value;
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient("RosettaClient");
            return new RosettaClient(httpClient, options);
        });

        return services;
    }
}
