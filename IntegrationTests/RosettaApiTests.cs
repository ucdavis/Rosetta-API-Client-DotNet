using Shouldly;
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

    [Fact]
    public async Task PeopleAsync_WithEmail_ReturnsResults()
    {
        // Arrange
        var email = _fixture.TestData.TestEmail ?? "testemail@ucdavis.edu";

        // Act
        var result = await _fixture.Client.Api.PeopleAsync(email: email);

        // Assert
        Assert.NotNull(result);
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
        // Skip if no test data configured
        Skip.IfNot(!string.IsNullOrWhiteSpace(_fixture.TestData.IamId), 
            "TestData:IamId not configured in user secrets or environment variables");

        // Act
        var result = await _fixture.Client.Api.PeopleAsync(iamid: _fixture.TestData.IamId);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Count > 0, "Expected at least one result");

        //I like shouldly...
        result.ShouldNotBeNull();
        result.Count.ShouldBeGreaterThan(0);
        result.ElementAt(0).Iam_id.ShouldBe(_fixture.TestData.IamId);
        result.ElementAt(0).Displayname.ShouldNotBeNullOrEmpty();
        result.ElementAt(0).Displayname.ShouldBe(_fixture.TestData.TestDisplayName);
        result.ElementAt(0).Manager_iam_id.ShouldEndWith("584"); //If using Jason's test user, manager iam id ends with 584
    }

    [SkippableFact]
    public async Task PeopleAsync_WithIamIds_ReturnsResults()
    {
        // Skip if no test data configured
        Skip.IfNot(!string.IsNullOrWhiteSpace(_fixture.TestData.IamIds), 
            "TestData:IamIds not configured in user secrets or environment variables");

        // Act
        var result = await _fixture.Client.Api.PeopleAsync(iamids: _fixture.TestData.IamIds);

        // Assert
        Assert.NotNull(result);
    }

    [SkippableFact]
    public async Task PeopleAsync_WithLoginId_ReturnsResults()
    {
        // Skip if no test data configured
        Skip.IfNot(!string.IsNullOrWhiteSpace(_fixture.TestData.LoginId),
            "TestData:LoginId not configured in user secrets or environment variables");

        // Act
        var result = await _fixture.Client.Api.PeopleAsync(loginid: _fixture.TestData.LoginId);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBeGreaterThan(0);
        result.ElementAt(0).Iam_id.ShouldNotBeNullOrEmpty();
    }

    [SkippableFact]
    public async Task PeopleAsync_WithManagerIamId_ReturnsResults()
    {
        // Skip if no test data configured
        Skip.IfNot(!string.IsNullOrWhiteSpace(_fixture.TestData.ManagerIamId),
            "TestData:ManagerIamId not configured in user secrets or environment variables");

        // Act
        var result = await _fixture.Client.Api.PeopleAsync(manager_iam_id: _fixture.TestData.ManagerIamId);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBeGreaterThan(0);
    }

    #endregion

    #region GraphQL

    [Fact]
    public async Task GraphqlAsync_WithPeopleQuery_ReturnsResult()
    {
        // Act
        var result = await _fixture.Client.Api.GraphqlAsync(new
        {
            query = "{ people(limit: 1) { iam_id displayname email { primary } } }"
        });

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GraphQL_TypedPeopleQuery_ReturnsResults()
    {
        // Act — strongly-typed ZeroQL query; no raw JSON strings
        var response = await _fixture.Client.GraphQL.Query(
            q => q.People(limit: 5, selector: o => new
            {
                o.Iam_id,
                o.Displayname,
                Email = o.Email(e => e.Primary)
            }));

        // Assert
        response.ShouldNotBeNull();
        response.Data.ShouldNotBeNull();
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

        // Act
        var response = await _fixture.Client.GraphQL.Query(
            q => q.People(loginid: loginId, selector: o => new
            {
                o.Iam_id,
                o.Displayname,
                Name = o.Name(n => new { n.Lived_first_name, n.Lived_last_name }),
                Email = o.Email(e => e.Primary)
            }));

        // Assert
        response.Data.ShouldNotBeNull();
        response.Data.ShouldNotBeEmpty();
        response.Data[0]!.Iam_id?.Value.ShouldNotBeNullOrEmpty();
        response.Data[0]!.Displayname.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task GraphQL_TypedCollegesQuery_ReturnsAllColleges()
    {
        // Act
        var response = await _fixture.Client.GraphQL.Query(
            q => q.Colleges(selector: o => new { o.College_code, o.College_title }));

        // Assert
        response.Data.ShouldNotBeNull();
        response.Data.ShouldNotBeEmpty();
        response.Data!.All(c => !string.IsNullOrEmpty(c?.College_code)).ShouldBeTrue();
    }

    #endregion

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
        var code = all.First().College_code;

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
