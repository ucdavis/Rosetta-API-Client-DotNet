using Microsoft.Extensions.Configuration;
using UCD.Rosetta.Client.Core;
using UCD.Rosetta.Client.Core.Configuration;

namespace IntegrationTests;

/// <summary>
/// Test fixture for integration tests that provides a configured RosettaClient.
/// Loads configuration from appsettings.json and user secrets.
/// </summary>
public class RosettaClientFixture : IDisposable
{
    public RosettaClient Client { get; }
    public RosettaClientOptions Options { get; }

    public RosettaClientFixture()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddUserSecrets(typeof(RosettaClientFixture).Assembly, optional: true)
            .AddEnvironmentVariables(prefix: "ROSETTA_")
            .Build();

        Options = new RosettaClientOptions();
        configuration.GetSection("RosettaClient").Bind(Options);

        // Create the client
        Client = new RosettaClient(Options);
    }

    public void Dispose()
    {
        Client?.Dispose();
    }
}
