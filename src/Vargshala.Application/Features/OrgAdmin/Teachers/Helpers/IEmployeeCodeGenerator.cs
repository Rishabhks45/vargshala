namespace Vargshala.Application.Features.OrgAdmin.Teachers.Helpers;

public interface IEmployeeCodeGenerator
{
    /// <summary>
    /// Generates the next unique employee code (format: EMP-YY00001, e.g. EMP-2600001)
    /// strictly on behalf of current user's organization for the current year.
    /// </summary>
    Task<string> GenerateNextCodeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether the specified employee code already exists within the current organization.
    /// </summary>
    Task<bool> IsCodeTakenAsync(string employeeCode, Guid? excludeTeacherId = null, CancellationToken cancellationToken = default);
}
