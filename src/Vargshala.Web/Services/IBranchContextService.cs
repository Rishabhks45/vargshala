using Vargshala.Contracts.Branches;

namespace Vargshala.Web.Services;

public interface IBranchContextService
{
    Guid? CurrentBranchId { get; }
    BranchDto? CurrentBranch { get; }
    bool IsAllBranches { get; }
    bool HasMainBranch { get; }
    bool IsInitialized { get; }
    bool IsLoading { get; }
    IReadOnlyList<BranchDto> AvailableBranches { get; }

    event Func<Task>? OnBranchChanged;

    Task InitializeAsync(bool forceReload = false);
    Task SetBranchAsync(Guid? branchId);
    Task SetAllBranchesAsync();
    Task ResetAsync();
}
