using System.Text.Json;
using Shouldly;
using UCD.Rosetta.Client.Core.Domain;
using UCD.Rosetta.Client.Generated;
using UCD.Rosetta.Client.GraphQL;

namespace IntegrationTests;

/// <summary>
/// Integration tests for the Rosetta API Client.
/// These tests require valid credentials in user secrets.
/// </summary>
public class RosettaApiTests : IClassFixture<RosettaClientFixture>
{
    private readonly RosettaClientFixture _fixture;

    public RosettaApiTests(RosettaClientFixture fixture)
    {
        _fixture = fixture;
    }

    #region People

    [SkippableFact]
    public async Task PeopleSearchAsync_WithEmail_ReturnsResults()
    {
        var email = await GetEmailForPeopleFilterAsync();

        // Act
        var result = await SkipEnvironmentLimitations(() => _fixture.Client.People.SearchAsync(new PeopleQuery { Email = email }));

        // Assert — every returned person should have the searched email in at least one email field
        result.ShouldNotBeNull();
        result.ShouldNotBeEmpty();
        result.ShouldAllBe(p =>
            p.Email != null &&
            (p.Email.Campus == email || p.Email.Health == email || p.Email.Personal == email));
    }

    [SkippableFact]
    public async Task PeopleSearchAsync_WithLimit_ReturnsResults()
    {
        // Arrange
        var limit = 5;

        // Act
        var result = await SkipEnvironmentLimitations(() => _fixture.Client.People.SearchAsync(new PeopleQuery { Limit = limit }));

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Count <= limit, 
            $"Expected at most {limit} results, got {result.Count}");
    }

    [SkippableFact]
    public async Task PeopleSearchAsync_WithIamId_ReturnsResults()
    {
        var iamId = await GetIamIdForPeopleFilterAsync();

        // Act
        var result = await SkipEnvironmentLimitations(() => _fixture.Client.People.SearchAsync(new PeopleQuery { IamId = iamId }));

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Count > 0, "Expected at least one result");

        //I like shouldly...
        result.ShouldNotBeNull();
        result.Count.ShouldBeGreaterThan(0);
        result.ElementAt(0).Iam_id.ShouldBe(iamId);
        result.ElementAt(0).Displayname.ShouldNotBeNullOrEmpty();
        if (!string.IsNullOrWhiteSpace(_fixture.TestData.TestDisplayName)
            && iamId == _fixture.TestData.IamId)
        {
            result.ElementAt(0).Displayname.ShouldBe(_fixture.TestData.TestDisplayName);
        }
    }

    [SkippableFact]
    public async Task PeopleSearchAsync_WithIamIds_ReturnsResults()
    {
        var iamIds = await GetIamIdsForPeopleFilterAsync();

        var requestedIds = iamIds
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .ToHashSet();

        // Act
        var result = await SkipEnvironmentLimitations(() => _fixture.Client.People.SearchAsync(new PeopleQuery { IamIds = iamIds }));

        // Assert — every returned person's IAM ID must be one of the requested IDs
        result.ShouldNotBeNull();
        result.ShouldNotBeEmpty();
        result.ShouldAllBe(p => requestedIds.Contains(p.Iam_id));
    }

    [SkippableFact]
    public async Task PeopleSearchAsync_WithLoginId_ReturnsResults()
    {
        var loginId = await GetLoginIdForPeopleFilterAsync();

        // Act
        var result = await SkipEnvironmentLimitations(() => _fixture.Client.People.SearchAsync(new PeopleQuery { LoginId = loginId }));

        // Assert — every returned person should have the searched login ID in their identity IDs
        result.ShouldNotBeNull();
        result.ShouldNotBeEmpty();
        result.ShouldAllBe(p =>
            p.Id != null &&
            p.Id.Login_id == loginId);
    }

    [SkippableFact]
    public async Task PeopleSearchAsync_WithManagerIamId_ReturnsResults()
    {
        var managerIamId = await GetManagerIamIdForPeopleFilterAsync();

        // Act
        var result = await SkipEnvironmentLimitations(() => _fixture.Client.People.SearchAsync(new PeopleQuery { ManagerIamId = managerIamId }));

        // Assert — every returned person should report the searched manager IAM ID
        result.ShouldNotBeNull();
        result.ShouldNotBeEmpty();
        result.ShouldAllBe(p => p.Manager_iam_id == managerIamId);
    }

    #endregion

    private async Task<string> GetIamIdForPeopleFilterAsync()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.TestData.IamId)
            && (await SkipEnvironmentLimitations(() => _fixture.Client.People.SearchAsync(new PeopleQuery { IamId = _fixture.TestData.IamId }))).Count > 0)
        {
            return _fixture.TestData.IamId;
        }

        var iamId = (await SkipEnvironmentLimitations(() => _fixture.GetPeopleSampleAsync()))
            .FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.Iam_id))
            ?.Iam_id;

        Skip.If(string.IsNullOrWhiteSpace(iamId), "No people with iam_id returned from API sample");
        return iamId!;
    }

    private async Task<string> GetIamIdsForPeopleFilterAsync()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.TestData.IamIds)
            && (await SkipEnvironmentLimitations(() => _fixture.Client.People.SearchAsync(new PeopleQuery { IamIds = _fixture.TestData.IamIds }))).Count > 0)
        {
            return _fixture.TestData.IamIds;
        }

        var iamIds = (await SkipEnvironmentLimitations(() => _fixture.GetPeopleSampleAsync()))
            .Select(p => p.Iam_id)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct()
            .Take(2)
            .ToArray();

        Skip.If(iamIds.Length == 0, "No people with iam_id returned from API sample");
        return string.Join(",", iamIds);
    }

    private async Task<string> GetEmailForPeopleFilterAsync()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.TestData.TestEmail)
            && (await SkipEnvironmentLimitations(() => _fixture.Client.People.SearchAsync(new PeopleQuery { Email = _fixture.TestData.TestEmail }))).Count > 0)
        {
            return _fixture.TestData.TestEmail;
        }

        var email = (await SkipEnvironmentLimitations(() => _fixture.GetPeopleSampleAsync()))
            .SelectMany(GetEmails)
            .FirstOrDefault();

        Skip.If(string.IsNullOrWhiteSpace(email), "No people with email addresses returned from API sample");
        return email!;
    }

    private async Task<string> GetLoginIdForPeopleFilterAsync()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.TestData.LoginId)
            && (await SkipEnvironmentLimitations(() => _fixture.Client.People.SearchAsync(new PeopleQuery { LoginId = _fixture.TestData.LoginId }))).Count > 0)
        {
            return _fixture.TestData.LoginId;
        }

        var loginId = (await SkipEnvironmentLimitations(() => _fixture.GetPeopleSampleAsync()))
            .Select(p => p.Id?.Login_id)
            .FirstOrDefault(id => !string.IsNullOrWhiteSpace(id));

        Skip.If(string.IsNullOrWhiteSpace(loginId), "No people with login_id returned from API sample");
        return loginId!;
    }

    private async Task<string> GetManagerIamIdForPeopleFilterAsync()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.TestData.ManagerIamId)
            && (await SkipEnvironmentLimitations(() => _fixture.Client.People.SearchAsync(new PeopleQuery { ManagerIamId = _fixture.TestData.ManagerIamId }))).Count > 0)
        {
            return _fixture.TestData.ManagerIamId;
        }

        var managerIamId = (await SkipEnvironmentLimitations(() => _fixture.GetPeopleSampleAsync()))
            .Select(p => p.Manager_iam_id)
            .FirstOrDefault(id => !string.IsNullOrWhiteSpace(id));

        Skip.If(string.IsNullOrWhiteSpace(managerIamId), "No people with manager_iam_id returned from API sample");
        return managerIamId!;
    }

    private static IEnumerable<string> GetEmails(UCD.Rosetta.Client.Generated.Person person)
    {
        return new[] { person.Email?.Campus, person.Email?.Health, person.Email?.Personal }
            .Where(email => !string.IsNullOrWhiteSpace(email))
            .Select(email => email!);
    }

    #region GraphQL

    [SkippableFact]
    public async Task GraphqlAsync_WithPeopleQuery_ReturnsResult()
    {
        // Act
        var result = await SkipEnvironmentLimitations(() => _fixture.Client.Api.GraphqlAsync(new
        {
            query = "{ people(filter: { limit: 3 }) { results { iam_id displayname email { campus } } } }"
        }));

        // Assert
        Assert.NotNull(result);
    }

    [SkippableFact]
    public async Task GraphQL_TypedPeopleQuery_ReturnsResults()
    {
        // Act — strongly-typed ZeroQL query; no raw JSON strings
        var filter = new PeopleFilterInput { Limit = 5 };

        var response = await SkipEnvironmentLimitations(() => _fixture.Client.GraphQL.Query(
            q => q.People(filter: filter, selector: o => new
            {
                Results = o.Results(p => new
                {
                    p.Iam_id,
                    p.Displayname,
                    Email = p.Email(e => e.Campus)
                })
            })));

        // Assert
        response.ShouldNotBeNull();
        response.Data.ShouldNotBeNull($"GraphQL errors: {JsonSerializer.Serialize(response.Errors)}");
        response.Data.Results.ShouldNotBeNull();
        response.Data.Results.ShouldNotBeEmpty();
        response.Data.Results[0]!.Iam_id?.Value.ShouldNotBeNullOrEmpty();
    }

    [SkippableFact]
    public async Task GraphQL_TypedPeopleQuery_ByLoginId_ReturnsMatchingPerson()
    {
        // ZeroQL requires query arguments to be local variables — cannot capture field accesses
        var loginId = await GetLoginIdForPeopleFilterAsync();
        var filter = new PeopleFilterInput { Loginid = loginId };

        // Act
        var response = await SkipEnvironmentLimitations(() => _fixture.Client.GraphQL.Query(
            q => q.People(filter: filter, selector: o => new
            {
                Results = o.Results(p => new
                {
                    p.Iam_id,
                    p.Displayname,
                    Name    = p.Name(n  => new { n.Lived_first_name, n.Lived_last_name }),
                    Email   = p.Email(e => e.Campus),
                    LoginId = p.Id(id => id.Login_id)
                })
            })));

        // Assert — every returned person should have the searched login ID
        response.Data.ShouldNotBeNull($"GraphQL errors: {JsonSerializer.Serialize(response.Errors)}");
        response.Data.Results.ShouldNotBeNull();
        response.Data.Results.ShouldNotBeEmpty();
        response.Data.Results.ShouldAllBe(p =>
            p != null &&
            p.LoginId != null &&
            p.LoginId.Any(id => id == loginId));
    }

    [SkippableFact]
    public async Task GraphQL_TypedCollegesQuery_ReturnsAllColleges()
    {
        // Act
        var response = await SkipEnvironmentLimitations(() => _fixture.Client.GraphQL.Query(
            q => q.Colleges(selector: o => new
            {
                Results = o.Results(c => new { c.College_code, c.College_title })
            })));

        // Assert
        response.Data.ShouldNotBeNull($"GraphQL errors: {JsonSerializer.Serialize(response.Errors)}");
        response.Data.Results.ShouldNotBeNull();
        response.Data.Results.ShouldNotBeEmpty();
        response.Data.Results!.All(c => !string.IsNullOrEmpty(c?.College_code)).ShouldBeTrue();
    }

    #endregion

    #region Reference Data

    [SkippableFact]
    public async Task CollegesAsync_ReturnsResults()
    {
        // Act
        var result = await SkipEnvironmentLimitations(() => _fixture.Client.ReferenceData.GetCollegesAsync());

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBeGreaterThan(0);
        result.All(c => !string.IsNullOrEmpty(c.College_code)).ShouldBeTrue("All colleges should have a college_code");
        result.All(c => !string.IsNullOrEmpty(c.College_title)).ShouldBeTrue("All colleges should have a college_title");
    }

    [SkippableFact]
    public async Task CollegesAsync_WithCollegeCode_ReturnsMatchingCollege()
    {
        // First get all colleges to find a valid code
        var all = await SkipEnvironmentLimitations(() => _fixture.Client.ReferenceData.GetCollegesAsync());
        Skip.If(all.Count == 0, "No colleges returned from API");
        var code = all.Where(c => !string.IsNullOrEmpty(c.College_code))
            .Select(c => c.College_code)
            .FirstOrDefault();
        Skip.If(string.IsNullOrWhiteSpace(code), "No valid college_code found — cannot use as filter");

        // Act
        var result = await SkipEnvironmentLimitations(() => _fixture.Client.ReferenceData.GetCollegesAsync(collegeCode: code));

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBeGreaterThan(0);
        result.All(c => c.College_code == code).ShouldBeTrue();
    }

    [SkippableFact]
    public async Task MajorsAsync_ReturnsResults()
    {
        // Act
        var result = await SkipEnvironmentLimitations(() => _fixture.Client.ReferenceData.GetMajorsAsync());

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBeGreaterThan(0);
        result.All(m => !string.IsNullOrEmpty(m.Major_code)).ShouldBeTrue("All majors should have a major_code");
        result.All(m => !string.IsNullOrEmpty(m.Major_title)).ShouldBeTrue("All majors should have a major_title");
        result.All(m => m.Major_status == "A" || m.Major_status == "I").ShouldBeTrue("Major status should be A or I");
    }

    [SkippableFact]
    public async Task MajorsAsync_FilterByStatus_ReturnsOnlyActive()
    {
        // Act
        var result = await SkipEnvironmentLimitations(() => _fixture.Client.ReferenceData.GetMajorsAsync(majorStatus: "A"));

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBeGreaterThan(0);
        result.All(m => m.Major_status == "A").ShouldBeTrue("Expected only active majors");
    }

    #endregion

    #region Domain Smoke Tests

    [SkippableFact]
    public async Task AccountsClient_Smoke_ReturnsSourcesAndOptionalLookup()
    {
        var counts = await SkipEnvironmentLimitations(() => _fixture.Client.Accounts.GetSourceCountsAsync());
        counts.ShouldNotBeNull();

        var sources = await SkipEnvironmentLimitations(() => _fixture.Client.Accounts.GetSourcesAsync());
        sources.ShouldNotBeNull();

        var query = await FirstConfiguredSelectorAsync();
        if (query != null)
        {
            var lookup = await SkipEnvironmentLimitations(() => _fixture.Client.Accounts.LookupAsync(query));
            lookup.ShouldNotBeNull();
        }
    }

    [SkippableFact]
    public async Task RolesClient_Smoke_ListsAndFetchesDiscoveredRole()
    {
        var roles = await SkipEnvironmentLimitations(() => _fixture.Client.Roles.ListAsync(limit: 1));
        roles.ShouldNotBeNull();

        var roleId = roles.Select(r => r.RoleId).FirstOrDefault(id => !string.IsNullOrWhiteSpace(id));
        Skip.If(string.IsNullOrWhiteSpace(roleId), "No role ID discovered from roles list");

        var role = await SkipEnvironmentLimitations(() => _fixture.Client.Roles.GetByIdAsync(roleId!, limit: 1));
        role.RoleId.ShouldBe(roleId);

        var membershipQuery = await FirstMembershipSelectorAsync();
        if (membershipQuery != null)
        {
            var membership = await SkipEnvironmentLimitations(() => _fixture.Client.Roles.GetMembershipAsync(membershipQuery));
            membership.ShouldNotBeNull();
        }
    }

    [SkippableFact]
    public async Task GroupsClient_Smoke_ListsAndFetchesDiscoveredGroup()
    {
        var groups = await SkipEnvironmentLimitations(() => _fixture.Client.Groups.ListAsync(new GroupQuery { Limit = 1 }));
        groups.ShouldNotBeNull();

        var sources = await SkipEnvironmentLimitations(() => _fixture.Client.Groups.GetSourcesAsync());
        sources.ShouldNotBeNull();

        var groupId = groups.SelectMany(g => g.Groups).FirstOrDefault(id => !string.IsNullOrWhiteSpace(id));
        if (!string.IsNullOrWhiteSpace(groupId))
        {
            var group = await SkipEnvironmentLimitations(() => _fixture.Client.Groups.GetByIdAsync(groupId));
            group.GroupId.ShouldNotBeNullOrWhiteSpace();
        }

        var membershipQuery = await FirstMembershipSelectorAsync();
        if (membershipQuery != null)
        {
            var membership = await SkipEnvironmentLimitations(() => _fixture.Client.Groups.GetMembershipAsync(membershipQuery));
            membership.ShouldNotBeNull();
        }
    }

    [SkippableFact]
    public async Task OrganizationsClient_Smoke_ListsAndFetchesDiscoveredOrganization()
    {
        var organizations = await SkipEnvironmentLimitations(() => _fixture.Client.Organizations.ListAsync(new OrganizationQuery { Limit = 1 }));
        organizations.ShouldNotBeNull();

        var organizationId = organizations.Select(o => o.Organization_id).FirstOrDefault(id => !string.IsNullOrWhiteSpace(id));
        Skip.If(string.IsNullOrWhiteSpace(organizationId), "No organization ID discovered from organization list");

        var organization = await SkipEnvironmentLimitations(() => _fixture.Client.Organizations.GetByIdAsync(organizationId!));
        organization.Organization_id.ShouldBe(organizationId);

        var departments = await SkipEnvironmentLimitations(() => _fixture.Client.Organizations.GetDepartmentsAsync(new OrganizationQuery
        {
            OrganizationId = organizationId,
            Limit = 1
        }));
        departments.ShouldNotBeNull();

        var departmentId = departments.SelectMany(o => o.Divisions ?? [])
            .SelectMany(d => d.Subdivisions ?? [])
            .SelectMany(s => s.Departments ?? [])
            .Select(d => d.Department_id)
            .FirstOrDefault(id => !string.IsNullOrWhiteSpace(id));

        if (!string.IsNullOrWhiteSpace(departmentId))
        {
            var department = await SkipEnvironmentLimitations(() => _fixture.Client.Organizations.GetDepartmentByIdAsync(departmentId));
            department.Department_id.ShouldBe(departmentId);
        }
    }

    [SkippableFact]
    public async Task AssociationClients_Smoke_ReturnLowLimitResults()
    {
        var employees = await SkipEnvironmentLimitations(() => _fixture.Client.EmployeeAssociations.SearchAsync(new EmployeeAssociationQuery { Limit = 1 }));
        employees.ShouldNotBeNull();

        var departments = await SkipEnvironmentLimitations(() => _fixture.Client.EmployeeAssociations.GetDepartmentsAsync(new OrganizationQuery { Limit = 1 }));
        departments.ShouldNotBeNull();

        var jobTypes = await SkipEnvironmentLimitations(() => _fixture.Client.EmployeeAssociations.GetJobTypeIdsAsync());
        jobTypes.ShouldNotBeNull();

        var students = await SkipEnvironmentLimitations(() => _fixture.Client.StudentAssociations.SearchAsync(new StudentAssociationQuery { Limit = 1 }));
        students.ShouldNotBeNull();
    }

    [SkippableFact]
    public async Task CampaignContactsClient_Smoke_ReturnsCsvStream()
    {
        using var csv = await SkipEnvironmentLimitations(() => _fixture.Client.CampaignContacts.GetCsvAsync(limit: 1));

        csv.ShouldNotBeNull();
        csv.Stream.ShouldNotBeNull();
    }

    #endregion

    private async Task<AccountLookupQuery?> FirstConfiguredSelectorAsync()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.TestData.IamId))
            return new AccountLookupQuery { IamId = _fixture.TestData.IamId };

        if (!string.IsNullOrWhiteSpace(_fixture.TestData.IamIds))
            return new AccountLookupQuery { IamIds = _fixture.TestData.IamIds };

        if (!string.IsNullOrWhiteSpace(_fixture.TestData.TestEmail))
            return new AccountLookupQuery { Email = _fixture.TestData.TestEmail };

        if (!string.IsNullOrWhiteSpace(_fixture.TestData.LoginId))
            return new AccountLookupQuery { LoginId = _fixture.TestData.LoginId };

        var sample = await SkipEnvironmentLimitations(() => _fixture.GetPeopleSampleAsync());
        var person = sample.FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(person?.Iam_id))
            return new AccountLookupQuery { IamId = person.Iam_id };

        var email = sample.SelectMany(GetEmails).FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(email))
            return new AccountLookupQuery { Email = email };

        var loginId = sample.Select(p => p.Id?.Login_id).FirstOrDefault(id => !string.IsNullOrWhiteSpace(id));
        if (!string.IsNullOrWhiteSpace(loginId))
            return new AccountLookupQuery { LoginId = loginId };

        return null;
    }

    private async Task<MembershipQuery?> FirstMembershipSelectorAsync()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.TestData.IamId))
            return new MembershipQuery { IamId = _fixture.TestData.IamId, Limit = 1 };

        if (!string.IsNullOrWhiteSpace(_fixture.TestData.IamIds))
            return new MembershipQuery { IamIds = _fixture.TestData.IamIds, Limit = 1 };

        if (!string.IsNullOrWhiteSpace(_fixture.TestData.TestEmail))
            return new MembershipQuery { Email = _fixture.TestData.TestEmail, Limit = 1 };

        if (!string.IsNullOrWhiteSpace(_fixture.TestData.LoginId))
            return new MembershipQuery { LoginId = _fixture.TestData.LoginId, Limit = 1 };

        var sample = await SkipEnvironmentLimitations(() => _fixture.GetPeopleSampleAsync());
        var person = sample.FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(person?.Iam_id))
            return new MembershipQuery { IamId = person.Iam_id, Limit = 1 };

        var email = sample.SelectMany(GetEmails).FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(email))
            return new MembershipQuery { Email = email, Limit = 1 };

        var loginId = sample.Select(p => p.Id?.Login_id).FirstOrDefault(id => !string.IsNullOrWhiteSpace(id));
        if (!string.IsNullOrWhiteSpace(loginId))
            return new MembershipQuery { LoginId = loginId, Limit = 1 };

        return null;
    }

    private static async Task<T> SkipEnvironmentLimitations<T>(Func<Task<T>> action)
    {
        for (var attempt = 1; attempt <= 2; attempt++)
        {
            try
            {
                var result = await action();
                if (result is ZeroQL.IGraphQLResult graphqlResult && IsQuotaExceeded(graphqlResult))
                {
                    if (attempt == 2)
                        Skip.If(true, "Rosetta API returned 429 via GraphQL errors; treating as rate-limit environment limitation.");

                    await Task.Delay(TimeSpan.FromSeconds(1));
                    continue;
                }

                return result;
            }
            catch (RosettaApiException ex) when (ex.StatusCode is 401 or 403)
            {
                Skip.If(true, $"Rosetta API returned {ex.StatusCode}; treating as credential/scope environment limitation.");
                throw;
            }
            catch (RosettaApiException ex) when (IsQuotaExceeded(ex))
            {
                if (attempt == 2)
                    Skip.If(true, "Rosetta API returned 429; treating as rate-limit environment limitation.");

                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }

        throw new InvalidOperationException("Unreachable environment-limit retry state.");
    }

    private static bool IsQuotaExceeded(RosettaApiException ex)
    {
        return ex.StatusCode == 429
            || ex.Response?.Contains("Quota has been exceeded", StringComparison.OrdinalIgnoreCase) == true;
    }

    private static bool IsQuotaExceeded(ZeroQL.IGraphQLResult response)
    {
        return response.Errors?.Any(error =>
            error.Message?.Contains("status code 429", StringComparison.OrdinalIgnoreCase) == true
            || error.Message?.Contains("Quota has been exceeded", StringComparison.OrdinalIgnoreCase) == true) == true;
    }
}
