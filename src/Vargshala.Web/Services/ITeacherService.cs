using Vargshala.Contracts.Common;
using Vargshala.Contracts.Teachers;

namespace Vargshala.Web.Services;

public interface ITeacherService
{
    Task<ApiResponse<PagedResponse<TeacherDto>>> GetTeachersPagedAsync(
        PagedRequest? request = null,
        string? department = null,
        string? designation = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<TeacherDto>> GetTeacherByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<TeacherDto>> CreateTeacherAsync(
        CreateTeacherRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<TeacherDto>> UpdateTeacherAsync(
        UpdateTeacherRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<bool>> DeleteTeacherAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<GeneratedTeacherCodeDto>?> GenerateTeacherCodeAsync(
        CancellationToken cancellationToken = default);
}
