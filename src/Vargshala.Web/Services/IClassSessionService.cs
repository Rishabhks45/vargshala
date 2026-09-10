using Vargshala.Contracts.ClassSessions;
using Vargshala.Contracts.Common;

namespace Vargshala.Web.Services;

public interface IClassSessionService
{
    Task<ApiResponse<PagedResponse<ClassSessionDto>>> GetClassSessionsPagedAsync(
        PagedRequest? request = null,
        Guid? branchId = null,
        Guid? classId = null,
        Guid? batchId = null,
        Guid? teacherId = null,
        DateOnly? fromDate = null,
        DateOnly? toDate = null,
        string? status = null,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<ClassSessionDto>> GetClassSessionByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<ClassSessionDto>> CreateClassSessionAsync(
        CreateClassSessionRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<ClassSessionDto>> UpdateClassSessionAsync(
        Guid id,
        UpdateClassSessionRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<bool>> DeleteClassSessionAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<bool>> CancelClassSessionAsync(
        Guid id,
        CancelClassSessionRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<bool>> CompleteClassSessionAsync(
        Guid id,
        CompleteClassSessionRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<int>> GenerateSessionsFromScheduleAsync(
        GenerateSessionsFromScheduleRequest request,
        CancellationToken cancellationToken = default);
}
