using System.Text.Json;
using Shouldly;
using UCD.Rosetta.Client.Generated;
using UCD.Rosetta.Client.GraphQL;
using Xunit.Abstractions;
using GeneratedPerson = UCD.Rosetta.Client.Generated.Person;

namespace IntegrationTests;

/// <summary>
/// Integration tests for the Rosetta API Client.
/// These tests require valid credentials in user secrets.
/// </summary>
public class RosettaApiTests : IClassFixture<RosettaClientFixture>
{
    private readonly RosettaClientFixture _fixture;
    private readonly ITestOutputHelper _output;

    public RosettaApiTests(RosettaClientFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    #region People

    [SkippableFact]
    public async Task PeopleGETAsync_WithEmail_ReturnsResults()
    {
        var email = await GetEmailForPeopleFilterAsync();

        // Act
        var result = await SkipEnvironmentLimitations(() => _fixture.Client.Api.PeopleGETAsync(email: email));

        // Assert — every returned person should have the searched email in at least one email field
        result.ShouldNotBeNull();
        result.ShouldNotBeEmpty();
        result.ShouldAllBe(p =>
            p.Email != null &&
            (p.Email.Campus == email || p.Email.Health == email || p.Email.Personal == email));
    }

    [SkippableFact]
    public async Task PeopleGETAsync_WithHealthEmail_ReturnsResults()
    {
        var email = await GetHealthEmailForPeopleFilterAsync();

        // Act
        var result = await SkipEnvironmentLimitations(() => _fixture.Client.Api.PeopleGETAsync(email: email));

        // Assert — every returned person should have the searched email in at least one email field
        result.ShouldNotBeNull();
        result.ShouldNotBeEmpty();
        result.Count.ShouldBe(1);
        var data = result.ElementAt(0);
        data.ShouldNotBeNull();
        data.Email.ShouldNotBeNull();
        data.Email.Health.ShouldNotBeNull();
        data.Email.Health.ShouldBe(email);
        data.Email.Campus.ShouldNotBeNull();
        data.Email.Campus.ShouldNotBeNull(email);

    }

    [SkippableFact]
    public async Task PeopleGETAsync_WithLimit_ReturnsResults()
    {
        // Arrange
        var limit = 5;

        // Act
        var result = await SkipEnvironmentLimitations(() => _fixture.Client.Api.PeopleGETAsync(limit: limit));

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Count <= limit, 
            $"Expected at most {limit} results, got {result.Count}");
    }

    [SkippableFact]
    public async Task PeopleGETAsync_WithLastName_ReturnsAllMatchingPeopleIn20RecordPages()
    {
        const string lastName = "Hu";
        const int pageSize = 20;
        var results = new List<GeneratedPerson>();

        // Act
        for (var offset = 0; ; offset += pageSize)
        {
            var page = await SkipEnvironmentLimitations(() =>
                _fixture.Client.Api.PeopleGETAsync(
                    limit: pageSize,
                    offset: offset,
                    lastname: lastName));

            page.ShouldNotBeNull();
            page.Count.ShouldBeLessThanOrEqualTo(pageSize);
            results.AddRange(page);

            if (page.Count < pageSize)
                break;
        }

        // Assert
        results.ShouldNotBeEmpty();
        results.ShouldAllBe(person => !string.IsNullOrWhiteSpace(person.Iam_id));
        results.Select(person => person.Iam_id).Distinct().Count()
            .ShouldBe(results.Count, "Expected all IAM IDs to be unique across pages");
        _output.WriteLine($"Total people matching last name {lastName}: {results.Count}");

        var exactMatches = results.Where(person =>
            string.Equals(person.Name?.Lived_last_name, lastName, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    [SkippableFact]
    public async Task PeopleGETAsync_WithLastNameLike_ReturnsAllMatchingPeopleIn20RecordPages()
    {
        const string lastName = "Sylv";
        const int pageSize = 20;
        var results = new List<GeneratedPerson>();

        // Act
        for (var offset = 0; ; offset += pageSize)
        {
            var page = await SkipEnvironmentLimitations(() =>
                _fixture.Client.Api.PeopleGETAsync(
                    limit: pageSize,
                    offset: offset,
                    lastnamelike: lastName));

            page.ShouldNotBeNull();
            page.Count.ShouldBeLessThanOrEqualTo(pageSize);
            results.AddRange(page);

            if (page.Count < pageSize)
                break;
        }

        // Assert
        results.ShouldNotBeEmpty();
        results.ShouldAllBe(person => !string.IsNullOrWhiteSpace(person.Iam_id));
        results.Select(person => person.Iam_id).Distinct().Count()
            .ShouldBe(results.Count, "Expected all IAM IDs to be unique across pages");
        _output.WriteLine($"Total people matching last name {lastName}: {results.Count}");

        //Found a user with a display name, and a null lived name...
        results.ShouldAllBe(person => person.Name.Lived_last_name == null ||
            person.Name.Lived_last_name.Contains(lastName, StringComparison.OrdinalIgnoreCase));
    }

    [SkippableFact]
    public async Task PeopleGETAsync_WithDepartmentCode_ReturnsMoreThan100Results()
    {
        const string departmentCode = "030000";

        // Act
        var result = await SkipEnvironmentLimitations(() =>
            _fixture.Client.Api.PeopleGETAsync(department: departmentCode));

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBeGreaterThan(100);
    }

    [SkippableFact]
    public async Task PeopleGETAsync_WithIamId_ReturnsResults()
    {
        var iamId = await GetIamIdForPeopleFilterAsync();

        // Act
        var result = await SkipEnvironmentLimitations(() => _fixture.Client.Api.PeopleGETAsync(iamid: iamId));

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
    public async Task PeopleGETAsync_MapsResultToPersonModel()
    {
        var iamId = await GetIamIdForPeopleFilterAsync();

        // Act
        var result = await SkipEnvironmentLimitations(() => _fixture.Client.Api.PeopleGETAsync(iamid: iamId));
        var people = result.Select(PersonModel.FromPerson).ToList();

        // Assert
        people.ShouldNotBeEmpty();

        var sourcePerson = result.First(person => person.Iam_id == iamId);
        var mappedPerson = people.First(person => person.IamId == iamId);

        mappedPerson.IamId.ShouldBe(sourcePerson.Iam_id);
        mappedPerson.EmployeeId.ShouldBe(sourcePerson.Id?.Employee_id);
        mappedPerson.Name.ShouldBe(sourcePerson.Displayname);
        mappedPerson.Emails.ShouldBe(GetEmails(sourcePerson));
    }

    [SkippableFact]
    public async Task PeopleGETAsync_WithIamIds_ReturnsResults()
    {
        var iamIds = await GetIamIdsForPeopleFilterAsync();

        var requestedIds = iamIds
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .ToHashSet();

        // Act
        var result = await SkipEnvironmentLimitations(() => _fixture.Client.Api.PeopleGETAsync(iamids: iamIds));

        // Assert — every returned person's IAM ID must be one of the requested IDs
        result.ShouldNotBeNull();
        result.ShouldNotBeEmpty();
        result.ShouldAllBe(p => requestedIds.Contains(p.Iam_id));
    }

    [SkippableFact]
    public async Task PeoplePOSTAsync_WithIamIds_ReturnsAllResultsInFiveIdBatches()
    {
        const int batchSize = 5;
        var iamIds = _fixture.TestDataBig.IamIds?
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Distinct(StringComparer.Ordinal)
            .ToArray() ?? [];

        Skip.If(iamIds.Length == 0, "TestDataBig__IamIds must contain at least one IAM ID");

        var requestedIds = iamIds.ToHashSet(StringComparer.Ordinal);
        var results = new List<GeneratedPerson>();

        // Act
        foreach (var batch in iamIds.Chunk(batchSize))
        {
            var batchResults = await SkipEnvironmentLimitations(() =>
                _fixture.Client.Api.PeoplePOSTAsync(new PeoplePostRequest
                {
                    Iamids = batch,
                    Count = false,
                    Limit = batch.Length,
                    Offset = 0
                }));

            batchResults.ShouldNotBeNull();
            //batchResults.Count.ShouldBe(batch.Length,
            //    "Expected one result for every IAM ID in the batch");

            var batchIds = batch.ToHashSet(StringComparer.Ordinal);
            //batchIds.SetEquals(batchResults.Select(person => person.Iam_id))
            //    .ShouldBeTrue("Expected the results to match the IAM IDs in the batch");

            results.AddRange(batchResults);
        }

        // Assert
        results.ShouldNotBeEmpty();
        results.ShouldAllBe(person => requestedIds.Contains(person.Iam_id));
        results.Select(person => person.Iam_id).Distinct(StringComparer.Ordinal).Count()
            .ShouldBe(results.Count, "Expected all IAM IDs to be unique across batches");
        requestedIds.SetEquals(results.Select(person => person.Iam_id))
            .ShouldBeTrue("Expected one result for every IAM ID configured in TestDataBig__IamIds");
    }

    [SkippableFact]
    public async Task PeopleGETAsync_WithLoginId_ReturnsResults()
    {
        var loginId = await GetLoginIdForPeopleFilterAsync();

        // Act
        var result = await SkipEnvironmentLimitations(() => _fixture.Client.Api.PeopleGETAsync(loginid: loginId));

        // Assert — every returned person should have the searched login ID in their identity IDs
        result.ShouldNotBeNull();
        result.ShouldNotBeEmpty();
        result.ShouldAllBe(p =>
            p.Id != null &&
            p.Id.Login_id == loginId);
    }

    /// <summary>
    /// Neat, we can query all the users that a manager manages. This is a good test of the manager_iam_id filter.
    /// </summary>
    /// <returns></returns>
    [SkippableFact]
    public async Task PeopleGETAsync_WithManagerIamId_ReturnsResults()
    {
        var managerIamId = await GetManagerIamIdForPeopleFilterAsync();

        // Act
        var result = await SkipEnvironmentLimitations(() => _fixture.Client.Api.PeopleGETAsync(manager_iam_id: managerIamId));

        // Assert — every returned person should report the searched manager IAM ID
        result.ShouldNotBeNull();
        result.ShouldNotBeEmpty();
        result.ShouldAllBe(p => p.Manager_iam_id == managerIamId);
    }

    #endregion

    private async Task<string> GetIamIdForPeopleFilterAsync()
    {
        return await ResolveFilterValueAsync(
            _fixture.TestData.IamId,
            NormalizeFilterValue,
            iamId => _fixture.Client.Api.PeopleGETAsync(iamid: iamId),
            sample => sample.FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.Iam_id))?.Iam_id,
            HasFilterValue,
            "No people with iam_id returned from API sample");
    }

    private async Task<string> GetIamIdsForPeopleFilterAsync()
    {
        return await ResolveFilterValueAsync(
            _fixture.TestData.IamIds,
            NormalizeIamIdsFilterValue,
            iamIds => _fixture.Client.Api.PeopleGETAsync(iamids: iamIds),
            sample =>
            {
                var iamIds = sample
                    .Select(p => p.Iam_id)
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Distinct()
                    .Take(2)
                    .ToArray();

                return iamIds.Length >= 2 ? string.Join(",", iamIds) : null;
            },
            HasFilterValue,
            "Fewer than two distinct people with iam_id returned from API sample");
    }

    private async Task<string> GetEmailForPeopleFilterAsync()
    {
        return await ResolveFilterValueAsync(
            _fixture.TestData.TestEmail,
            NormalizeFilterValue,
            email => _fixture.Client.Api.PeopleGETAsync(email: email),
            sample => sample.SelectMany(GetEmails).FirstOrDefault(),
            HasFilterValue,
            "No people with email addresses returned from API sample");
    }

    private async Task<string> GetHealthEmailForPeopleFilterAsync()
    {
        return await ResolveFilterValueAsync(
            _fixture.TestData.TestHealthEmail,
            NormalizeFilterValue,
            email => _fixture.Client.Api.PeopleGETAsync(email: email),
            sample => sample.SelectMany(GetEmails).FirstOrDefault(),
            HasFilterValue,
            "No people with email addresses returned from API sample");
    }

    private async Task<string> GetLoginIdForPeopleFilterAsync()
    {
        return await ResolveFilterValueAsync(
            _fixture.TestData.LoginId,
            NormalizeFilterValue,
            loginId => _fixture.Client.Api.PeopleGETAsync(loginid: loginId),
            sample => sample.Select(p => p.Id?.Login_id).FirstOrDefault(id => !string.IsNullOrWhiteSpace(id)),
            HasFilterValue,
            "No people with login_id returned from API sample");
    }

    private async Task<string> GetManagerIamIdForPeopleFilterAsync()
    {
        return await ResolveFilterValueAsync(
            _fixture.TestData.ManagerIamId,
            NormalizeFilterValue,
            managerIamId => _fixture.Client.Api.PeopleGETAsync(manager_iam_id: managerIamId),
            sample => sample.Select(p => p.Manager_iam_id).FirstOrDefault(id => !string.IsNullOrWhiteSpace(id)),
            HasFilterValue,
            "No people with manager_iam_id returned from API sample");
    }

    private static IEnumerable<string> GetEmails(GeneratedPerson person)
    {
        return new[] { person.Email?.Campus, person.Email?.Health, person.Email?.Personal }
            .Where(email => !string.IsNullOrWhiteSpace(email))
            .Select(email => email!);
    }

    private async Task<TValue> ResolveFilterValueAsync<TValue>(
        string? configuredValue,
        Func<string, TValue?> configuredSelector,
        Func<TValue, Task<ICollection<GeneratedPerson>>> queryFactory,
        Func<ICollection<GeneratedPerson>, TValue?> sampleSelector,
        Func<TValue?, bool> hasValue,
        string sampleSkipReason)
    {
        if (!string.IsNullOrWhiteSpace(configuredValue))
        {
            var configuredFilterValue = configuredSelector(configuredValue);
            if (hasValue(configuredFilterValue)
                && (await SkipEnvironmentLimitations(() => queryFactory(configuredFilterValue!))).Count > 0)
            {
                return configuredFilterValue!;
            }
        }

        var sampleFilterValue = sampleSelector(await SkipEnvironmentLimitations(() => _fixture.GetPeopleSampleAsync()));

        Skip.If(!hasValue(sampleFilterValue), sampleSkipReason);
        return sampleFilterValue!;
    }

    private static string? NormalizeFilterValue(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static string? NormalizeIamIdsFilterValue(string value)
    {
        var iamIds = value
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct()
            .ToArray();

        return iamIds.Length >= 2 ? string.Join(",", iamIds) : null;
    }

    private static bool HasFilterValue(string? value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }

    #region GraphQL

    [SkippableFact]
    public async Task GraphqlAsync_WithPeopleFilter_ReturnsResult()
    {
        // Act
        var result = await SkipEnvironmentLimitations(() => _fixture.Client.Api.GraphqlAsync(new
        {
            query = "{ people(filter: { limit: 3 }) { results { iam_id displayname email { campus } } } }"
        }));

        // Assert
        result.ShouldBeOfType<JsonElement>();
        var json = (JsonElement)result;

        if (json.TryGetProperty("errors", out var errors) && errors.ValueKind != JsonValueKind.Null)
        {
            errors.ValueKind.ShouldBe(JsonValueKind.Array, $"GraphQL errors should be absent or an empty array, got: {errors}");
            errors.GetArrayLength().ShouldBe(0, $"GraphQL errors: {errors}");
        }

        json.TryGetProperty("data", out var data).ShouldBeTrue("GraphQL response should include a data object.");
        data.ValueKind.ShouldBe(JsonValueKind.Object);
        data.TryGetProperty("people", out var people).ShouldBeTrue("GraphQL response data should include people.");
        people.ValueKind.ShouldBe(JsonValueKind.Object);
        people.TryGetProperty("results", out var results).ShouldBeTrue("GraphQL people data should include results.");
        results.ValueKind.ShouldBe(JsonValueKind.Array);
        results.GetArrayLength().ShouldBeGreaterThan(0, "GraphQL people results should contain at least one result.");
    }

    [SkippableFact]
    public async Task GraphQL_TypedPeopleFilter_ReturnsResults()
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
    public async Task GraphQL_TypedPeopleFilter_WithModifiedSince_ReturnsFirst100In20RecordPages()
    {
        const int pageSize = 20;
        const int recordsToGet = 100;
        var peopleIamIds = new List<string>(recordsToGet);
        int? totalCount = null;

        // Act
        for (var offset = 0;
             offset < recordsToGet && (totalCount is null || offset < totalCount.Value);
             offset += pageSize)
        {
            var filter = new PeopleFilterInput
            {
                Modifiedsince = "2d",
                Count = offset == 0,
                Limit = pageSize,
                Offset = offset
            };

            var response = await SkipEnvironmentLimitations(() => _fixture.Client.GraphQL.Query(
                q => q.People(filter: filter, selector: o => new
                {
                    Results = o.Results(p => new { p.Iam_id, p.Displayname }),
                    Meta = o.Meta(m => new { m.X_total_count })
                })));

            response.Data.ShouldNotBeNull($"GraphQL errors: {JsonSerializer.Serialize(response.Errors)}");
            response.Data.Results.ShouldNotBeNull();
            response.Data.Results.ShouldAllBe(person =>
                person != null &&
                person.Iam_id != null &&
                !string.IsNullOrWhiteSpace(person.Iam_id.Value));

            if (offset == 0)
            {
                response.Data.Meta.ShouldNotBeNull();
                totalCount = response.Data.Meta.X_total_count;
                totalCount.ShouldNotBeNull();
                _output.WriteLine($"Total people modified in the last two days: {totalCount}");
            }

            var expectedPageCount = Math.Min(pageSize, Math.Min(recordsToGet, totalCount!.Value) - offset);
            response.Data.Results.Length.ShouldBe(expectedPageCount);
            peopleIamIds.AddRange(response.Data.Results.Select(person => person!.Iam_id!.Value!));
        }

        // Assert
        totalCount.ShouldNotBeNull();
        var expectedRecordCount = Math.Min(totalCount.Value, recordsToGet);
        peopleIamIds.Count.ShouldBe(expectedRecordCount);

        var uniqueIamIds = peopleIamIds.Distinct().ToList();
        uniqueIamIds.ShouldNotBeNull();
        uniqueIamIds.Count.ShouldBe(expectedRecordCount, "Expected all IAM IDs to be unique across pages");
    }

    [SkippableFact]
    public async Task GraphQL_TypedPeopleFilter_ByLastName_ReturnsAllMatchingPeople()
    {
        const string lastName = "Hu";
        const int pageSize = 20;
        var results = new List<GeneratedPerson>();
        int? totalCount = null;

        // Act
        for (var offset = 0; totalCount is null || offset < totalCount.Value; offset += pageSize)
        {
            var filter = new PeopleFilterInput
            {
                Lastname = lastName,
                Count = offset == 0,
                Limit = pageSize,
                Offset = offset
            };

            var response = await SkipEnvironmentLimitations(() => _fixture.Client.GraphQL.Query(
                q => q.People(filter: filter, selector: o => new
                {
                    Results = o.Results(p => new
                    {
                        p.Iam_id,
                        Name = p.Name(n => new { n.Lived_last_name })
                    }),
                    Meta = o.Meta(m => new { m.X_total_count })
                })));

            response.Data.ShouldNotBeNull($"GraphQL errors: {JsonSerializer.Serialize(response.Errors)}");
            response.Data.Results.ShouldNotBeNull();

            if (offset == 0)
            {
                response.Data.Meta.ShouldNotBeNull();
                totalCount = response.Data.Meta.X_total_count;
                totalCount.ShouldNotBeNull();
                _output.WriteLine($"Total people matching last name {lastName}: {totalCount}");
            }

            var expectedPageCount = Math.Min(pageSize, totalCount!.Value - offset);
            response.Data.Results.Length.ShouldBe(expectedPageCount);
            response.Data.Results.ShouldAllBe(person =>
                person != null &&
                person.Iam_id != null &&
                !string.IsNullOrWhiteSpace(person.Iam_id.Value));

            results.AddRange(response.Data.Results.Select(person => new GeneratedPerson
            {
                Iam_id = person!.Iam_id!.Value!,
                Name = new UCD.Rosetta.Client.Generated.Name
                {
                    Lived_last_name = person.Name?
                        .Where(name => name != null && !string.IsNullOrWhiteSpace(name.Lived_last_name))
                        .Select(name => name!.Lived_last_name!)
                        .FirstOrDefault()
                }
            }));
        }

        // Assert
        totalCount.ShouldNotBeNull();
        results.Count.ShouldBe(totalCount.Value);
        results.Select(result => result.Iam_id).Distinct().Count()
            .ShouldBe(totalCount.Value, "Expected all IAM IDs to be unique across pages");

        var ExactMatches = results.Where(result => string.Equals(result.Name?.Lived_last_name, lastName, StringComparison.OrdinalIgnoreCase)).ToList();

    }

    [SkippableFact]
    public async Task GraphQL_TypedPeopleFilter_ByLoginId_ReturnsMatchingPerson()
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
    public async Task GraphQL_TypedPeopleFilter_ByEmails_ReturnsMatchingPeople()
    {
        var emails = (_fixture.TestData.TestMultipleEmails ?? string.Empty)
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        Skip.If(emails.Length == 0, "TestData:TestMultipleEmails is not configured");

        var requestedEmails = emails.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var filter = new PeopleFilterInput { Emails = emails };

        // Act
        var response = await SkipEnvironmentLimitations(() => _fixture.Client.GraphQL.Query(
            q => q.People(filter: filter, selector: o => new
            {
                Results = o.Results(p => new
                {
                    p.Iam_id,
                    Email = p.Email(e => new { e.Campus, e.Health })
                })
            })));

        // Assert
        response.Data.ShouldNotBeNull($"GraphQL errors: {JsonSerializer.Serialize(response.Errors)}");
        response.Data.Results.ShouldNotBeNull();
        response.Data.Results.Count().ShouldBe(emails.Length);
        response.Data.Results.ShouldAllBe(person =>
            person != null &&
            person.Email != null &&
            person.Email.Any(address =>
                address != null &&
                ((!string.IsNullOrWhiteSpace(address.Campus) && requestedEmails.Contains(address.Campus)) ||
                 (!string.IsNullOrWhiteSpace(address.Health) && requestedEmails.Contains(address.Health)))));

        foreach (var email in requestedEmails)
        {
            response.Data.Results.ShouldContain(person =>
                person != null &&
                person.Email != null &&
                person.Email.Any(address =>
                    address != null &&
                    (string.Equals(address.Campus, email, StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(address.Health, email, StringComparison.OrdinalIgnoreCase))));
        }
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
        var result = await SkipEnvironmentLimitations(() => _fixture.Client.Api.CollegesAsync());

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
        var all = await SkipEnvironmentLimitations(() => _fixture.Client.Api.CollegesAsync());
        Skip.If(all.Count == 0, "No colleges returned from API");
        var code = all.Where(c => !string.IsNullOrEmpty(c.College_code))
            .Select(c => c.College_code)
            .FirstOrDefault();
        Skip.If(string.IsNullOrWhiteSpace(code), "No valid college_code found — cannot use as filter");

        // Act
        var result = await SkipEnvironmentLimitations(() => _fixture.Client.Api.CollegesAsync(college_code: code));

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBeGreaterThan(0);
        result.All(c => c.College_code == code).ShouldBeTrue();
    }

    [SkippableFact]
    public async Task MajorsAsync_ReturnsResults()
    {
        // Act
        var result = await SkipEnvironmentLimitations(() => _fixture.Client.Api.MajorsAsync());

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
        var result = await SkipEnvironmentLimitations(() => _fixture.Client.Api.MajorsAsync(major_status: "A"));

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBeGreaterThan(0);
        result.All(m => m.Major_status == "A").ShouldBeTrue("Expected only active majors");
    }

    #endregion

    #region REST Endpoint Smoke Tests

    [SkippableFact]
    public async Task AccountsApi_Smoke_ReturnsSourcesAndOptionalLookup()
    {
        var counts = await SkipEnvironmentLimitations(() => _fixture.Client.Api.AccountsAsync());
        counts.ShouldNotBeNull();

        var sources = await SkipEnvironmentLimitations(() => _fixture.Client.Api.Sources2Async());
        sources.ShouldNotBeNull();

        var selector = await FirstConfiguredSelectorAsync();
        if (selector != null)
        {
            var lookup = await SkipEnvironmentLimitations(() => _fixture.Client.Api.LookupAsync(
                iamid: selector.IamId,
                iamids: selector.IamIds,
                email: selector.Email,
                loginid: selector.LoginId));
            lookup.ShouldNotBeNull();
        }
    }

    [SkippableFact]
    public async Task RolesApi_Smoke_ListsAndFetchesDiscoveredRole()
    {
        var roles = await SkipEnvironmentLimitations(() => _fixture.Client.Api.RolesAllAsync(limit: 1));
        roles.ShouldNotBeNull();

        var roleId = roles.Select(r => r.RoleId).FirstOrDefault(id => !string.IsNullOrWhiteSpace(id));
        Skip.If(string.IsNullOrWhiteSpace(roleId), "No role ID discovered from roles list");

        var role = await SkipEnvironmentLimitations(() => _fixture.Client.Api.RolesAsync(roleId!, limit: 1));
        role.RoleId.ShouldBe(roleId);

        var membershipSelector = await FirstConfiguredSelectorAsync();
        if (membershipSelector != null)
        {
            var membership = await SkipEnvironmentLimitations(() => _fixture.Client.Api.Membership2Async(
                iamid: membershipSelector.IamId,
                iamids: membershipSelector.IamIds,
                email: membershipSelector.Email,
                loginid: membershipSelector.LoginId,
                limit: 1));
            membership.ShouldNotBeNull();
        }
    }

    [SkippableFact]
    public async Task GroupsApi_Smoke_ListsAndFetchesDiscoveredGroup()
    {
        var groups = await SkipEnvironmentLimitations(() => _fixture.Client.Api.GroupsAllAsync(limit: 1));
        groups.ShouldNotBeNull();

        var sources = await SkipEnvironmentLimitations(() => _fixture.Client.Api.SourcesAsync());
        sources.ShouldNotBeNull();

        var groupId = groups.SelectMany(g => g.Groups ?? []).FirstOrDefault(id => !string.IsNullOrWhiteSpace(id));
        if (!string.IsNullOrWhiteSpace(groupId))
        {
            var group = await SkipEnvironmentLimitations(() => _fixture.Client.Api.GroupsAsync(groupId));
            group.GroupId.ShouldNotBeNullOrWhiteSpace();
        }

        var membershipSelector = await FirstConfiguredSelectorAsync();
        if (membershipSelector != null)
        {
            var membership = await SkipEnvironmentLimitations(() => _fixture.Client.Api.MembershipAsync(
                iamid: membershipSelector.IamId,
                iamids: membershipSelector.IamIds,
                email: membershipSelector.Email,
                loginid: membershipSelector.LoginId,
                limit: 1));
            membership.ShouldNotBeNull();
        }
    }

    [SkippableFact]
    public async Task OrganizationsApi_Smoke_ListsAndFetchesDiscoveredOrganization()
    {
        var organizations = await SkipEnvironmentLimitations(() => _fixture.Client.Api.OrganizationsAllAsync(limit: 1));
        organizations.ShouldNotBeNull();

        var organizationId = organizations.Select(o => o.Organization_id).FirstOrDefault(id => !string.IsNullOrWhiteSpace(id));
        Skip.If(string.IsNullOrWhiteSpace(organizationId), "No organization ID discovered from organization list");

        var organization = await SkipEnvironmentLimitations(() => _fixture.Client.Api.OrganizationsAsync(organizationId!));
        organization.Organization_id.ShouldBe(organizationId);

        var departments = await SkipEnvironmentLimitations(() => _fixture.Client.Api.DepartmentsAll2Async(
            organizationid: organizationId,
            limit: 1));
        departments.ShouldNotBeNull();

        var departmentId = departments.SelectMany(o => o.Divisions ?? [])
            .SelectMany(d => d.Subdivisions ?? [])
            .SelectMany(s => s.Departments ?? [])
            .Select(d => d.Department_id)
            .FirstOrDefault(id => !string.IsNullOrWhiteSpace(id));

        if (!string.IsNullOrWhiteSpace(departmentId))
        {
            var department = await SkipEnvironmentLimitations(() => _fixture.Client.Api.DepartmentsAsync(departmentId));
            department.Department_id.ShouldBe(departmentId);
        }
    }

    [SkippableFact]
    public async Task AssociationApis_Smoke_ReturnLowLimitResults()
    {
        var employees = await SkipEnvironmentLimitations(() => _fixture.Client.Api.EmployeeAssociationAsync(limit: 1));
        employees.ShouldNotBeNull();

        var departments = await SkipEnvironmentLimitations(() => _fixture.Client.Api.DepartmentsAllAsync(limit: 1));
        departments.ShouldNotBeNull();

        var jobTypes = await SkipEnvironmentLimitations(() => _fixture.Client.Api.JobtypeidsAsync());
        jobTypes.ShouldNotBeNull();

        var students = await SkipEnvironmentLimitations(() => _fixture.Client.Api.StudentAssociationAsync(limit: 1));
        students.ShouldNotBeNull();
    }

    [SkippableFact]
    public async Task CampaignContactsApi_Smoke_ReturnsCsvStream()
    {
        using var csv = await SkipEnvironmentLimitations(() => _fixture.Client.Api.CampaignContactsAsync(limit: 1));

        csv.ShouldNotBeNull();
        csv.Stream.ShouldNotBeNull();
    }

    #endregion

    private async Task<IdentitySelector?> FirstConfiguredSelectorAsync()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.TestData.IamId))
            return new IdentitySelector(IamId: _fixture.TestData.IamId);

        if (!string.IsNullOrWhiteSpace(_fixture.TestData.IamIds))
            return new IdentitySelector(IamIds: _fixture.TestData.IamIds);

        if (!string.IsNullOrWhiteSpace(_fixture.TestData.TestEmail))
            return new IdentitySelector(Email: _fixture.TestData.TestEmail);

        if (!string.IsNullOrWhiteSpace(_fixture.TestData.LoginId))
            return new IdentitySelector(LoginId: _fixture.TestData.LoginId);

        var sample = await SkipEnvironmentLimitations(() => _fixture.GetPeopleSampleAsync());
        var person = sample.FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(person?.Iam_id))
            return new IdentitySelector(IamId: person.Iam_id);

        var email = sample.SelectMany(GetEmails).FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(email))
            return new IdentitySelector(Email: email);

        var loginId = sample.Select(p => p.Id?.Login_id).FirstOrDefault(id => !string.IsNullOrWhiteSpace(id));
        if (!string.IsNullOrWhiteSpace(loginId))
            return new IdentitySelector(LoginId: loginId);

        return null;
    }

    private sealed record IdentitySelector(string? IamId = null, string? IamIds = null, string? Email = null, string? LoginId = null);

    private sealed record PersonModel(
        string IamId,
        string? EmployeeId,
        string Name,
        IReadOnlyCollection<string> Emails)
    {
        public static PersonModel FromPerson(GeneratedPerson person) => new(
            IamId: person.Iam_id,
            EmployeeId: person.Id?.Employee_id,
            Name: person.Displayname,
            Emails: GetEmails(person).ToArray());
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
