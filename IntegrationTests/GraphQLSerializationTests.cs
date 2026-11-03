using System.Net;
using System.Text;
using System.Text.Json;
using Shouldly;
using UCD.Rosetta.Client.Core;
using UCD.Rosetta.Client.Core.Configuration;
using UCD.Rosetta.Client.GraphQL;
using ZeroQL.Json;

namespace IntegrationTests;

// Offline regression tests: no service credentials or integration-test fixture.
public class GraphQLSerializationTests
{
    public GraphQLSerializationTests()
    {
        // Options may already have been used by another ZeroQL client.
        JsonSerializer.Serialize(42, ZeroQLJsonOptions.Options);
    }

    [Fact]
    public async Task TypedPeopleFilter_ByEmails_SerializesRequestAndReadsResponse()
    {
        using var handler = new GraphQLResponseHandler();
        using var restHttpClient = new HttpClient();
        using var graphqlHttpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://rosetta.example.invalid/graphql")
        };
        using var client = new RosettaClient(restHttpClient, graphqlHttpClient, CreateOptions());
        var filter = new PeopleFilterInput { Emails = ["person@example.invalid"] };

        var response = await client.GraphQL.Query(q => q.People(
            filter: filter,
            selector: people => new
            {
                Results = people.Results(person => new { person.Displayname })
            }));

        handler.RequestBody.ShouldNotBeNull();
        using var request = JsonDocument.Parse(handler.RequestBody);
        var serializedFilter = request.RootElement.GetProperty("variables").GetProperty("filter");
        serializedFilter.GetProperty("emails")[0].GetString().ShouldBe("person@example.invalid");
        serializedFilter.TryGetProperty("iamid", out _).ShouldBeFalse();
        serializedFilter.TryGetProperty("iamId", out _).ShouldBeFalse();
        response.Errors.ShouldBeNull();
        response.Data.ShouldNotBeNull();
        response.Data.Results.ShouldNotBeNull();
        response.Data.Results.Single()!.Displayname.ShouldBe("Test Person");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PeopleFilter_PreservesCaseOnlyAliases(bool useProvidedHttpClients)
    {
        using var restHttpClient = new HttpClient();
        using var graphqlHttpClient = new HttpClient();
        using var client = useProvidedHttpClients
            ? new RosettaClient(restHttpClient, graphqlHttpClient, CreateOptions())
            : new RosettaClient(CreateOptions());
        var filter = new PeopleFilterInput
        {
            Iamid = "12345678",
            IamId = "87654321",
            Loginid = "lower-alias",
            LoginId = "camel-alias"
        };

        using var document = JsonDocument.Parse(JsonSerializer.Serialize(filter, ZeroQLJsonOptions.Options));

        document.RootElement.GetProperty("iamid").GetString().ShouldBe("12345678");
        document.RootElement.GetProperty("iamId").GetString().ShouldBe("87654321");
        document.RootElement.GetProperty("loginid").GetString().ShouldBe("lower-alias");
        document.RootElement.GetProperty("loginId").GetString().ShouldBe("camel-alias");
    }

    [Fact]
    public void AdditionalClients_ReuseSharedSerializerOptions()
    {
        using var firstClient = new RosettaClient(CreateOptions());
        var serializerOptions = ZeroQLJsonOptions.Options;

        using var secondClient = new RosettaClient(CreateOptions());

        ZeroQLJsonOptions.Options.ShouldBeSameAs(serializerOptions);
    }

    private static RosettaClientOptions CreateOptions() => new()
    {
        BaseUrl = "https://rosetta.example.invalid/api/{version}",
        TokenUrl = "https://rosetta.example.invalid/oauth/token",
        ClientId = "offline-test-client",
        ClientSecret = "offline-test-placeholder"
    };

    private sealed class GraphQLResponseHandler : HttpMessageHandler
    {
        public string? RequestBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestBody = await request.Content!.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """{"data":{"people":{"results":[{"displayname":"Test Person"}]}}}""",
                    Encoding.UTF8,
                    "application/json")
            };
        }
    }
}
