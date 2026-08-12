using System.Net;
using System.Text;
using System.Text.Json;
using Shouldly;
using UCD.Rosetta.Client.Core.Domain;
using UCD.Rosetta.Client.Generated;

namespace IntegrationTests;

public class RosettaDomainClientTests
{
    [Fact]
    public async Task PeopleSearchAsync_DelegatesToGetAndDeserializesShiftedPersonShape()
    {
        var client = CreateGeneratedClient(async request =>
        {
            request.Method.ShouldBe(HttpMethod.Get);
            request.RequestUri!.AbsolutePath.ShouldBe("/api/v1/people");
            request.RequestUri.Query.ShouldContain("loginid=jsmith");
            request.RequestUri.Query.ShouldContain("affiliationState=withAffiliation");
            request.RequestUri.Query.ShouldContain("academic_level=undergrad");
            request.RequestUri.Query.ShouldContain("class_level=00");

            return JsonResponse("""
                [{
                  "iam_id": "1234567890",
                  "displayname": "Jane Smith",
                  "birth_date": "1975-01-23",
                  "manager_iam_id": "1234567891",
                  "provisioning_status": [{
                    "primary": "active",
                    "employee": "active",
                    "health_affiliate": "pending_removal"
                  }],
                  "affiliation": { "employee": "Y" },
                  "employment_status": { "is_msp": "Y", "is_campus_employee": "Y" },
                  "name": { "lived_first_name": "Jane", "lived_last_name": "Smith" },
                  "id": {
                    "iam_id": "1234567890",
                    "login_id": "jsmith",
                    "mail_id": { "campus": "jsmith", "health": "jsmithh" }
                  },
                  "email": {
                    "campus": "jsmith@ucdavis.edu",
                    "health": "jsmith@health.ucdavis.edu",
                    "personal": "jane@example.com"
                  },
                  "phone": {},
                  "student_association": [],
                  "employee_association": [],
                  "modified_date": "2025-12-02T18:30:00Z",
                  "create_date": "2020-09-15T10:05:00Z"
                }]
                """);
        });

        var result = await new PeopleClient(client).SearchAsync(new PeopleQuery
        {
            LoginId = "jsmith",
            AffiliationState = "withAffiliation",
            AcademicLevel = "undergrad",
            ClassLevel = "00"
        });

        var person = result.Single();
        person.Email.Campus.ShouldBe("jsmith@ucdavis.edu");
        person.Email.Health.ShouldBe("jsmith@health.ucdavis.edu");
        person.Id.Mail_id!.Campus.ShouldBe("jsmith");
        person.Affiliation.Employee.ShouldBe("Y");
        person.Employment_status.Is_msp.ShouldBe("Y");
        person.Provisioning_status.Single().Health_affiliate.ShouldBe("pending_removal");
    }

    [Fact]
    public async Task PeopleGetBulkAsync_DelegatesToPostBody()
    {
        var client = CreateGeneratedClient(async request =>
        {
            request.Method.ShouldBe(HttpMethod.Post);
            request.RequestUri!.AbsolutePath.ShouldBe("/api/v1/people");

            var body = await request.Content!.ReadAsStringAsync();
            using var json = JsonDocument.Parse(body);
            json.RootElement.GetProperty("iamids").EnumerateArray().Select(x => x.GetString()).ShouldBe(["1234567890", "0987654321"]);
            json.RootElement.GetProperty("limit").GetInt32().ShouldBe(2);
            json.RootElement.GetProperty("count").GetBoolean().ShouldBeTrue();

            return JsonResponse("[]");
        });

        var result = await new PeopleClient(client).GetBulkAsync(new PeopleBulkQuery
        {
            IamIds = ["1234567890", "0987654321"],
            Limit = 2,
            Count = true
        });

        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task AccountLookupAsync_MapsRawObjectPayloadIntoStableDtos()
    {
        var client = CreateGeneratedClient(request =>
        {
            request.Method.ShouldBe(HttpMethod.Get);
            request.RequestUri!.AbsolutePath.ShouldBe("/api/v1/accounts/lookup");
            request.RequestUri.Query.ShouldContain("iamid=1234567890");

            return Task.FromResult(JsonResponse("""
                [{
                  "identityId": "identity-1",
                  "name": "Wilson Miller",
                  "iamid": "1234567890",
                  "loginId": "wamiller",
                  "email": "wamiller@ucdavis.edu",
                  "accounts": [{
                    "sourceName": "Google Workspace",
                    "accountId": "account-1",
                    "isAdmin": "false",
                    "objectPath": "ucdavis.edu/GAPP",
                    "objectType": "user",
                    "lastLoginTime": "2026-05-11T15:36:21.000Z",
                    "created": "2025-09-22T18:56:01.497Z",
                    "modified": "2026-05-18T16:57:09.720Z"
                  }]
                }]
                """));
        });

        var result = await new AccountsClient(client).LookupAsync(new AccountLookupQuery { IamId = "1234567890" });

        var identity = result.Single();
        identity.IdentityId.ShouldBe("identity-1");
        identity.IamId.ShouldBe("1234567890");
        identity.Accounts.Single().SourceName.ShouldBe("Google Workspace");
        identity.Accounts.Single().Created.ShouldBe(new DateTimeOffset(2025, 9, 22, 18, 56, 1, 497, TimeSpan.Zero));
    }

    [Fact]
    public async Task RoleAndGroupDetails_MapAwkwardGeneratedTypesIntoStableDtos()
    {
        var calls = new Queue<Func<HttpRequestMessage, HttpResponseMessage>>([
            request =>
            {
                request.RequestUri!.AbsolutePath.ShouldBe("/api/v1/roles/role-1");
                request.RequestUri.Query.ShouldContain("limit=5");
                return JsonResponse("""
                    {
                      "roleName": "Example Role",
                      "roleId": "role-1",
                      "description": "Example",
                      "enabled": true,
                      "requestable": false,
                      "roleMembers": [{ "firstname": "Ada", "lastname": "Lovelace", "iamid": "1234567890", "email": "ada@ucdavis.edu" }]
                    }
                    """);
            },
            request =>
            {
                request.RequestUri!.AbsolutePath.ShouldBe("/api/v1/groups/group-1");
                return JsonResponse("""
                    {
                      "groupName": "Example Group",
                      "groupId": "group-1",
                      "groupMembers": [{ "firstname": "Grace", "lastname": "Hopper", "iamid": "0987654321", "email": "grace@ucdavis.edu" }]
                    }
                    """);
            }
        ]);
        var client = CreateGeneratedClient(request => Task.FromResult(calls.Dequeue()(request)));

        var role = await new RolesClient(client).GetByIdAsync("role-1", limit: 5);
        var group = await new GroupsClient(client).GetByIdAsync("group-1");

        role.RoleName.ShouldBe("Example Role");
        role.Members.Single().FirstName.ShouldBe("Ada");
        group.GroupName.ShouldBe("Example Group");
        group.Members.Single().IamId.ShouldBe("0987654321");
        calls.ShouldBeEmpty();
    }

    [Fact]
    public async Task OrganizationsClient_RoutesListAndByIdMethods()
    {
        var calls = new Queue<Func<HttpRequestMessage, HttpResponseMessage>>([
            request =>
            {
                request.RequestUri!.AbsolutePath.ShouldBe("/api/v1/organizations/departments");
                request.RequestUri.Query.ShouldContain("organizationid=ORG001");
                request.RequestUri.Query.ShouldContain("limit=3");
                return JsonResponse("""[{ "organization_id": "ORG001", "organization_title": "Org", "divisions": [] }]""");
            },
            request =>
            {
                request.RequestUri!.AbsolutePath.ShouldBe("/api/v1/organizations/departments/DEPT01");
                return JsonResponse("""
                    {
                      "department_id": "DEPT01",
                      "department_title": "Department",
                      "organization_id": "ORG001",
                      "organization_title": "Org"
                    }
                    """);
            }
        ]);
        var client = CreateGeneratedClient(request => Task.FromResult(calls.Dequeue()(request)));
        var organizations = new OrganizationsClient(client);

        var list = await organizations.GetDepartmentsAsync(new OrganizationQuery { OrganizationId = "ORG001", Limit = 3 });
        var detail = await organizations.GetDepartmentByIdAsync("DEPT01");

        list.Single().Organization_id.ShouldBe("ORG001");
        detail.Department_id.ShouldBe("DEPT01");
        calls.ShouldBeEmpty();
    }

    private static Client CreateGeneratedClient(Func<HttpRequestMessage, Task<HttpResponseMessage>> handler)
    {
        var httpClient = new HttpClient(new FakeHttpMessageHandler(handler));
        return new Client(httpClient)
        {
            BaseUrl = "https://example.test/api/v1"
        };
    }

    private static HttpResponseMessage JsonResponse(string json)
    {
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
    }

    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, Task<HttpResponseMessage>> _handler;

        public FakeHttpMessageHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return _handler(request);
        }
    }
}
