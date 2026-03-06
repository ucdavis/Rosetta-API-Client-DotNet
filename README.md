# UCD.Rosetta.Client

![Rosetta API Spec](https://img.shields.io/badge/Rosetta%20API%20Spec-v1.0.33-blue)

Official .NET client library for the UC Davis IAM Rosetta API. Provides easy access to identity and access management data from UC Davis IAM services.

## Features

- ✅ **Automatic OAuth 2.0 Authentication** - Client credentials flow handled automatically
- ✅ **Strongly-Typed REST API** - Full IntelliSense support with auto-generated models
- ✅ **Strongly-Typed GraphQL Client** - Compile-time-checked queries via [ZeroQL](https://github.com/byme8/ZeroQL/wiki); no raw query strings required
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

// Search for people by login ID
var people = await client.Api.PeopleAsync(loginid: "jsmith");

// Search for people by IAM ID
var person = await client.Api.PeopleAsync(iamid: "1234567890");

// Get all colleges
var colleges = await client.Api.CollegesAsync();

// Get active majors
var majors = await client.Api.MajorsAsync(major_status: "A");

// Strongly-typed GraphQL query (via ZeroQL)
var loginId = "jsmith";
var response = await client.GraphQL.Query(
    q => q.People(loginid: loginId,
        selector: o => new { o.Iam_id, o.Displayname, Email = o.Email(e => e.Primary) }));
var firstPerson = response.Data?[0];
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

    public async Task<ICollection<Person>> GetPersonByIamIdAsync(string iamId)
    {
        return await _rosettaClient.Api.PeopleAsync(iamid: iamId);
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

The client exposes two complementary surfaces: a REST API via `client.Api` and a strongly-typed GraphQL client via `client.GraphQL`.

### People

```csharp
// Search by a variety of identifiers
await client.Api.PeopleAsync(loginid: "jsmith");
await client.Api.PeopleAsync(iamid: "1234567890");
await client.Api.PeopleAsync(email: "user@ucdavis.edu");
await client.Api.PeopleAsync(employeeid: "123456");
await client.Api.PeopleAsync(studentid: "987654");
await client.Api.PeopleAsync(manager_iam_id: "0987654321");

// Pass a comma-separated list of IAM IDs
await client.Api.PeopleAsync(iamids: "1234567890,0987654321");
```

### Reference Data

```csharp
// Colleges
await client.Api.CollegesAsync();
await client.Api.CollegesAsync(college_code: "EN");

// Majors
await client.Api.MajorsAsync();
await client.Api.MajorsAsync(major_status: "A"); // active majors only
```

### GraphQL (Strongly-Typed)

The `client.GraphQL` property exposes a [ZeroQL](https://github.com/byme8/ZeroQL/wiki)-generated client. Queries are written as C# lambdas and validated at compile time — no raw query strings, full IntelliSense on every field.

```csharp
// People — select specific fields
var loginId = "jsmith";
var response = await client.GraphQL.Query(
    q => q.People(
        loginid: loginId,
        selector: o => new
        {
            o.Iam_id,
            o.Displayname,
            Name  = o.Name(n  => new { n.Lived_first_name, n.Lived_last_name }),
            Email = o.Email(e => e.Primary),
            Phone = o.Phone(p => p.Primary),
            Student = o.Student_association(s => new { s.College, s.Major, s.Class_level }),
            Payroll = o.Payroll_association(p => new { p.Position_title, p.Employee_classification })
        }));

foreach (var person in response.Data ?? [])
    Console.WriteLine($"{person.Iam_id?.Value}: {person.Displayname}");

// Colleges
var colleges = await client.GraphQL.Query(
    q => q.Colleges(selector: o => new { o.College_code, o.College_title }));

// Majors filtered by status
var majors = await client.GraphQL.Query(
    q => q.Majors(major_status: "A", selector: o => new { o.Major_code, o.Major_title }));
```

#### Query parameters (variables)

ZeroQL captures query arguments via lambda closure. The argument **must be a local variable or a method/constructor parameter** — it cannot be a property or field access. ZeroQL enforces this at compile time and will emit a build error if you pass a property directly.

```csharp
// ✅ Local variable — works
var loginId = _options.LoginId;
var response = await client.GraphQL.Query(
    q => q.People(loginid: loginId, selector: o => new { o.Iam_id, o.Displayname }));

// ❌ Property access — ZeroQL reports a compilation error
var response = await client.GraphQL.Query(
    q => q.People(loginid: _options.LoginId, selector: o => new { o.Iam_id, o.Displayname }));
```

For `static` lambdas (or to make variable capture explicit), use the two-argument overload that takes a `variables` object:

```csharp
var variables = new { LoginId = "jsmith" };
var response = await client.GraphQL.Query(
    variables,
    static (vars, q) => q.People(
        loginid: vars.LoginId,
        selector: o => new { o.Iam_id, o.Displayname }));
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
5. **Shared Token Cache** - The default constructor and `AddRosettaClientWithFactory` wire REST and GraphQL through a shared `OAuthTokenProvider`, so only one token request is made on cold start

Tokens receive a buffer of up to 5 minutes, or half their lifetime if shorter.

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
    var people = await client.Api.PeopleAsync(
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
<repo root>/
├── specs/                           # Downloaded and extracted by update-spec.sh
│   ├── rosetta-api.json             # OpenAPI specification
│   └── rosetta-api.graphql          # GraphQL SDL schema (with schema root appended)
├── update-spec.sh                   # Script to pull a new API spec version
└── UCD.Rosetta.Client/
    ├── Core/
    │   ├── Authentication/
    │   │   ├── OAuthHandler.cs          # Delegating handler that attaches Bearer tokens
    │   │   └── OAuthTokenProvider.cs    # Token acquisition, caching, and refresh logic
    │   ├── Configuration/
    │   │   └── RosettaClientOptions.cs  # Configuration options
    │   ├── Converters/
    │   │   └── LenientTypedCollectionConverter.cs
    │   ├── Extensions/
    │   │   ├── ClientExtensions.cs      # Debug logging
    │   │   └── ServiceCollectionExtensions.cs  # DI extensions
    │   └── RosettaClient.cs             # Main client facade (REST + GraphQL)
    ├── Generated/
    │   └── RosettaApiClient.g.cs        # Auto-generated from OpenAPI spec (NSwag)
    └── rosetta.zeroql.json              # ZeroQL codegen config
                                         # → generates obj/ZeroQL/rosetta.zeroql.json.g.cs on build
```

### Regenerating Client Code

The REST client (`RosettaApiClient.g.cs`) is regenerated from the OpenAPI spec by NSwag during Debug builds.

The GraphQL client (`obj/ZeroQL/rosetta.zeroql.json.g.cs`) is regenerated from `specs/rosetta-api.graphql` by ZeroQL on every build.

To update both specs to a new API version, use the convenience script:
```bash
./update-spec.sh 1.0.12  # Replace with desired version
```

To find the latest version number: open the [Rosetta API Exchange page](https://anypoint.mulesoft.com/exchange/portals/university-of-california-346/9b04bfa8-6eeb-4d85-b676-91db930f8411/iam-unified-api-dev/), open the **Download** dropdown, and hover over any link — the version appears in the URL shown in the browser status bar.

The script downloads the spec from MuleSoft Exchange, extracts the embedded GraphQL SDL into `specs/rosetta-api.graphql` (appending the `schema { query: Query }` root required by ZeroQL), and updates the README version badge. Then rebuild:
```bash
dotnet clean && dotnet build
```

## Future Enhancements

- Response caching options
- 🔄 Retry policies with Polly

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

- The Rosetta API spec models use typed arrays (e.g. `ICollection<Name>`, `ICollection<Email>`) for nested sub-objects on `Person`. In practice the API can return `null` elements or unexpected primitives inside those arrays for records with no data. A `LenientTypedCollectionConverter<T>` is registered in ClientExtensions.UpdateJsonSerializerSettings() during client initialization to silently skip such tokens rather than throwing a `JsonException`. This keeps the library compatible with `System.Text.Json` (no Newtonsoft dependency) while being resilient to inconsistent server payloads.

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
