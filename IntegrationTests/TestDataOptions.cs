namespace IntegrationTests;

/// <summary>
/// Configuration for test data used in integration tests.
/// These values can be set via user secrets or environment variables.
/// </summary>
public class TestDataOptions
{
    /// <summary>
    /// IAM ID for testing individual person/account lookups.
    /// Example: "1234567890"
    /// </summary>
    public string? IamId { get; set; }

    /// <summary>
    /// Comma-separated list of IAM IDs for testing bulk operations.
    /// Example: "1234567890,0987654321"
    /// </summary>
    public string? IamIds { get; set; }

    /// <summary>
    /// Account ID for testing account lookups.
    /// Example: "account-123"
    /// </summary>
    public string? AccountId { get; set; }

    /// <summary>
    /// Employee ID for testing employee lookups.
    /// Example: "123456"
    /// </summary>
    public string? EmployeeId { get; set; }

    /// <summary>
    /// Student ID for testing student lookups.
    /// Example: "987654"
    /// </summary>
    public string? StudentId { get; set; }

    /// <summary>
    /// Email address for testing person searches.
    /// Example: "someone@ucdavis.edu"
    /// </summary>
    public string? TestEmail { get; set; }

    /// <summary>
    /// Enable debug logging of API response payloads during tests.
    /// When enabled, raw response data will be printed to console.
    /// Default: false
    /// 
    /// Set via user secrets: dotnet user-secrets set "TestData:EnableDebugLogging" "true" --project Example
    /// Set via env var: export ROSETTA_TestData__EnableDebugLogging=true
    /// </summary>
    public bool EnableDebugLogging { get; set; }

    /// <summary>
    /// Maximum length of response payload to log when debug logging is enabled.
    /// Use -1 for unlimited, 0 to disable, or a positive number to truncate.
    /// Default: 2000 characters
    /// </summary>
    public int DebugResponseMaxLength { get; set; } = 2000;
}
