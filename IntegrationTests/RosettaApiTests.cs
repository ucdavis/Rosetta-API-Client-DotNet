using System.Text.Json;
using Shouldly;
using UCD.Rosetta.Client.GraphQL;
using RestPerson = UCD.Rosetta.Client.Generated.Person;
using RosettaApiException = UCD.Rosetta.Client.Generated.RosettaApiException;

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
    public async Task PeopleAsync_WithEmail_ReturnsResults()
    {
        var email = await GetEmailForPeopleFilterAsync();

        // Act
        var result = await _fixture.Client.Api.PeopleAsync(email: email);

        // Assert — every returned person should have the searched email in at least one email field
        result.ShouldNotBeNull();
        result.ShouldNotBeEmpty();
        result.ShouldAllBe(p =>
            p.Email != null &&
            p.Email.Any(e => e.Primary == email || e.Work == email || e.Personal == email));
    }

    [Fact]
    public async Task PeopleAsync_WithLimit_ReturnsResults()
    {
        // Arrange
        var limit = 5;

        // Act
        var result = await _fixture.Client.Api.PeopleAsync(limit: limit);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Count <= limit, 
            $"Expected at most {limit} results, got {result.Count}");
    }

    [SkippableFact]
    public async Task PeopleAsync_WithIamId_ReturnsResults()
    {
        var iamId = await GetIamIdForPeopleFilterAsync();

        // Act
        var result = await _fixture.Client.Api.PeopleAsync(iamid: iamId);

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
    public async Task PeopleAsync_WithIamIds_ReturnsResults()
    {
        var iamIds = await GetIamIdsForPeopleFilterAsync();

        var requestedIds = iamIds
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .ToHashSet();

        // Act
        var result = await _fixture.Client.Api.PeopleAsync(iamids: iamIds);

        // Assert — every returned person's IAM ID must be one of the requested IDs
        result.ShouldNotBeNull();
        result.ShouldNotBeEmpty();
        result.ShouldAllBe(p => requestedIds.Contains(p.Iam_id));
    }

    [SkippableFact]
    public async Task PeopleAsync_WithLoginId_ReturnsResults()
    {
        var loginId = await GetLoginIdForPeopleFilterAsync();

        // Act
        var result = await _fixture.Client.Api.PeopleAsync(loginid: loginId);

        // Assert — every returned person should have the searched login ID in their identity IDs
        result.ShouldNotBeNull();
        result.ShouldNotBeEmpty();
        result.ShouldAllBe(p =>
            p.Id != null &&
            p.Id.Any(id => id.Login_id == loginId));
    }

    [SkippableFact]
    public async Task PeopleAsync_WithManagerIamId_ReturnsResults()
    {
        var managerIamId = await GetManagerIamIdForPeopleFilterAsync();

        // Act
        var result = await _fixture.Client.Api.PeopleAsync(manager_iam_id: managerIamId);

        // Assert — every returned person should report the searched manager IAM ID
        result.ShouldNotBeNull();
        result.ShouldNotBeEmpty();
        result.ShouldAllBe(p => p.Manager_iam_id == managerIamId);
    }

    #endregion

    private async Task<string> GetIamIdForPeopleFilterAsync()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.TestData.IamId)
            && (await _fixture.Client.Api.PeopleAsync(iamid: _fixture.TestData.IamId)).Count > 0)
        {
            return _fixture.TestData.IamId;
        }

        var iamId = (await _fixture.GetPeopleSampleAsync())
            .FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.Iam_id))
            ?.Iam_id;

        Skip.If(string.IsNullOrWhiteSpace(iamId), "No people with iam_id returned from API sample");
        return iamId!;
    }

    private async Task<string> GetIamIdsForPeopleFilterAsync()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.TestData.IamIds)
            && (await _fixture.Client.Api.PeopleAsync(iamids: _fixture.TestData.IamIds)).Count > 0)
        {
            return _fixture.TestData.IamIds;
        }

        var iamIds = (await _fixture.GetPeopleSampleAsync())
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
            && (await _fixture.Client.Api.PeopleAsync(email: _fixture.TestData.TestEmail)).Count > 0)
        {
            return _fixture.TestData.TestEmail;
        }

        var email = (await _fixture.GetPeopleSampleAsync())
            .SelectMany(GetEmails)
            .FirstOrDefault();

        Skip.If(string.IsNullOrWhiteSpace(email), "No people with email addresses returned from API sample");
        return email!;
    }

    private async Task<string> GetLoginIdForPeopleFilterAsync()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.TestData.LoginId)
            && (await _fixture.Client.Api.PeopleAsync(loginid: _fixture.TestData.LoginId)).Count > 0)
        {
            return _fixture.TestData.LoginId;
        }

        var loginId = (await _fixture.GetPeopleSampleAsync())
            .SelectMany(GetLoginIds)
            .FirstOrDefault();

        Skip.If(string.IsNullOrWhiteSpace(loginId), "No people with login_id returned from API sample");
        return loginId!;
    }

    private async Task<string> GetManagerIamIdForPeopleFilterAsync()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.TestData.ManagerIamId)
            && (await _fixture.Client.Api.PeopleAsync(manager_iam_id: _fixture.TestData.ManagerIamId)).Count > 0)
        {
            return _fixture.TestData.ManagerIamId;
        }

        var managerIamId = (await _fixture.GetPeopleSampleAsync())
            .Select(p => p.Manager_iam_id)
            .FirstOrDefault(id => !string.IsNullOrWhiteSpace(id));

        Skip.If(string.IsNullOrWhiteSpace(managerIamId), "No people with manager_iam_id returned from API sample");
        return managerIamId!;
    }

    private static IEnumerable<string> GetEmails(RestPerson person)
    {
        return person.Email?
            .SelectMany(email => new[] { email.Primary, email.Work, email.Personal })
            .Where(email => !string.IsNullOrWhiteSpace(email))
            .Select(email => email!)
            ?? [];
    }

    private static IEnumerable<string> GetLoginIds(RestPerson person)
    {
        return person.Id?
            .Select(id => id.Login_id)
            .Where(loginId => !string.IsNullOrWhiteSpace(loginId))
            .Select(loginId => loginId!)
            ?? [];
    }

    #region GraphQL

    [Fact]
    public async Task GraphqlAsync_WithPeopleQuery_ReturnsResult()
    {
        // Act
        var result = await GraphqlAsyncWithQuotaSkip(new
            {
                query = "{ people(filter: { limit: 3 }) { results { iam_id displayname } } }"
            });

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GraphQL_TypedPeopleQuery_ReturnsResults()
    {
        var filter = new PeopleFilterInput { Limit = 5 };

        // Act — strongly-typed ZeroQL query; no raw JSON strings
        var response = await TypedGraphqlQueryWithQuotaSkip(() =>
            _fixture.Client.GraphQL.Query(
                q => q.People(filter: filter, selector: result => result.Results(o => new
                    {
                        o.Iam_id,
                        o.Displayname
                    }))));

        // Assert
        response.ShouldNotBeNull();
        response.Data.ShouldNotBeNull($"GraphQL errors: {JsonSerializer.Serialize(response.Errors)}");
        response.Data.ShouldNotBeEmpty();
        response.Data[0]!.Iam_id?.Value.ShouldNotBeNullOrEmpty();
    }

    [SkippableFact]
    public async Task GraphQL_TypedPeopleQuery_ByLoginId_ReturnsMatchingPerson()
    {
        Skip.IfNot(!string.IsNullOrWhiteSpace(_fixture.TestData.LoginId),
            "TestData:LoginId not configured in user secrets or environment variables");

        // ZeroQL requires query arguments to be local variables — cannot capture field accesses
        var loginId = _fixture.TestData.LoginId;
        var filter = new PeopleFilterInput { Loginid = loginId };

        // Act
        var response = await TypedGraphqlQueryWithQuotaSkip(() =>
            _fixture.Client.GraphQL.Query(
                q => q.People(filter: filter, selector: result => result.Results(o => new
                    {
                        o.Iam_id,
                        o.Displayname,
                        LoginId = o.Id(id => id.Login_id)
                    }))));

        // Assert — every returned person should have the searched login ID
        response.Data.ShouldNotBeNull($"GraphQL errors: {JsonSerializer.Serialize(response.Errors)}");
        response.Data.ShouldNotBeEmpty();
        response.Data.ShouldAllBe(p =>
            p != null &&
            p.LoginId != null &&
            p.LoginId.Any(id => id == loginId));
    }

    [Fact]
    public async Task GraphQL_TypedCollegesQuery_ReturnsAllColleges()
    {
        // Act
        var response = await TypedGraphqlQueryWithQuotaSkip(() =>
            _fixture.Client.GraphQL.Query(
                q => q.Colleges(selector: result => result.Results(o => new { o.College_code, o.College_title }))));

        // Assert
        response.Data.ShouldNotBeNull();
        response.Data.ShouldNotBeEmpty();
        response.Data!.All(c => !string.IsNullOrEmpty(c?.College_code)).ShouldBeTrue();
    }

    #endregion

    private async Task<object> GraphqlAsyncWithQuotaSkip(object request)
    {
        for (var attempt = 1; attempt <= 2; attempt++)
        {
            try
            {
                return await _fixture.Client.Api.GraphqlAsync(request);
            }
            catch (RosettaApiException ex) when (IsQuotaExceeded(ex))
            {
                if (attempt == 2)
                    Skip.If(true, "GraphQL query skipped because the test environment returned HTTP 429 quota exceeded");

                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }

        throw new InvalidOperationException("Unreachable GraphQL quota retry state");
    }

    private static async Task<TResponse> TypedGraphqlQueryWithQuotaSkip<TResponse>(Func<Task<TResponse>> query)
    {
        for (var attempt = 1; attempt <= 2; attempt++)
        {
            var response = await query();
            if (!IsQuotaExceeded(response))
                return response;

            if (attempt == 2)
                Skip.If(true, "Typed GraphQL query skipped because the test environment returned HTTP 429 quota exceeded");

            await Task.Delay(TimeSpan.FromSeconds(1));
        }

        throw new InvalidOperationException("Unreachable typed GraphQL quota retry state");
    }

    private static bool IsQuotaExceeded(RosettaApiException ex)
    {
        return ex.StatusCode == 429
            || ex.Response?.Contains("Quota has been exceeded", StringComparison.OrdinalIgnoreCase) == true;
    }

    private static bool IsQuotaExceeded<TResponse>(TResponse response)
    {
        var responseJson = JsonSerializer.Serialize(response);
        return responseJson.Contains("status code 429", StringComparison.OrdinalIgnoreCase)
            || responseJson.Contains("Quota has been exceeded", StringComparison.OrdinalIgnoreCase);
    }

    #region Reference Data

    [Fact]
    public async Task CollegesAsync_ReturnsResults()
    {
        // Act
        var result = await _fixture.Client.Api.CollegesAsync();

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBeGreaterThan(0);
        result.All(c => !string.IsNullOrEmpty(c.College_code)).ShouldBeTrue("All colleges should have a college_code");
        result.All(c => !string.IsNullOrEmpty(c.College_title)).ShouldBeTrue("All colleges should have a college_title");
    }

    [Fact]
    public async Task CollegesAsync_WithCollegeCode_ReturnsMatchingCollege()
    {
        // First get all colleges to find a valid code
        var all = await _fixture.Client.Api.CollegesAsync();
        Skip.If(all.Count == 0, "No colleges returned from API");
        var code = all.Where(c => !string.IsNullOrEmpty(c.College_code))
            .Select(c => c.College_code)
            .FirstOrDefault();
        Skip.If(string.IsNullOrWhiteSpace(code), "No valid college_code found — cannot use as filter");

        // Act
        var result = await _fixture.Client.Api.CollegesAsync(college_code: code);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBeGreaterThan(0);
        result.All(c => c.College_code == code).ShouldBeTrue();
    }

    [Fact]
    public async Task MajorsAsync_ReturnsResults()
    {
        // Act
        var result = await _fixture.Client.Api.MajorsAsync();

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBeGreaterThan(0);
        result.All(m => !string.IsNullOrEmpty(m.Major_code)).ShouldBeTrue("All majors should have a major_code");
        result.All(m => !string.IsNullOrEmpty(m.Major_title)).ShouldBeTrue("All majors should have a major_title");
        result.All(m => m.Major_status == "A" || m.Major_status == "I").ShouldBeTrue("Major status should be A or I");
    }

    [Fact]
    public async Task MajorsAsync_FilterByStatus_ReturnsOnlyActive()
    {
        // Act
        var result = await _fixture.Client.Api.MajorsAsync(major_status: "A");

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBeGreaterThan(0);
        result.All(m => m.Major_status == "A").ShouldBeTrue("Expected only active majors");
    }

    #endregion
}
