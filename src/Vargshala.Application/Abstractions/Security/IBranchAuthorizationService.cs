namespace Vargshala.Application.Abstractions.Security;

public record BranchAuthorizationResult(bool IsValid, Guid BranchId, string? ErrorMessage = null, int StatusCode = 200)
{
    public static BranchAuthorizationResult Success(Guid branchId) => new(true, branchId, null, 200);
    public static BranchAuthorizationResult Unauthorized(string message = "Unauthorized: Missing tenant or branch identity.") => new(false, Guid.Empty, message, 401);
    public static BranchAuthorizationResult Forbidden(string message) => new(false, Guid.Empty, message, 403);
    public static BranchAuthorizationResult Conflict(string message) => new(false, Guid.Empty, message, 409);
}

public interface IBranchAuthorizationService
{
    Task<BranchAuthorizationResult> ValidateBranchAccessAsync(CancellationToken cancellationToken = default);
    Task<bool> CanAccessClassAsync(Guid classId, Guid branchId, CancellationToken cancellationToken = default);
    Task<bool> CanAccessBatchAsync(Guid batchId, Guid branchId, CancellationToken cancellationToken = default);
    Task<bool> CanAccessClassSessionAsync(Guid sessionId, Guid branchId, CancellationToken cancellationToken = default);
    Task<bool> CanAccessStudentAsync(Guid studentId, Guid branchId, CancellationToken cancellationToken = default);
    Task<bool> CanAccessTeacherAsync(Guid teacherId, Guid branchId, CancellationToken cancellationToken = default);
    Task EnsureTeacherBranchAccessAsync(Guid teacherId, Guid branchId, CancellationToken cancellationToken = default);
    Task<bool> CanAccessFeeStructureAsync(Guid feeStructureId, Guid branchId, CancellationToken cancellationToken = default);
    Task<bool> CanAccessStudentFeeAsync(Guid studentFeeId, Guid branchId, CancellationToken cancellationToken = default);
}
