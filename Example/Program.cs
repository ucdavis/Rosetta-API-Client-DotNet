using UCD.Rosetta.Client.Core;
using UCD.Rosetta.Client.Core.Configuration;
using Microsoft.Extensions.Configuration;

// Example: Using the Rosetta API Client
Console.WriteLine("UC Davis Rosetta API Client Example");
Console.WriteLine("====================================\n");

// Configuration - load from appsettings.json, user secrets and environment variables
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddUserSecrets(System.Reflection.Assembly.GetEntryAssembly() ?? typeof(Program).Assembly, optional: true)
    .AddEnvironmentVariables(prefix: "ROSETTA_")
    .Build();

var options = new RosettaClientOptions();
configuration.GetSection("RosettaClient").Bind(options);

// Create the client
using var client = new RosettaClient(options);

// Enable debug logging to see raw API responses
// Use -1 for unlimited, 0 to disable, or a positive number (e.g., 1000) to limit output
client.DebugResponseMaxLength = 0;

try
{
    Console.WriteLine("Example 1: Get information about the authenticated user");
    Console.WriteLine("--------------------------------------------------------");
    var me = await client.Api.MeAsync();
    Console.WriteLine($"✓ Successfully retrieved user information");
    Console.WriteLine($"  (Inspect the 'me' object to see available properties)\n");

    Console.WriteLine("Example 2: Search for a person by email");
    Console.WriteLine("----------------------------------------");
    var peopleByEmail = await client.Api.PeopleAsync(email: "email-address@ucdavis.edu");
    Console.WriteLine($"✓ Found {peopleByEmail.Count} person/people\n");

    Console.WriteLine("Example 3: Get accounts for a specific IAM ID");
    Console.WriteLine("----------------------------------------------");
    var accounts = await client.Api.AccountsAllAsync(iamid: "1234567890");
    Console.WriteLine($"✓ Found {accounts.Count} account(s)\n");

    Console.WriteLine("Example 4: Get identities modified in last 24 hours");
    Console.WriteLine("----------------------------------------------------");
    var identities = await client.Api.IdentitiesAsync(limit: 10);
    Console.WriteLine($"✓ Retrieved {identities.Count} identities\n");

    Console.WriteLine("Example 5: Get all groups");
    Console.WriteLine("-------------------------");
    var groups = await client.Api.GroupsAsync();
    Console.WriteLine($"✓ Retrieved {groups.Count} groups\n");

    Console.WriteLine("✓ All examples completed successfully!");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error: {ex.Message}");
    Console.WriteLine("\nNote: This example requires valid credentials and network access to the Rosetta API.");
    Console.WriteLine("Update the configuration at the top of Program.cs with your actual credentials.");
}

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();
