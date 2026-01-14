using Shouldly;

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

    #region Authentication & User

    [Fact]
    public async Task MeAsync_ReturnsCurrentUser()
    {
        // Act
        var result = await _fixture.Client.Api.MeAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Fail($"MeAsync returns untyped 'object'. API spec incomplete. Response: {result}");
    }

    #endregion

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

    #endregion

    #region Accounts

    [SkippableFact]
    public async Task AccountsAllAsync_WithIamId_ReturnsResults()
    {
        // Skip if no test data configured
        Skip.IfNot(!string.IsNullOrWhiteSpace(_fixture.TestData.IamId), 
            "TestData:IamId not configured in user secrets or environment variables");

        // Act
        var result = await _fixture.Client.Api.AccountsAllAsync(iamid: _fixture.TestData.IamId);

        // Assert
        Assert.NotNull(result);
        var first = result.FirstOrDefault();
        Assert.Fail($"AccountsAllAsync returns untyped 'ICollection<object>'. API spec incomplete. First result: {first}");
    }

    [SkippableFact]
    public async Task AccountsAllAsync_WithIamIds_ReturnsResults()
    {
        // Skip if no test data configured
        Skip.IfNot(!string.IsNullOrWhiteSpace(_fixture.TestData.IamIds), 
            "TestData:IamIds not configured in user secrets or environment variables");

        // Act
        var result = await _fixture.Client.Api.AccountsAllAsync(iamids: _fixture.TestData.IamIds);

        // Assert
        Assert.NotNull(result);
        var first = result.FirstOrDefault();
        Assert.Fail($"AccountsAllAsync returns untyped 'ICollection<object>'. API spec incomplete. First result: {first}");
    }

    [Fact]
    public async Task AccountsAllAsync_WithLimit_ReturnsResults()
    {
        // Arrange
        var limit = 10;

        // Act
        var result = await _fixture.Client.Api.AccountsAllAsync(limit: limit);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Count <= limit, 
            $"Expected at most {limit} results, got {result.Count}");
        var first = result.FirstOrDefault();
        Assert.Fail($"AccountsAllAsync returns untyped 'ICollection<object>'. API spec incomplete. First result: {first}");
    }

    [SkippableFact]
    public async Task AccountsAsync_WithId_ReturnsResult()
    {
        // Skip if no test data configured
        Skip.IfNot(!string.IsNullOrWhiteSpace(_fixture.TestData.AccountId), 
            "TestData:AccountId not configured in user secrets or environment variables");

        // Act
        var result = await _fixture.Client.Api.AccountsAsync(_fixture.TestData.AccountId!);

        // Assert
        Assert.NotNull(result);
        Assert.Fail($"AccountsAsync returns untyped 'object'. API spec incomplete. Response: {result}");
    }

    #endregion

    #region Employees

    [Fact]
    public async Task EmployeesAllAsync_ReturnsResults()
    {
        // Act
        var result = await _fixture.Client.Api.EmployeesAllAsync();

        // Assert
        Assert.NotNull(result);
        var first = result.FirstOrDefault();
        Assert.Fail($"EmployeesAllAsync returns untyped 'ICollection<object>'. API spec incomplete. First result: {first}");
    }

    [SkippableFact]
    public async Task EmployeesAsync_WithId_ReturnsResult()
    {
        // Skip if no test data configured
        Skip.IfNot(!string.IsNullOrWhiteSpace(_fixture.TestData.EmployeeId), 
            "TestData:EmployeeId not configured in user secrets or environment variables");

        // Act
        var result = await _fixture.Client.Api.EmployeesAsync(_fixture.TestData.EmployeeId!);

        // Assert
        Assert.NotNull(result);
        Assert.Fail($"EmployeesAsync returns untyped 'object'. API spec incomplete. Response: {result}");
    }

    #endregion

    #region Students

    [Fact]
    public async Task StudentsAllAsync_ReturnsResults()
    {
        // Act
        var result = await _fixture.Client.Api.StudentsAllAsync();

        // Assert
        Assert.NotNull(result);
        var first = result.FirstOrDefault();
        Assert.Fail($"StudentsAllAsync returns untyped 'ICollection<object>'. API spec incomplete. First result: {first}");
    }

    [SkippableFact]
    public async Task StudentsAsync_WithId_ReturnsResult()
    {
        // Skip if no test data configured
        Skip.IfNot(!string.IsNullOrWhiteSpace(_fixture.TestData.StudentId), 
            "TestData:StudentId not configured in user secrets or environment variables");

        // Act
        var result = await _fixture.Client.Api.StudentsAsync(_fixture.TestData.StudentId!);

        // Assert
        Assert.NotNull(result);
        Assert.Fail($"StudentsAsync returns untyped 'object'. API spec incomplete. Response: {result}");
    }

    #endregion

    #region Reference Data

    [Fact]
    public async Task GroupsAsync_ReturnsResults()
    {
        // Act
        var result = await _fixture.Client.Api.GroupsAsync();

        // Assert
        Assert.NotNull(result);
        var first = result.FirstOrDefault();
        Assert.Fail($"GroupsAsync returns untyped 'ICollection<object>'. API spec incomplete. First result: {first}");
    }

    [Fact]
    public async Task OrganizationsAsync_ReturnsResults()
    {
        // Act
        var result = await _fixture.Client.Api.OrganizationsAsync();

        // Assert
        Assert.NotNull(result);
        var first = result.FirstOrDefault();
        Assert.Fail($"OrganizationsAsync returns untyped 'ICollection<object>'. API spec incomplete. First result: {first}");
    }

    [Fact]
    public async Task RolesAsync_ReturnsResults()
    {
        // Act
        var result = await _fixture.Client.Api.RolesAsync();

        // Assert
        Assert.NotNull(result);
        var first = result.FirstOrDefault();
        Assert.Fail($"RolesAsync returns untyped 'ICollection<object>'. API spec incomplete. First result: {first}");
    }

    [Fact]
    public async Task CollegesAsync_ReturnsResults()
    {
        // Act
        var result = await _fixture.Client.Api.CollegesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Fail($"CollegesAsync returns untyped 'object'. API spec incomplete. Response: {result}");
    }

    [Fact]
    public async Task MajorsAsync_ReturnsResults()
    {
        // Act
        var result = await _fixture.Client.Api.MajorsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Fail($"MajorsAsync returns untyped 'object'. API spec incomplete. Response: {result}");
    }

    #endregion

    #region Campaign Contacts

    [Fact]
    public async Task CampaignContactsAsync_ReturnsFile()
    {
        // Arrange
        var limit = 10;

        // Act
        var result = await _fixture.Client.Api.CampaignContactsAsync(limit: limit, save: false);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.False(result.IsPartial, "Expected complete response, not partial (206)");
        Assert.NotNull(result.Stream);
        Assert.True(result.Stream.CanRead, "Expected readable stream");
        Assert.True(result.Stream.Length > 0, 
            $"Expected non-empty CSV stream, got {result.Stream.Length} bytes");
        
        // Verify Content-Type header suggests CSV
        if (result.Headers.TryGetValue("Content-Type", out var contentTypes))
        {
            var contentType = string.Join(", ", contentTypes);
            Assert.True(contentType.Contains("csv", StringComparison.OrdinalIgnoreCase) || 
                       contentType.Contains("text", StringComparison.OrdinalIgnoreCase),
                $"Expected CSV content type, got: {contentType}");
        }

        // Basic CSV format validation - read first few bytes to verify it looks like CSV
        using var reader = new StreamReader(result.Stream, leaveOpen: false);
        var firstLine = await reader.ReadLineAsync();
        Assert.NotNull(firstLine);
        Assert.True(firstLine.Contains(',') || firstLine.Contains('\t'),
            $"Expected CSV-like content with delimiters, got: {firstLine.Substring(0, Math.Min(100, firstLine.Length))}");
    }

    [Fact]
    public async Task CampaignContactsModifiedAsync_ReturnsFile()
    {
        // Act
        var result = await _fixture.Client.Api.CampaignContactsModifiedAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.False(result.IsPartial, "Expected complete response, not partial (206)");
        Assert.NotNull(result.Stream);
        Assert.True(result.Stream.CanRead, "Expected readable stream");
        Assert.True(result.Stream.Length > 0, 
            $"Expected non-empty CSV stream, got {result.Stream.Length} bytes");
        
        // Verify Content-Type header suggests CSV
        if (result.Headers.TryGetValue("Content-Type", out var contentTypes))
        {
            var contentType = string.Join(", ", contentTypes);
            Assert.True(contentType.Contains("csv", StringComparison.OrdinalIgnoreCase) || 
                       contentType.Contains("text", StringComparison.OrdinalIgnoreCase),
                $"Expected CSV content type, got: {contentType}");
        }

        // Basic CSV format validation - read first few bytes to verify it looks like CSV
        using var reader = new StreamReader(result.Stream, leaveOpen: false);
        var firstLine = await reader.ReadLineAsync();
        Assert.NotNull(firstLine);
        Assert.True(firstLine.Contains(',') || firstLine.Contains('\t'),
            $"Expected CSV-like content with delimiters, got: {firstLine.Substring(0, Math.Min(100, firstLine.Length))}");
    }

    #endregion
}
