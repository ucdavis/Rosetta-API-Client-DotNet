using Microsoft.Extensions.Configuration;
using UCD.Rosetta.Client.Core;
using UCD.Rosetta.Client.Core.Configuration;
using DotNetEnv;
using UCD.Rosetta.Client.Generated;

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
    public TestDataOptions TestDataBig { get; }
    private readonly object _peopleSampleLock = new();
    private Lazy<Task<ICollection<Person>>> _peopleSample;

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

        TestDataBig = new TestDataOptions();
        configuration.GetSection("TestDataBig").Bind(TestDataBig);

        // Create the client
        Client = new RosettaClient(Options);
        _peopleSample = CreatePeopleSampleLazy();

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

    public async Task<ICollection<Person>> GetPeopleSampleAsync()
    {
        var peopleSample = _peopleSample;
        try
        {
            return await peopleSample.Value;
        }
        catch
        {
            ResetPeopleSample(peopleSample);
        }

        return await _peopleSample.Value;
    }

    private Lazy<Task<ICollection<Person>>> CreatePeopleSampleLazy()
    {
        return new Lazy<Task<ICollection<Person>>>(() =>
            Client.Api.PeopleGETAsync(limit: 25));
    }

    private void ResetPeopleSample(Lazy<Task<ICollection<Person>>> failedPeopleSample)
    {
        lock (_peopleSampleLock)
        {
            if (ReferenceEquals(_peopleSample, failedPeopleSample))
                _peopleSample = CreatePeopleSampleLazy();
        }
    }
}
