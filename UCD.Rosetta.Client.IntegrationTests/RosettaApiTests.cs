namespace UCD.Rosetta.Client.IntegrationTests;

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

    [Fact]
    public async Task MeAsync_ReturnsCurrentUser()
    {
        // Act
        var result = await _fixture.Client.Api.MeAsync();

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task IdentitiesAsync_WithLimit_ReturnsResults()
    {
        // Arrange
        var limit = 10;

        // Act
        var result = await _fixture.Client.Api.IdentitiesAsync(limit: limit);

        // Assert
        Assert.NotNull(result);
        // // Note: API may not respect the limit parameter, just verify we got results
        // Assert.True(result.Count > 0, "Expected at least one result");
        Assert.True(result.Count <= limit, $"Expected at most {limit} results, got {result.Count}");
    }

    [Fact]
    public async Task PeopleAllAsync_WithEmail_ReturnsResults()
    {
        // Arrange
        var email = "someone@ucdavis.edu";

        // Act
        var result = await _fixture.Client.Api.PeopleAllAsync(email: email);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GroupsAsync_ReturnsResults()
    {
        // Act
        var result = await _fixture.Client.Api.GroupsAsync();

        // Assert
        Assert.NotNull(result);
    }

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
}
