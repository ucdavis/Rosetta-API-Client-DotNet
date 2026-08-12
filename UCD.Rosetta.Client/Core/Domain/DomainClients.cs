using UCD.Rosetta.Client.Generated;

#pragma warning disable CS1591

namespace UCD.Rosetta.Client.Core.Domain;

public sealed class AccountsClient
{
    private readonly IClient _api;

    public AccountsClient(IClient api)
    {
        _api = api ?? throw new ArgumentNullException(nameof(api));
    }

    public async Task<IReadOnlyCollection<AccountSourceSummary>> GetSourceCountsAsync(CancellationToken cancellationToken = default)
    {
        var sources = await _api.AccountsAsync(cancellationToken).ConfigureAwait(false);
        return sources.Select(RosettaDomainMapping.Map).ToList();
    }

    public async Task<IReadOnlyCollection<SourceSummary>> GetSourcesAsync(CancellationToken cancellationToken = default)
    {
        var sources = await _api.Sources2Async(cancellationToken).ConfigureAwait(false);
        return sources.Select(RosettaDomainMapping.Map).ToList();
    }

    public async Task<IReadOnlyCollection<AccountLookupResult>> LookupAsync(AccountLookupQuery query, CancellationToken cancellationToken = default)
    {
        if (query == null)
            throw new ArgumentNullException(nameof(query));

        var raw = await _api.LookupAsync(
            iamid: query.IamId,
            iamids: query.IamIds,
            email: query.Email,
            loginid: query.LoginId,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        return RosettaDomainMapping.MapAccountLookup(raw);
    }
}

public sealed class RolesClient
{
    private readonly IClient _api;

    public RolesClient(IClient api)
    {
        _api = api ?? throw new ArgumentNullException(nameof(api));
    }

    public async Task<IReadOnlyCollection<RoleSummary>> ListAsync(int? limit = null, CancellationToken cancellationToken = default)
    {
        var roles = await _api.RolesAllAsync(limit, cancellationToken).ConfigureAwait(false);
        return roles.Select(RosettaDomainMapping.Map).ToList();
    }

    public async Task<RoleDetail> GetByIdAsync(string id, int? limit = null, CancellationToken cancellationToken = default)
    {
        var role = await _api.RolesAsync(id, limit, cancellationToken).ConfigureAwait(false);
        return RosettaDomainMapping.Map(role);
    }

    public async Task<IReadOnlyCollection<RoleMembershipResult>> GetMembershipAsync(MembershipQuery? query = null, CancellationToken cancellationToken = default)
    {
        query ??= new MembershipQuery();
        var memberships = await _api.Membership2Async(
            iamid: query.IamId,
            iamids: query.IamIds,
            email: query.Email,
            loginid: query.LoginId,
            employeeid: query.EmployeeId,
            limit: query.Limit,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        return memberships.Select(RosettaDomainMapping.Map).ToList();
    }
}

public sealed class GroupsClient
{
    private readonly IClient _api;

    public GroupsClient(IClient api)
    {
        _api = api ?? throw new ArgumentNullException(nameof(api));
    }

    public Task<ICollection<Group>> ListAsync(GroupQuery? query = null, CancellationToken cancellationToken = default)
    {
        query ??= new GroupQuery();
        return _api.GroupsAllAsync(
            source: query.Source,
            count: query.Count,
            limit: query.Limit,
            offset: query.Offset,
            sortBy: query.SortBy,
            searchAfter: query.SearchAfter,
            cancellationToken: cancellationToken);
    }

    public async Task<GroupDetail> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var group = await _api.GroupsAsync(id, cancellationToken).ConfigureAwait(false);
        return RosettaDomainMapping.Map(group);
    }

    public async Task<IReadOnlyCollection<SourceSummary>> GetSourcesAsync(CancellationToken cancellationToken = default)
    {
        var sources = await _api.SourcesAsync(cancellationToken).ConfigureAwait(false);
        return sources.Select(RosettaDomainMapping.Map).ToList();
    }

    public async Task<IReadOnlyCollection<GroupMembershipResult>> GetMembershipAsync(MembershipQuery? query = null, CancellationToken cancellationToken = default)
    {
        query ??= new MembershipQuery();
        var memberships = await _api.MembershipAsync(
            iamid: query.IamId,
            iamids: query.IamIds,
            email: query.Email,
            loginid: query.LoginId,
            employeeid: query.EmployeeId,
            source: query.Source,
            limit: query.Limit,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        return memberships.Select(RosettaDomainMapping.Map).ToList();
    }
}

public sealed class OrganizationsClient
{
    private readonly IClient _api;

    public OrganizationsClient(IClient api)
    {
        _api = api ?? throw new ArgumentNullException(nameof(api));
    }

    public Task<ICollection<Organization>> ListAsync(OrganizationQuery? query = null, CancellationToken cancellationToken = default)
    {
        query ??= new OrganizationQuery();
        return _api.OrganizationsAllAsync(
            departmentid: query.DepartmentId,
            subdivisionid: query.SubdivisionId,
            subdivisionl4id: query.SubdivisionL4Id,
            organizationid: query.OrganizationId,
            divisionid: query.DivisionId,
            limit: query.Limit,
            cancellationToken: cancellationToken);
    }

    public Task<OrganizationDivision> GetByIdAsync(string id, CancellationToken cancellationToken = default) =>
        _api.OrganizationsAsync(id, cancellationToken);

    public Task<ICollection<OrganizationDivision>> GetDivisionsAsync(OrganizationQuery? query = null, CancellationToken cancellationToken = default)
    {
        query ??= new OrganizationQuery();
        return _api.DivisionsAllAsync(query.DepartmentId, query.SubdivisionId, query.SubdivisionL4Id, query.OrganizationId, query.DivisionId, query.Limit, cancellationToken);
    }

    public Task<DivisionSubdivision> GetDivisionByIdAsync(string id, CancellationToken cancellationToken = default) =>
        _api.DivisionsAsync(id, cancellationToken);

    public Task<ICollection<OrganizationSubdivision>> GetSubdivisionsAsync(OrganizationQuery? query = null, CancellationToken cancellationToken = default)
    {
        query ??= new OrganizationQuery();
        return _api.SubdivisionsAllAsync(query.DepartmentId, query.SubdivisionId, query.SubdivisionL4Id, query.OrganizationId, query.DivisionId, query.Limit, cancellationToken);
    }

    public Task<SubdivisionSubdivisionL4> GetSubdivisionByIdAsync(string id, CancellationToken cancellationToken = default) =>
        _api.SubdivisionsAsync(id, cancellationToken);

    public Task<ICollection<OrganizationSubdivisionL4>> GetSubdivisionL4sAsync(OrganizationQuery? query = null, CancellationToken cancellationToken = default)
    {
        query ??= new OrganizationQuery();
        return _api.SubdivisionL4sAllAsync(query.DepartmentId, query.SubdivisionId, query.SubdivisionL4Id, query.OrganizationId, query.DivisionId, query.Limit, cancellationToken);
    }

    public Task<SubdivisionL4Department> GetSubdivisionL4ByIdAsync(string id, CancellationToken cancellationToken = default) =>
        _api.SubdivisionL4sAsync(id, cancellationToken);

    public Task<ICollection<OrganizationDepartment>> GetDepartmentsAsync(OrganizationQuery? query = null, CancellationToken cancellationToken = default)
    {
        query ??= new OrganizationQuery();
        return _api.DepartmentsAll2Async(query.DepartmentId, query.SubdivisionId, query.SubdivisionL4Id, query.OrganizationId, query.DivisionId, query.Limit, cancellationToken);
    }

    public Task<Department> GetDepartmentByIdAsync(string id, CancellationToken cancellationToken = default) =>
        _api.DepartmentsAsync(id, cancellationToken);
}

public sealed class EmployeeAssociationsClient
{
    private readonly IClient _api;

    public EmployeeAssociationsClient(IClient api)
    {
        _api = api ?? throw new ArgumentNullException(nameof(api));
    }

    public Task<ICollection<EmployeeAssociation>> SearchAsync(EmployeeAssociationQuery? query = null, CancellationToken cancellationToken = default)
    {
        query ??= new EmployeeAssociationQuery();
        return _api.EmployeeAssociationAsync(
            iamid: query.IamId,
            iamids: query.IamIds,
            loginid: query.LoginId,
            employeeid: query.EmployeeId,
            jobtypeid: query.JobTypeId,
            organizationid: query.OrganizationId,
            departmentid: query.DepartmentId,
            divisionid: query.DivisionId,
            subdivisionid: query.SubdivisionId,
            subdivisionl4id: query.SubdivisionL4Id,
            count: query.Count,
            limit: query.Limit,
            offset: query.Offset,
            modifiedsince: query.ModifiedSince,
            cancellationToken: cancellationToken);
    }

    public Task<ICollection<Department>> GetDepartmentsAsync(OrganizationQuery? query = null, CancellationToken cancellationToken = default)
    {
        query ??= new OrganizationQuery();
        return _api.DepartmentsAllAsync(query.DepartmentId, query.SubdivisionId, query.SubdivisionL4Id, query.OrganizationId, query.DivisionId, query.Limit, cancellationToken);
    }

    public Task<ICollection<JobTypeID>> GetJobTypeIdsAsync(string? jobTypeId = null, CancellationToken cancellationToken = default) =>
        _api.JobtypeidsAsync(jobTypeId, cancellationToken);
}

public sealed class StudentAssociationsClient
{
    private readonly IClient _api;

    public StudentAssociationsClient(IClient api)
    {
        _api = api ?? throw new ArgumentNullException(nameof(api));
    }

    public Task<ICollection<StudentAssociation>> SearchAsync(StudentAssociationQuery? query = null, CancellationToken cancellationToken = default)
    {
        query ??= new StudentAssociationQuery();
        return _api.StudentAssociationAsync(
            iamid: query.IamId,
            iamids: query.IamIds,
            studentid: query.StudentId,
            majorcode: query.MajorCode,
            collegecode: query.CollegeCode,
            count: query.Count,
            limit: query.Limit,
            offset: query.Offset,
            modifiedsince: query.ModifiedSince,
            cancellationToken: cancellationToken);
    }
}

public sealed class ReferenceDataClient
{
    private readonly IClient _api;

    public ReferenceDataClient(IClient api)
    {
        _api = api ?? throw new ArgumentNullException(nameof(api));
    }

    public Task<ICollection<College>> GetCollegesAsync(string? collegeCode = null, string? collegeTitle = null, CancellationToken cancellationToken = default) =>
        _api.CollegesAsync(collegeCode, collegeTitle, cancellationToken);

    public Task<ICollection<Major>> GetMajorsAsync(string? majorCode = null, string? majorTitle = null, string? majorStatus = null, CancellationToken cancellationToken = default) =>
        _api.MajorsAsync(majorCode, majorTitle, majorStatus, cancellationToken);
}

public sealed class CampaignContactsClient
{
    private readonly IClient _api;

    public CampaignContactsClient(IClient api)
    {
        _api = api ?? throw new ArgumentNullException(nameof(api));
    }

    public Task<FileResponse> GetCsvAsync(int? limit = null, bool? save = null, CancellationToken cancellationToken = default) =>
        _api.CampaignContactsAsync(limit, save, cancellationToken);

    public Task<FileResponse> GetModifiedCsvAsync(string? modifiedSince = null, CancellationToken cancellationToken = default) =>
        _api.CampaignContactsModifiedAsync(modifiedSince, cancellationToken);
}
