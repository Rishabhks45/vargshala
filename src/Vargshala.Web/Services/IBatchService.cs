using Vargshala.Contracts.Batches;
using Vargshala.Contracts.Common;

namespace Vargshala.Web.Services;

public interface IBatchService
{
    Task<ApiResponse<PagedResponse<BatchDto>>> GetBatchesPagedAsync(
        PagedRequest? request = null,
        Guid? branchId = null,
        Guid? classId = null,
        Guid? subjectId = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<List<BatchDto>>> GetAllActiveBatchesAsync(
        Guid? classId = null,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<BatchDetailDto>> GetBatchByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<BatchDto>> CreateBatchAsync(
        CreateBatchRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<BatchDto>> UpdateBatchAsync(
        Guid id,
        UpdateBatchRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<bool>> DeleteBatchAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<bool>> ToggleBatchStatusAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<List<BatchTeacherDto>>> GetBatchTeachersAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<bool>> AssignTeacherToBatchAsync(
        Guid id,
        AssignTeacherToBatchRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<bool>> RemoveTeacherFromBatchAsync(
        Guid id,
        Guid teacherId,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<List<BatchStudentDto>>> GetBatchStudentsAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<BatchStudentDto>> EnrollStudentToBatchAsync(
        Guid id,
        EnrollStudentToBatchRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<bool>> RemoveStudentFromBatchAsync(
        Guid id,
        Guid studentId,
        CancellationToken cancellationToken = default);
}
