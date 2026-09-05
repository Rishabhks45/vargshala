namespace Vargshala.Application.Features.OrgAdmin.Students.Helpers;

public record StudentCodeAndRollResult(string StudentCode, string RollNumber, int SequenceNumber);

public interface IStudentCodeGenerator
{
    /// <summary>
    /// Generates the next unique student code (e.g. STU-2600001) and roll number (e.g. 2600001)
    /// strictly on behalf of current user's organization for the current year.
    /// </summary>
    Task<StudentCodeAndRollResult> GenerateNextCodeAndRollAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates the next unique student code (format: STU-YY00001, e.g. STU-2600001) for the current organization.
    /// </summary>
    Task<string> GenerateNextCodeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether the specified student code already exists within the current organization.
    /// </summary>
    Task<bool> IsCodeTakenAsync(string studentCode, Guid? excludeStudentId = null, CancellationToken cancellationToken = default);
}
