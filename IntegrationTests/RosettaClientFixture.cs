using Microsoft.Extensions.Configuration;
using UCD.Rosetta.Client.Core;
using UCD.Rosetta.Client.Core.Configuration;
using DotNetEnv;

namespace IntegrationTests;

/// <summary>
/// Test fixture for integration tests that provides a configured RosettaClient.
/// Loads configuration from appsettings.json and .env file.
/// </summary>
public class RosettaClientFixture : IDisposable
{
    public RosettaClient Client { get; }
    public RosettaClientOptions Options { get; }
    public TestDataOptions TestData { get; }

    public RosettaClientFixture()
    {
        // Load .env file from the repository root
        var envPath = Path.Combine(Directory.GetCurrentDirectory(), "../../../..", ".env");
        if (File.Exists(envPath))
        {
            Env.Load(envPath);
        }

        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        Options = new RosettaClientOptions();
        configuration.GetSection("RosettaClient").Bind(Options);

        TestData = new TestDataOptions();
        configuration.GetSection("TestData").Bind(TestData);

        // Create the client
        Client = new RosettaClient(Options);

        // Configure debug logging if enabled
        if (TestData.EnableDebugLogging)
        {
            Client.DebugResponseMaxLength = TestData.DebugResponseMaxLength;
            Console.WriteLine($"[Debug] Response logging enabled (max length: {(TestData.DebugResponseMaxLength == -1 ? "unlimited" : TestData.DebugResponseMaxLength.ToString())})");
        }
    }

    public void Dispose()
    {
        Client?.Dispose();
    }
}
