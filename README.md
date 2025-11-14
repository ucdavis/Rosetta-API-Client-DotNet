# UCD.Rosetta.Client

![Rosetta API Spec](https://img.shields.io/badge/Rosetta%20API%20Spec-v1.0.17-blue)

Official .NET client library for the UC Davis IAM Rosetta API. Provides easy access to identity and access management data from UC Davis IAM services.

## Features

- ✅ **Automatic OAuth 2.0 Authentication** - Client credentials flow handled automatically
- ✅ **Strongly-Typed API** - Full IntelliSense support with auto-generated models
- ✅ **Modern .NET 8.0** - Built on the latest .NET with nullable reference types
- ✅ **Dependency Injection Ready** - First-class support for ASP.NET Core DI
- ✅ **IHttpClientFactory Support** - Proper HttpClient lifecycle management
- ✅ **Auto-Generated from OpenAPI** - Stays in sync with API specification
- ✅ **System.Text.Json** - Uses modern, high-performance JSON serialization
- ✅ **Async/Await** - Fully asynchronous API with cancellation token support

## Installation

Install via NuGet Package Manager:

```bash
dotnet add package UCD.Rosetta.Client
```

Or via Package Manager Console:

```powershell
Install-Package UCD.Rosetta.Client
```

## Quick Start

### Basic Usage

```csharp
using UCD.Rosetta.Client.Core;
using UCD.Rosetta.Client.Core.Configuration;

// Configure the client
var options = new RosettaClientOptions
{
    BaseUrl = "https://iam-rosetta-dev-2y5rmy.jrfxkn.usa-w2.cloudhub.io/api/{version}",
    TokenUrl = "https://your-oauth-server.com/oauth/token",
    ClientId = "your-client-id",
    ClientSecret = "your-client-secret",
    ApiVersion = "v1"
};

// Create and use the client
using var client = new RosettaClient(options);

// Get current user information
var me = await client.Api.MeAsync();

// Search for people by email
var people = await client.Api.PeopleAllAsync(email: "someone@ucdavis.edu");

// Get accounts for a specific IAM ID
var accounts = await client.Api.AccountsAllAsync(iamid: "1234567890");

// Get identities modified in the last 24 hours
var identities = await client.Api.IdentitiesAsync(limit: 100);
```

### ASP.NET Core Dependency Injection

#### Option 1: Simple Registration (Recommended for most scenarios)

```csharp
// Program.cs or Startup.cs
using UCD.Rosetta.Client.Core.Extensions;

builder.Services.AddRosettaClient(options =>
{
    options.BaseUrl = builder.Configuration["Rosetta:BaseUrl"];
    options.TokenUrl = builder.Configuration["Rosetta:TokenUrl"];
    options.ClientId = builder.Configuration["Rosetta:ClientId"];
    options.ClientSecret = builder.Configuration["Rosetta:ClientSecret"];
    options.ApiVersion = "v1";
});

// In your controller or service
public class MyService
{
    private readonly RosettaClient _rosettaClient;

    public MyService(RosettaClient rosettaClient)
    {
        _rosettaClient = rosettaClient;
    }

    public async Task<object> GetUserAccountsAsync(string iamId)
    {
        return await _rosettaClient.Api.AccountsAllAsync(iamid: iamId);
    }
}
```

#### Option 2: With IHttpClientFactory (Recommended for high-throughput applications)

```csharp
// Program.cs or Startup.cs
builder.Services.AddRosettaClientWithFactory(options =>
{
    options.BaseUrl = builder.Configuration["Rosetta:BaseUrl"];
    options.TokenUrl = builder.Configuration["Rosetta:TokenUrl"];
    options.ClientId = builder.Configuration["Rosetta:ClientId"];
    options.ClientSecret = builder.Configuration["Rosetta:ClientSecret"];
    options.ApiVersion = "v1";
});
```

### Configuration with appsettings.json

```json
{
  "Rosetta": {
    "BaseUrl": "https://iam-rosetta-dev-2y5rmy.jrfxkn.usa-w2.cloudhub.io/api/{version}",
    "TokenUrl": "https://your-oauth-server.com/oauth/token",
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret",
    "ApiVersion": "v1",
    "TimeoutSeconds": 30
  }
}
```

**⚠️ Security Warning:** Never commit secrets to source control. Use:
- User Secrets for local development: `dotnet user-secrets set "Rosetta:ClientSecret" "your-secret"`
- Azure Key Vault or similar for production
- Environment variables for containerized deployments

## Available API Endpoints

The client provides access to all Rosetta API endpoints through the `client.Api` property:

### Identity & People

```csharp
// Get identities modified in last 24 hours
await client.Api.IdentitiesAsync(limit: 100);

// Search for people
await client.Api.PeopleAllAsync(
    email: "user@ucdavis.edu",
    iamIds: "1234567890,0987654321",
    userId: "kerberos-id",
    employeeId: "123456",
    studentId: "987654",
    limit: 50
);

// Get specific person by IAM ID
await client.Api.PeopleAsync("1234567890");

// Get current authenticated user
await client.Api.MeAsync();
```

### Accounts

```csharp
// Get all accounts for an IAM ID
await client.Api.AccountsAllAsync(iamid: "1234567890");

// Get accounts for multiple IAM IDs
await client.Api.AccountsAllAsync(iamids: "1234567890,0987654321");

// Get specific account by ID
await client.Api.AccountsAsync("account-id");

// Get base profiles
await client.Api.BaseprofilesAsync();

// Get employment status
await client.Api.EmploymentstatusAsync();

// Get UC Path entitlements
await client.Api.UcpathentitlementsAsync();

// Get student associations
await client.Api.StudentassociationsAsync();
```

### Employees & Students

```csharp
// Get all employees
await client.Api.EmployeesAllAsync(
    email: "employee@ucdavis.edu",
    employeeId: "123456",
    limit: 50
);

// Get specific employee
await client.Api.EmployeesAsync("1234567890");

// Get all students
await client.Api.StudentsAllAsync(
    email: "student@ucdavis.edu",
    studentId: "987654",
    limit: 50
);

// Get specific student
await client.Api.StudentsAsync("1234567890");
```

### Reference Data

```csharp
// Get all groups
await client.Api.GroupsAsync();

// Get all organizations
await client.Api.OrganizationsAsync();

// Get all roles
await client.Api.RolesAsync();

// Get colleges
await client.Api.CollegesAsync();

// Get majors
await client.Api.MajorsAsync();
```

### Campaign Contacts (CSV Export)

```csharp
// Get campaign contacts as CSV
var csvFile = await client.Api.CampaignContactsAsync(limit: 1000, save: true);
```

## Error Handling

The client throws `RosettaApiException` for API errors:

```csharp
using UCD.Rosetta.Client.Generated;

try
{
    var person = await client.Api.PeopleAsync("invalid-id");
}
catch (RosettaApiException ex)
{
    Console.WriteLine($"API Error: {ex.StatusCode} - {ex.Message}");
    Console.WriteLine($"Response: {ex.Response}");
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"Network Error: {ex.Message}");
}
```

## Authentication

The client automatically handles OAuth 2.0 client credentials authentication:

1. **Token Acquisition** - Automatically requests access token on first API call
2. **Token Caching** - Caches tokens for their lifetime (typically 24 hours)
3. **Automatic Refresh** - Refreshes expired tokens automatically
4. **Thread-Safe** - Token acquisition is thread-safe for concurrent requests

The OAuth handler adds a 5-minute buffer before token expiration to ensure reliability.

## Advanced Configuration

### Custom Timeout

```csharp
var options = new RosettaClientOptions
{
    // ... other options
    TimeoutSeconds = 60 // Default is 30 seconds
};
```

### Cancellation Tokens

All API methods support cancellation:

```csharp
var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

try
{
    var people = await client.Api.PeopleAllAsync(
        limit: 1000,
        cancellationToken: cts.Token
    );
}
catch (OperationCanceledException)
{
    Console.WriteLine("Request was cancelled");
}
```

## Requirements

- .NET 8.0 or later
- Valid Rosetta API credentials (Client ID and Client Secret)
- Network access to the Rosetta API endpoints

## Development

### Project Structure

```
UCD.Rosetta.Client/
├── Core/
│   ├── Authentication/
│   │   └── OAuthHandler.cs          # OAuth 2.0 authentication handler
│   ├── Configuration/
│   │   └── RosettaClientOptions.cs  # Configuration options
│   ├── Extensions/
│   │   └── ServiceCollectionExtensions.cs  # DI extensions
│   └── RosettaClient.cs             # Main client facade
├── Generated/
│   └── RosettaApiClient.g.cs        # Auto-generated from OpenAPI spec
└── specs/
    └── rosetta-api.json             # OpenAPI specification
```

### Regenerating Client Code

The client code is automatically regenerated from the OpenAPI specification during build. 

To update the spec to a new version, use the convenience script:
```bash
./update-spec.sh 1.0.12  # Replace with desired version
```

This will download the latest spec from MuleSoft Exchange and save it as `specs/rosetta-api.json`, then run:
```bash
dotnet clean && dotnet build
```

## Future Enhancements

- 🔄 GraphQL API support (planned by IAM team)
- 📊 Response caching options
- 🔄 Retry policies with Polly
- 📝 Enhanced logging and diagnostics

## Local development, secrets, and running tests

The repository includes a small example app and an xUnit integration test project. The example and tests load configuration from `appsettings.json` and then override sensitive values from user secrets or environment variables.

Quick steps to run the example locally:

1. Add your Rosetta credentials to user secrets for the example project (the example project uses UserSecretsId `db36e8a1-703d-48f0-af34-05f22ba84854`):

```bash
cd Example
dotnet user-secrets set "RosettaClient:ClientId" "<your-client-id>"
dotnet user-secrets set "RosettaClient:ClientSecret" "<your-client-secret>"
dotnet user-secrets set "RosettaClient:TokenUrl" "https://your-oauth-server.com/token"
```

2. Run the example:

```bash
dotnet run --project Example
```

Running integration tests:

- The integration test project `IntegrationTests` shares the same UserSecretsId as the example project so the commands above are sufficient for tests as well.
- Tests also read environment variables, which is useful for CI. The supported environment variable names mirror the configuration keys (for example: `RosettaClient__ClientId` and `RosettaClient__ClientSecret`).

Run tests locally with:

```bash
dotnet test IntegrationTests
```

Debugging API responses

- When diagnosing deserialization errors the generated client exposes a configurable debug hook that logs response metadata and a truncated response body. Set `DebugResponseMaxLength` on the `RosettaClient` wrapper to control output size:

    - `0` = disabled (default)
    - `-1` = unlimited (print full response)
    - positive integer = maximum number of characters to print

Example (in code):

```csharp
var client = new RosettaClient(options);
client.DebugResponseMaxLength = 4096; // print up to 4KiB of response body for debugging
```

Why this extra configuration exists

- The Rosetta API spec contains several endpoints typed as untyped objects (OpenAPI `type: object` or arrays of `object`). The generated client uses `System.Text.Json`. To handle runtime variations in responses (arrays, single objects, or plain strings) a small custom `JsonConverter` is registered at runtime which defensively parses these responses into an `ICollection<object>` safely. This keeps the library compatible with `System.Text.Json` (no Newtonsoft dependency) while being resilient to inconsistent server payloads.

CI notes

- For CI runs store secrets as protected pipeline variables or use your cloud provider's secret store and set the corresponding environment variables (`RosettaClient__ClientId`, `RosettaClient__ClientSecret`, etc.).
- Avoid committing secrets to the repository. `appsettings.json` contains non-sensitive defaults and is safe to commit.

## License

MIT License - See LICENSE file for details

## Support

For issues, questions, or contributions:
- **Issues**: [GitHub Issues](https://github.com/ucdavis/UCD.Rosetta.Client/issues)
- **Documentation**: [Rosetta API Docs](https://anypoint.mulesoft.com/exchange/portals/university-of-california-346/9b04bfa8-6eeb-4d85-b676-91db930f8411/iam-unified-api-dev/)
- **UC Davis IAM Team**: Contact your IAM representative for credentials and API access
