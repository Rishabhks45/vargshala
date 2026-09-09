using Vargshala.Contracts.Common;
using Vargshala.Contracts.Subjects;

namespace Vargshala.Web.Services;

public interface ISubjectService
{
    Task<ApiResponse<PagedResponse<SubjectDto>>> GetSubjectsPagedAsync(
        PagedRequest? request = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<List<SubjectDto>>> GetAllActiveSubjectsAsync(
        CancellationToken cancellationToken = default);

    Task<ApiResponse<SubjectDto>> GetSubjectByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<SubjectDto>> CreateSubjectAsync(
        CreateSubjectRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<SubjectDto>> UpdateSubjectAsync(
        UpdateSubjectRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<bool>> DeleteSubjectAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<bool>> ToggleSubjectStatusAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
