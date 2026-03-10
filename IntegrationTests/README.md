# UCD Rosetta Client Integration Tests

Integration tests for the UCD.Rosetta.Client library that verify functionality against the actual Rosetta API.

## Prerequisites

- Valid Rosetta API credentials (ClientId and ClientSecret)
- Network access to the Rosetta API endpoints

## Configuration

The tests load configuration from multiple sources (in priority order):

1. **Default values** - `appsettings.json`
2. **.env file** - Local development (shared across projects)
3. **Environment variables** - CI/CD pipelines (highest priority)

### Option 1: .env File (Local Development)

Create a `.env` file in the repository root with your credentials. A `.env.example` template is provided:

```bash
# Copy the example file
cp .env.example .env

# Edit .env with your actual values
```

Your `.env` file should contain:

```bash
# Required: API credentials
RosettaClient__TokenUrl=https://your-token-url/token
RosettaClient__ClientId=your-client-id
RosettaClient__ClientSecret=your-client-secret
RosettaClient__BaseUrl=https://your-api-base-url/api/v1
RosettaClient__ApiVersion=v1
RosettaClient__Scope=read:public
RosettaClient__TimeoutSeconds=30

# Optional: Test data for specific ID-based tests
TestData__IamId=1234567890
TestData__IamIds=1234567890,0987654321
TestData__AccountId=account-123
TestData__EmployeeId=123456
TestData__StudentId=987654
TestData__TestEmail=someone@ucdavis.edu
TestData__TestDisplayName=Someone Name
TestData__LoginId=somelogin
TestData__ManagerIamId=1234567890
```

**Note:** The `.env` file is already in `.gitignore` and will not be committed to source control.

Without test data configured, tests that require specific IDs will be automatically skipped.

### Option 2: Environment Variables (CI/CD)

For CI/CD pipelines, Azure App Service, Docker, or other deployment environments, set environment variables directly:

```bash
# Required: API credentials
export RosettaClient__ClientId="your-client-id"
export RosettaClient__ClientSecret="your-client-secret"
export RosettaClient__TokenUrl="https://your-token-url/token"
export RosettaClient__BaseUrl="https://your-api-base-url/api/v1"

# Optional: Test data
export TestData__IamId="1234567890"
export TestData__IamIds="1234567890,0987654321"
export TestData__AccountId="account-123"
export TestData__EmployeeId="123456"
export TestData__StudentId="987654"
export TestData__TestEmail="someone@ucdavis.edu"
```

**Note:** Use double underscores (`__`) to represent nested configuration sections in environment variables. This format is consistent with Azure App Service configuration.

## Debug Logging

Enable debug logging to see raw API response payloads during test execution. This is useful for troubleshooting issues or understanding API responses.

When enabled, every API response will be logged to the console with:
- Request method and path (e.g., `GET /api/v1/me`)
- HTTP status code (e.g., `200 OK`)
- Content-Type header
- Response body length
- Response body content (full or truncated based on max length)

### Enable via .env File (Local Development)

Add these lines to your `.env` file:

```bash
TestData__EnableDebugLogging=true
TestData__DebugResponseMaxLength=4000  # Optional: default is 2000
```

### Enable via Environment Variables (CI/CD)

```bash
export TestData__EnableDebugLogging=true
export TestData__DebugResponseMaxLength=4000  # Optional
```

### Debug Response Max Length Options

- `0` - Disable response logging (same as EnableDebugLogging=false)
- `-1` - Unlimited (print full response, be careful with large responses!)
- `2000` - Default, print up to 2000 characters
- Any positive number - Truncate response to that many characters

### Example Output

When debug logging is enabled, you'll see output like this for each API call:

```
=== DEBUG: API Response ===
Request: GET /api/v1/me
Status: 200 OK
Content-Type: application/json; charset=utf-8
Body Length: 1523 characters
Body:
{"iamId":"1234567890","name":"John Doe","email":"johndoe@ucdavis.edu",...}
=========================
```

**Note:** Debug logging prints raw JSON responses to the console, which can be verbose. Only enable when actively debugging.

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
- **Run normally** when test data is provided via .env file or environment variables

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
