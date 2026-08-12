using UCD.Rosetta.Client.Generated;

#pragma warning disable CS1591

namespace UCD.Rosetta.Client.Core.Domain;

public sealed class PeopleClient
{
    private readonly IClient _api;

    public PeopleClient(IClient api)
    {
        _api = api ?? throw new ArgumentNullException(nameof(api));
    }

    public Task<ICollection<Person>> SearchAsync(PeopleQuery? query = null, CancellationToken cancellationToken = default)
    {
        query ??= new PeopleQuery();

        return _api.PeopleGETAsync(
            modifiedsince: query.ModifiedSince,
            count: query.Count,
            limit: query.Limit,
            offset: query.Offset,
            firstname: query.FirstName,
            lastname: query.LastName,
            iamid: query.IamId,
            iamids: query.IamIds,
            manager_iam_id: query.ManagerIamId,
            email: query.Email,
            loginid: query.LoginId,
            employeeid: query.EmployeeId,
            studentid: query.StudentId,
            mailid: query.MailId,
            pidm: query.Pidm,
            mothraid: query.MothraId,
            pps_id: query.PpsId,
            cosmos_id: query.CosmosId,
            cpe_id: query.CpeId,
            health_affiliate_id: query.HealthAffiliateId,
            ucanr_id: query.UcanrId,
            usda_whnrc_id: query.UsdaWhnrcId,
            affiliate_id: query.AffiliateId,
            ucnet_id: query.UcnetId,
            affiliationContains: query.AffiliationContains,
            affiliationNotContains: query.AffiliationNotContains,
            affiliationState: RosettaDomainMapping.ParseEnumMember<AffiliationState>(query.AffiliationState),
            department: query.Department,
            collegecode: query.CollegeCode,
            majorcode: query.MajorCode,
            academic_level: RosettaDomainMapping.ParseEnumMember<Academic_level>(query.AcademicLevel),
            class_level: RosettaDomainMapping.ParseEnumMember<Class_level>(query.ClassLevel),
            organizationid: query.OrganizationId,
            divisionid: query.DivisionId,
            subdivisionid: query.SubdivisionId,
            subdivisionl4id: query.SubdivisionL4Id,
            employmentStatus: query.EmploymentStatus,
            cancellationToken: cancellationToken);
    }

    public Task<ICollection<Person>> GetBulkAsync(PeopleBulkQuery query, CancellationToken cancellationToken = default)
    {
        if (query == null)
            throw new ArgumentNullException(nameof(query));

        var iamIds = query.IamIds.Where(id => !string.IsNullOrWhiteSpace(id)).ToList();
        if (iamIds.Count == 0)
            throw new ArgumentException("At least one IAM ID is required.", nameof(query));

        return _api.PeoplePOSTAsync(new PeoplePostRequest
        {
            Iamids = iamIds,
            Limit = query.Limit,
            Offset = query.Offset,
            Count = query.Count
        }, cancellationToken);
    }

    public Task<ICollection<Person>> GetStudentsAsync(PeopleQuery? query = null, CancellationToken cancellationToken = default)
    {
        query ??= new PeopleQuery();

        return _api.StudentsAsync(
            modifiedsince: query.ModifiedSince,
            count: query.Count,
            limit: query.Limit,
            offset: query.Offset,
            firstname: query.FirstName,
            lastname: query.LastName,
            iamid: query.IamId,
            iamids: query.IamIds,
            manager_iam_id: query.ManagerIamId,
            email: query.Email,
            loginid: query.LoginId,
            employeeid: query.EmployeeId,
            studentid: query.StudentId,
            mailid: query.MailId,
            pidm: query.Pidm,
            mothraid: query.MothraId,
            pps_id: query.PpsId,
            cosmos_id: query.CosmosId,
            cpe_id: query.CpeId,
            health_affiliate_id: query.HealthAffiliateId,
            ucanr_id: query.UcanrId,
            usda_whnrc_id: query.UsdaWhnrcId,
            affiliate_id: query.AffiliateId,
            ucnet_id: query.UcnetId,
            affiliationContains: query.AffiliationContains,
            affiliationNotContains: query.AffiliationNotContains,
            affiliationState: RosettaDomainMapping.ParseEnumMember<AffiliationState2>(query.AffiliationState),
            department: query.Department,
            collegecode: query.CollegeCode,
            majorcode: query.MajorCode,
            academic_level: RosettaDomainMapping.ParseEnumMember<Academic_level2>(query.AcademicLevel),
            class_level: RosettaDomainMapping.ParseEnumMember<Class_level2>(query.ClassLevel),
            organizationid: query.OrganizationId,
            divisionid: query.DivisionId,
            subdivisionid: query.SubdivisionId,
            subdivisionl4id: query.SubdivisionL4Id,
            employmentStatus: query.EmploymentStatus,
            cancellationToken: cancellationToken);
    }

    public Task<ICollection<Person>> GetEmployeesAsync(PeopleQuery? query = null, CancellationToken cancellationToken = default)
    {
        query ??= new PeopleQuery();

        return _api.EmployeesAsync(
            modifiedsince: query.ModifiedSince,
            count: query.Count,
            limit: query.Limit,
            offset: query.Offset,
            firstname: query.FirstName,
            lastname: query.LastName,
            iamid: query.IamId,
            iamids: query.IamIds,
            manager_iam_id: query.ManagerIamId,
            email: query.Email,
            loginid: query.LoginId,
            employeeid: query.EmployeeId,
            studentid: query.StudentId,
            mailid: query.MailId,
            pidm: query.Pidm,
            mothraid: query.MothraId,
            pps_id: query.PpsId,
            cosmos_id: query.CosmosId,
            cpe_id: query.CpeId,
            health_affiliate_id: query.HealthAffiliateId,
            ucanr_id: query.UcanrId,
            usda_whnrc_id: query.UsdaWhnrcId,
            affiliate_id: query.AffiliateId,
            ucnet_id: query.UcnetId,
            affiliationContains: query.AffiliationContains,
            affiliationNotContains: query.AffiliationNotContains,
            affiliationState: RosettaDomainMapping.ParseEnumMember<AffiliationState3>(query.AffiliationState),
            department: query.Department,
            collegecode: query.CollegeCode,
            majorcode: query.MajorCode,
            academic_level: RosettaDomainMapping.ParseEnumMember<Academic_level3>(query.AcademicLevel),
            class_level: RosettaDomainMapping.ParseEnumMember<Class_level3>(query.ClassLevel),
            organizationid: query.OrganizationId,
            divisionid: query.DivisionId,
            subdivisionid: query.SubdivisionId,
            subdivisionl4id: query.SubdivisionL4Id,
            employmentStatus: query.EmploymentStatus,
            cancellationToken: cancellationToken);
    }

    public Task<ICollection<Person>> GetFacultyAsync(PeopleQuery? query = null, CancellationToken cancellationToken = default)
    {
        query ??= new PeopleQuery();

        return _api.FacultyAsync(
            modifiedsince: query.ModifiedSince,
            count: query.Count,
            limit: query.Limit,
            offset: query.Offset,
            firstname: query.FirstName,
            lastname: query.LastName,
            iamid: query.IamId,
            iamids: query.IamIds,
            manager_iam_id: query.ManagerIamId,
            email: query.Email,
            loginid: query.LoginId,
            employeeid: query.EmployeeId,
            studentid: query.StudentId,
            mailid: query.MailId,
            pidm: query.Pidm,
            mothraid: query.MothraId,
            pps_id: query.PpsId,
            cosmos_id: query.CosmosId,
            cpe_id: query.CpeId,
            health_affiliate_id: query.HealthAffiliateId,
            ucanr_id: query.UcanrId,
            usda_whnrc_id: query.UsdaWhnrcId,
            affiliate_id: query.AffiliateId,
            ucnet_id: query.UcnetId,
            affiliationContains: query.AffiliationContains,
            affiliationNotContains: query.AffiliationNotContains,
            affiliationState: RosettaDomainMapping.ParseEnumMember<AffiliationState4>(query.AffiliationState),
            department: query.Department,
            collegecode: query.CollegeCode,
            majorcode: query.MajorCode,
            academic_level: RosettaDomainMapping.ParseEnumMember<Academic_level4>(query.AcademicLevel),
            class_level: RosettaDomainMapping.ParseEnumMember<Class_level4>(query.ClassLevel),
            organizationid: query.OrganizationId,
            divisionid: query.DivisionId,
            subdivisionid: query.SubdivisionId,
            subdivisionl4id: query.SubdivisionL4Id,
            employmentStatus: query.EmploymentStatus,
            cancellationToken: cancellationToken);
    }

    public Task<ICollection<Person>> GetExternalAsync(PeopleQuery? query = null, CancellationToken cancellationToken = default)
    {
        query ??= new PeopleQuery();

        return _api.ExternalAsync(
            modifiedsince: query.ModifiedSince,
            count: query.Count,
            limit: query.Limit,
            offset: query.Offset,
            firstname: query.FirstName,
            lastname: query.LastName,
            iamid: query.IamId,
            iamids: query.IamIds,
            manager_iam_id: query.ManagerIamId,
            email: query.Email,
            loginid: query.LoginId,
            employeeid: query.EmployeeId,
            studentid: query.StudentId,
            mailid: query.MailId,
            pidm: query.Pidm,
            mothraid: query.MothraId,
            pps_id: query.PpsId,
            cosmos_id: query.CosmosId,
            cpe_id: query.CpeId,
            health_affiliate_id: query.HealthAffiliateId,
            ucanr_id: query.UcanrId,
            usda_whnrc_id: query.UsdaWhnrcId,
            affiliate_id: query.AffiliateId,
            ucnet_id: query.UcnetId,
            affiliationContains: query.AffiliationContains,
            affiliationNotContains: query.AffiliationNotContains,
            affiliationState: RosettaDomainMapping.ParseEnumMember<AffiliationState5>(query.AffiliationState),
            department: query.Department,
            collegecode: query.CollegeCode,
            majorcode: query.MajorCode,
            academic_level: RosettaDomainMapping.ParseEnumMember<Academic_level5>(query.AcademicLevel),
            class_level: RosettaDomainMapping.ParseEnumMember<Class_level5>(query.ClassLevel),
            organizationid: query.OrganizationId,
            divisionid: query.DivisionId,
            subdivisionid: query.SubdivisionId,
            subdivisionl4id: query.SubdivisionL4Id,
            employmentStatus: query.EmploymentStatus,
            cancellationToken: cancellationToken);
    }
}
