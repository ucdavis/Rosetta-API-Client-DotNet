# UCD Rosetta Client Integration Tests

Integration tests for the UCD.Rosetta.Client library that verify functionality against the actual Rosetta API.

## Prerequisites

- Valid Rosetta API credentials (ClientId and ClientSecret)
- Network access to the Rosetta API endpoints

## Configuration

The tests load configuration from multiple sources (in priority order):

1. **Default values** - `appsettings.json`
2. **User secrets** - Local development (shared with example project)
3. **Environment variables** - CI/CD pipelines (highest priority)

### Option 1: User Secrets (Local Development)

The test project shares the same UserSecretsId with the example project, so you only need to configure once:

```bash
# Required: API credentials
dotnet user-secrets set "RosettaClient:ClientId" "your-client-id" --project Example
dotnet user-secrets set "RosettaClient:ClientSecret" "your-client-secret" --project Example

# Optional: Test data for specific ID-based tests
dotnet user-secrets set "TestData:IamId" "1234567890" --project Example
dotnet user-secrets set "TestData:IamIds" "1234567890,0987654321" --project Example
dotnet user-secrets set "TestData:AccountId" "account-123" --project Example
dotnet user-secrets set "TestData:EmployeeId" "123456" --project Example
dotnet user-secrets set "TestData:StudentId" "987654" --project Example
dotnet user-secrets set "TestData:TestEmail" "someone@ucdavis.edu" --project Example
```

Without test data configured, tests that require specific IDs will be automatically skipped.

### Option 2: Environment Variables (CI/CD)

For CI/CD pipelines or Docker environments, use environment variables with the `ROSETTA_` prefix:

```bash
# Required: API credentials
export ROSETTA_RosettaClient__ClientId="your-client-id"
export ROSETTA_RosettaClient__ClientSecret="your-client-secret"

# Optional: Test data
export ROSETTA_TestData__IamId="1234567890"
export ROSETTA_TestData__IamIds="1234567890,0987654321"
export ROSETTA_TestData__AccountId="account-123"
export ROSETTA_TestData__EmployeeId="123456"
export ROSETTA_TestData__StudentId="987654"
export ROSETTA_TestData__TestEmail="someone@ucdavis.edu"
```

**Note:** Use double underscores (`__`) to represent nested configuration sections in environment variables.

## Running Tests

Run all tests:
```bash
dotnet test
```

Run tests from the integration test project only:
```bash
dotnet test IntegrationTests
```

Run with verbose output:
```bash
dotnet test --verbosity normal
```

## Test Organization

- **RosettaClientFixture** - Shared fixture that creates and configures the RosettaClient and loads test data for all tests
- **TestDataOptions** - Configuration class for optional test data (IDs, emails, etc.)
- **RosettaApiTests** - Integration tests for various API endpoints organized by category:
  - Authentication & User (1 test)
  - Identities (2 tests)
  - People (4 tests - 2 require TestData:IamId)
  - Accounts (3 tests - all require test data)
  - Employees (2 tests - 1 requires TestData:EmployeeId)
  - Students (2 tests - 1 requires TestData:StudentId)
  - Reference Data (9 tests)
  - Campaign Contacts (2 tests - manually skipped, file downloads)

### Test Behavior

Tests that require specific identifying data (IAM IDs, account IDs, etc.) will:
- **Automatically skip** if the corresponding `TestData` configuration value is not set
- **Run normally** when test data is provided via user secrets or environment variables

This allows the test suite to run in environments without test data while still providing comprehensive coverage when test data is available.

### Test Examples

The test suite includes tests for:
- ✅ `MeAsync` - Get current authenticated user (always runs)
- ✅ `IdentitiesAsync` - Get identities modified in last 24 hours (always runs)
- ✅ `PeopleAllAsync` - Search for people by email (uses TestData:TestEmail or default)
- ✅ `GroupsAsync` - Get all groups (always runs)
- ⏭️ `PeopleAsync` - Get specific person (skipped unless TestData:IamId is configured)
- ⏭️ `AccountsAllAsync` - Get accounts (skipped unless TestData:IamId is configured)

## Notes

- Tests require valid credentials - they will fail if credentials are not configured
- Tests that need specific IDs are automatically skipped when test data is not provided
- Integration tests make real API calls and may be affected by API rate limits or service availability
- Some tests (Campaign Contacts CSV downloads) are manually skipped and should be tested interactively
