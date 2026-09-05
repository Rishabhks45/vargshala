using Vargshala.Contracts.Branches;
using Vargshala.Contracts.Common;

namespace Vargshala.Web.Services;

public interface IBranchService
{
    Task<ApiResponse<PagedResponse<BranchDto>>> GetBranchesPagedAsync(
        PagedRequest? request = null,
        string? city = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<List<BranchDto>>> GetAllActiveBranchesAsync(
        CancellationToken cancellationToken = default);

    Task<ApiResponse<BranchDto>> GetBranchByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<BranchDto>> CreateBranchAsync(
        CreateBranchRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<BranchDto>> UpdateBranchAsync(
        UpdateBranchRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<bool>> DeleteBranchAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<List<UserBranchAccessDto>>> GetUserBranchesAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<bool>> AssignUserBranchesAsync(
        AssignUserBranchesRequest request,
        CancellationToken cancellationToken = default);
}
