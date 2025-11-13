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
    }

    #endregion

    #region Identities

    [Fact]
    public async Task IdentitiesAsync_WithLimit_ReturnsResults()
    {
        // Arrange
        var limit = 10;

        // Act
        var result = await _fixture.Client.Api.IdentitiesAsync(limit: limit);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Count <= limit, $"Expected at most {limit} results, got {result.Count}");
    }

    [Fact]
    public async Task IdentitiesAsync_WithoutLimit_ReturnsResults()
    {
        // Act
        var result = await _fixture.Client.Api.IdentitiesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Count > 0, "Expected at least one result");
    }

    #endregion

    #region People

    [Fact]
    public async Task PeopleAsync_WithEmail_ReturnsResults()
    {
        // Arrange
        var email = _fixture.TestData.TestEmail ?? "swebermilne@ucdavis.edu";

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
    }

    [Fact]
    public async Task OrganizationsAsync_ReturnsResults()
    {
        // Act
        var result = await _fixture.Client.Api.OrganizationsAsync();

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task RolesAsync_ReturnsResults()
    {
        // Act
        var result = await _fixture.Client.Api.RolesAsync();

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task CollegesAsync_ReturnsResults()
    {
        // Act
        var result = await _fixture.Client.Api.CollegesAsync();

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task MajorsAsync_ReturnsResults()
    {
        // Act
        var result = await _fixture.Client.Api.MajorsAsync();

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task BaseprofilesAsync_ReturnsResults()
    {
        // Act
        var result = await _fixture.Client.Api.BaseprofilesAsync();

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task EmploymentstatusAsync_ReturnsResults()
    {
        // Act
        var result = await _fixture.Client.Api.EmploymentstatusAsync();

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task UcpathentitlementsAsync_ReturnsResults()
    {
        // Act
        var result = await _fixture.Client.Api.UcpathentitlementsAsync();

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task StudentassociationsAsync_ReturnsResults()
    {
        // Act
        var result = await _fixture.Client.Api.StudentassociationsAsync();

        // Assert
        Assert.NotNull(result);
    }

    #endregion

    #region Campaign Contacts

    [Fact(Skip = "CSV file download - test manually")]
    public async Task CampaignContactsAsync_ReturnsFile()
    {
        // Arrange
        var limit = 10;

        // Act
        var result = await _fixture.Client.Api.CampaignContactsAsync(limit: limit, save: true);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Stream);
    }

    [Fact(Skip = "CSV file download - test manually")]
    public async Task CampaignContactsModifiedAsync_ReturnsFile()
    {
        // Act
        var result = await _fixture.Client.Api.CampaignContactsModifiedAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Stream);
    }

    #endregion
}
