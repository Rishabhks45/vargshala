using Vargshala.Contracts.Common;
using Vargshala.Contracts.Students;

namespace Vargshala.Web.Services;

public interface IStudentService
{
    Task<ApiResponse<PagedResponse<StudentDto>>> GetStudentsPagedAsync(
        PagedRequest? request = null,
        string? className = null,
        string? section = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<StudentDto>> GetStudentByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<StudentDto>> CreateStudentAsync(
        CreateStudentRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<StudentDto>> UpdateStudentAsync(
        UpdateStudentRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<bool>> DeleteStudentAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<GeneratedStudentCodeDto>?> GenerateStudentCodeAsync(
        CancellationToken cancellationToken = default);
}
