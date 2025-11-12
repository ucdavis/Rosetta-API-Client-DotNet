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
}
