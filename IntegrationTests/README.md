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
dotnet user-secrets set "RosettaClient:ClientId" "your-client-id" --project Example
dotnet user-secrets set "RosettaClient:ClientSecret" "your-client-secret" --project Example
```

### Option 2: Environment Variables (CI/CD)

For CI/CD pipelines or Docker environments, use environment variables with the `ROSETTA_` prefix:

```bash
export ROSETTA_RosettaClient__ClientId="your-client-id"
export ROSETTA_RosettaClient__ClientSecret="your-client-secret"
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

- **RosettaClientFixture** - Shared fixture that creates and configures the RosettaClient for all tests
- **RosettaApiTests** - Integration tests for various API endpoints

### Test Examples

The test suite includes tests for:
- ✅ `MeAsync` - Get current authenticated user
- ✅ `IdentitiesAsync` - Get identities modified in last 24 hours
- ✅ `PeopleAllAsync` - Search for people by email
- ✅ `GroupsAsync` - Get all groups
- ⏭️ `AccountsAllAsync` - Get accounts (skipped by default, requires specific IAM ID)

## Notes

- Some tests are skipped by default (marked with `[Fact(Skip = "reason")]`) and require specific test data
- Tests require valid credentials - they will fail if credentials are not configured
- Integration tests make real API calls and may be affected by API rate limits or service availability
