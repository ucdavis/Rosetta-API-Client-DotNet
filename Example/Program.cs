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
    Console.WriteLine("Example 1: Search for a person by email");
    Console.WriteLine("----------------------------------------");
    var peopleByEmail = await client.Api.PeopleAsync(email: "email-address@ucdavis.edu");
    Console.WriteLine($"✓ Found {peopleByEmail.Count} person/people\n");

    Console.WriteLine("Example 2: Search for a person by login ID");
    Console.WriteLine("-------------------------------------------");
    var peopleByLogin = await client.Api.PeopleAsync(loginid: "jsmith");
    Console.WriteLine($"✓ Found {peopleByLogin.Count} person/people\n");

    Console.WriteLine("Example 3: Get all colleges");
    Console.WriteLine("--------------------------");
    var colleges = await client.Api.CollegesAsync();
    Console.WriteLine($"✓ Retrieved {colleges.Count} colleges");
    foreach (var college in colleges.Take(3))
        Console.WriteLine($"  {college.College_code}: {college.College_title}");
    Console.WriteLine();

    Console.WriteLine("Example 4: Get active majors");
    Console.WriteLine("----------------------------");
    var majors = await client.Api.MajorsAsync(major_status: "A");
    Console.WriteLine($"✓ Retrieved {majors.Count} active majors");
    foreach (var major in majors.Take(3))
        Console.WriteLine($"  {major.Major_code}: {major.Major_title}");
    Console.WriteLine();

    Console.WriteLine("Example 5: GraphQL query");
    Console.WriteLine("------------------------");
    var graphqlResult = await client.Api.GraphqlAsync(new
    {
        query = "{ people(limit: 5) { iam_id displayname email { primary } } }"
    });
    Console.WriteLine($"✓ GraphQL query returned a result (inspect 'graphqlResult' for data)\n");

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
