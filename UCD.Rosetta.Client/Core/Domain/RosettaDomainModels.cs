using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using UCD.Rosetta.Client.Generated;

#pragma warning disable CS1591

namespace UCD.Rosetta.Client.Core.Domain;

public record PeopleQuery
{
    public string? ModifiedSince { get; init; }
    public bool? Count { get; init; }
    public int? Limit { get; init; }
    public int? Offset { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? IamId { get; init; }
    public string? IamIds { get; init; }
    public string? ManagerIamId { get; init; }
    public string? Email { get; init; }
    public string? LoginId { get; init; }
    public string? EmployeeId { get; init; }
    public string? StudentId { get; init; }
    public string? MailId { get; init; }
    public string? Pidm { get; init; }
    public string? MothraId { get; init; }
    public string? PpsId { get; init; }
    public string? CosmosId { get; init; }
    public string? CpeId { get; init; }
    public string? HealthAffiliateId { get; init; }
    public string? UcanrId { get; init; }
    public string? UsdaWhnrcId { get; init; }
    public string? AffiliateId { get; init; }
    public string? UcnetId { get; init; }
    public string? AffiliationContains { get; init; }
    public string? AffiliationNotContains { get; init; }
    public string? AffiliationState { get; init; }
    public string? Department { get; init; }
    public string? CollegeCode { get; init; }
    public string? MajorCode { get; init; }
    public string? AcademicLevel { get; init; }
    public string? ClassLevel { get; init; }
    public string? OrganizationId { get; init; }
    public string? DivisionId { get; init; }
    public string? SubdivisionId { get; init; }
    public string? SubdivisionL4Id { get; init; }
    public string? EmploymentStatus { get; init; }
}

public record PeopleBulkQuery
{
    public IEnumerable<string> IamIds { get; init; } = [];
    public int? Limit { get; init; }
    public int? Offset { get; init; }
    public bool? Count { get; init; }
}

public record AccountLookupQuery
{
    public string? IamId { get; init; }
    public string? IamIds { get; init; }
    public string? Email { get; init; }
    public string? LoginId { get; init; }
}

public record OrganizationQuery
{
    public string? DepartmentId { get; init; }
    public string? SubdivisionId { get; init; }
    public string? SubdivisionL4Id { get; init; }
    public string? OrganizationId { get; init; }
    public string? DivisionId { get; init; }
    public int? Limit { get; init; }
}

public record EmployeeAssociationQuery
{
    public string? IamId { get; init; }
    public string? IamIds { get; init; }
    public string? LoginId { get; init; }
    public string? EmployeeId { get; init; }
    public string? JobTypeId { get; init; }
    public string? OrganizationId { get; init; }
    public string? DepartmentId { get; init; }
    public string? DivisionId { get; init; }
    public string? SubdivisionId { get; init; }
    public string? SubdivisionL4Id { get; init; }
    public bool? Count { get; init; }
    public int? Limit { get; init; }
    public int? Offset { get; init; }
    public string? ModifiedSince { get; init; }
}

public record StudentAssociationQuery
{
    public string? IamId { get; init; }
    public string? IamIds { get; init; }
    public string? StudentId { get; init; }
    public string? MajorCode { get; init; }
    public string? CollegeCode { get; init; }
    public bool? Count { get; init; }
    public int? Limit { get; init; }
    public int? Offset { get; init; }
    public string? ModifiedSince { get; init; }
}

public record GroupQuery
{
    public string? Source { get; init; }
    public bool? Count { get; init; }
    public int? Limit { get; init; }
    public int? Offset { get; init; }
    public string? SortBy { get; init; }
    public string? SearchAfter { get; init; }
}

public record MembershipQuery
{
    public string? IamId { get; init; }
    public string? IamIds { get; init; }
    public string? Email { get; init; }
    public string? LoginId { get; init; }
    public string? EmployeeId { get; init; }
    public string? Source { get; init; }
    public int? Limit { get; init; }
}

public record SourceSummary(string SourceName, string SourceId);

public record AccountSourceSummary(string SourceName, string SourceId, int AccountCount);

public record AccountLookupResult
{
    [JsonPropertyName("identityId")]
    public string? IdentityId { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("iamid")]
    public string? IamId { get; init; }

    [JsonPropertyName("loginId")]
    public string? LoginId { get; init; }

    [JsonPropertyName("email")]
    public string? Email { get; init; }

    [JsonPropertyName("accounts")]
    public IReadOnlyCollection<AccountSummary> Accounts { get; init; } = [];
}

public record AccountSummary
{
    [JsonPropertyName("sourceName")]
    public string? SourceName { get; init; }

    [JsonPropertyName("accountId")]
    public string? AccountId { get; init; }

    [JsonPropertyName("isAdmin")]
    public string? IsAdmin { get; init; }

    [JsonPropertyName("objectPath")]
    public string? ObjectPath { get; init; }

    [JsonPropertyName("objectType")]
    public string? ObjectType { get; init; }

    [JsonPropertyName("lastLoginTime")]
    public DateTimeOffset? LastLoginTime { get; init; }

    [JsonPropertyName("created")]
    public DateTimeOffset? Created { get; init; }

    [JsonPropertyName("modified")]
    public DateTimeOffset? Modified { get; init; }
}

public record MemberSummary(string? FirstName, string? LastName, string? IamId, string? Email);

public record RoleSummary(string RoleName, string RoleId, string? Description, bool? Enabled, bool? Requestable);

public record RoleDetail(
    string RoleName,
    string RoleId,
    string? Description,
    bool? Enabled,
    bool? Requestable,
    IReadOnlyCollection<MemberSummary> Members);

public record RoleMembershipResult(
    string? Name,
    string? IamId,
    string? LoginId,
    string? EmployeeId,
    string? Email,
    IReadOnlyCollection<RoleSummary> Roles);

public record GroupDetail(string GroupName, string GroupId, IReadOnlyCollection<MemberSummary> Members);

public record GroupMembershipSource(string SourceName, string SourceId, IReadOnlyCollection<string> Groups);

public record GroupMembershipResult(
    string? Name,
    string? IamId,
    string? LoginId,
    string? EmployeeId,
    string? Email,
    IReadOnlyCollection<GroupMembershipSource> Groups);

internal static class RosettaDomainMapping
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public static TEnum? ParseEnumMember<TEnum>(string? value) where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        foreach (var field in typeof(TEnum).GetFields())
        {
            var enumMember = field.GetCustomAttributes(typeof(EnumMemberAttribute), false)
                .OfType<EnumMemberAttribute>()
                .FirstOrDefault();

            if (string.Equals(enumMember?.Value, value, StringComparison.OrdinalIgnoreCase))
                return (TEnum)field.GetValue(null)!;
        }

        return Enum.Parse<TEnum>(value, ignoreCase: true);
    }

    public static IReadOnlyCollection<AccountLookupResult> MapAccountLookup(IEnumerable<object> raw)
    {
        return raw.Select(MapAccountLookup)
            .Where(result => result != null)
            .Cast<AccountLookupResult>()
            .ToList();
    }

    private static AccountLookupResult? MapAccountLookup(object raw)
    {
        if (raw is AccountLookupResult result)
            return result;

        if (raw is JsonElement element)
            return element.Deserialize<AccountLookupResult>(JsonOptions);

        var json = JsonSerializer.Serialize(raw, JsonOptions);
        return JsonSerializer.Deserialize<AccountLookupResult>(json, JsonOptions);
    }

    public static SourceSummary Map(Source source) => new(source.SourceName, source.SourceId);

    public static AccountSourceSummary Map(AccountSourceCount source) =>
        new(source.SourceName, source.SourceId, source.AccountCount);

    public static MemberSummary Map(Items item) =>
        new(item.Firstname, item.Lastname, item.Iamid, item.Email);

    public static RoleSummary Map(Role role) =>
        new(role.RoleName, role.RoleId, role.Description, role.Enabled, role.Requestable);

    public static RoleSummary Map(Items_3 role) =>
        new(role.RoleName, role.RoleId, role.Description, role.Enabled, role.Requestable);

    public static RoleDetail Map(Type_1 role) =>
        new(
            role.RoleName,
            role.RoleId,
            role.Description,
            role.Enabled,
            role.Requestable,
            role.RoleMembers.Select(Map).ToList());

    public static RoleMembershipResult Map(RoleMembership membership) =>
        new(
            membership.Name,
            membership.Iamid,
            membership.LoginId,
            membership.EmployeeId,
            membership.Email,
            membership.Roles.Select(Map).ToList());

    public static GroupDetail Map(Generated.Type group) =>
        new(group.GroupName, group.GroupId, group.GroupMembers.Select(Map).ToList());

    public static GroupMembershipSource Map(Items_2 source) =>
        new(source.SourceName, source.SourceId, source.Groups.ToList());

    public static GroupMembershipResult Map(GroupMembership membership) =>
        new(
            membership.Name,
            membership.Iamid,
            membership.LoginId,
            membership.EmployeeId,
            membership.Email,
            membership.Groups.Select(Map).ToList());
}
