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
    public async Task PeopleAllAsync_WithEmail_ReturnsResults()
    {
        // Arrange
        var email = "swebermilne@ucdavis.edu";

        // Act
        var result = await _fixture.Client.Api.PeopleAllAsync(email: email);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task PeopleAllAsync_WithLimit_ReturnsResults()
    {
        // Arrange
        var limit = 5;

        // Act
        var result = await _fixture.Client.Api.PeopleAllAsync(limit: limit);

        // Assert
        Assert.NotNull(result);
    }

    [Fact(Skip = "Requires specific IAM ID")]
    public async Task PeopleAllAsync_WithIamId_ReturnsResults()
    {
        // Arrange
        var iamId = "1234567890"; // Replace with actual IAM ID for testing

        // Act
        var result = await _fixture.Client.Api.PeopleAllAsync(iamid: iamId);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Count > 0, "Expected at least one result");
    }

    [Fact(Skip = "Requires specific IAM ID")]
    public async Task PeopleAsync_WithId_ReturnsResult()
    {
        // Arrange
        var id = "1234567890"; // Replace with actual IAM ID for testing

        // Act
        var result = await _fixture.Client.Api.PeopleAsync(id);

        // Assert
        Assert.NotNull(result);
    }

    #endregion

    #region Accounts

    [Fact(Skip = "Requires specific IAM ID")]
    public async Task AccountsAllAsync_WithIamId_ReturnsResults()
    {
        // Arrange
        var iamId = "1234567890"; // Replace with actual IAM ID for testing

        // Act
        var result = await _fixture.Client.Api.AccountsAllAsync(iamid: iamId);

        // Assert
        Assert.NotNull(result);
    }

    [Fact(Skip = "Requires specific IAM IDs")]
    public async Task AccountsAllAsync_WithIamIds_ReturnsResults()
    {
        // Arrange
        var iamIds = "1234567890,0987654321"; // Replace with actual IAM IDs for testing

        // Act
        var result = await _fixture.Client.Api.AccountsAllAsync(iamids: iamIds);

        // Assert
        Assert.NotNull(result);
    }

    [Fact(Skip = "Requires specific account ID")]
    public async Task AccountsAsync_WithId_ReturnsResult()
    {
        // Arrange
        var id = "account-id"; // Replace with actual account ID for testing

        // Act
        var result = await _fixture.Client.Api.AccountsAsync(id);

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

    [Fact(Skip = "Requires specific employee ID")]
    public async Task EmployeesAsync_WithId_ReturnsResult()
    {
        // Arrange
        var id = "1234567890"; // Replace with actual employee ID for testing

        // Act
        var result = await _fixture.Client.Api.EmployeesAsync(id);

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

    [Fact(Skip = "Requires specific student ID")]
    public async Task StudentsAsync_WithId_ReturnsResult()
    {
        // Arrange
        var id = "1234567890"; // Replace with actual student ID for testing

        // Act
        var result = await _fixture.Client.Api.StudentsAsync(id);

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
