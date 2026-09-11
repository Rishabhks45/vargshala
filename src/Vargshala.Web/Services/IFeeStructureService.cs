using Vargshala.Contracts.Common;
using Vargshala.Contracts.FeeStructures;

namespace Vargshala.Web.Services;

public interface IFeeStructureService
{
    Task<ApiResponse<PagedResponse<FeeStructureDto>>> GetFeeStructuresPagedAsync(
        PagedRequest? request = null,
        Guid? branchId = null,
        Guid? classId = null,
        string? session = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<List<FeeStructureLookupDto>>> GetAllActiveFeeStructuresAsync(
        Guid? branchId = null,
        Guid? classId = null,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<FeeStructureDto>> GetFeeStructureByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<FeeStructureDto>> CreateFeeStructureAsync(
        CreateFeeStructureRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<FeeStructureDto>> UpdateFeeStructureAsync(
        UpdateFeeStructureRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<bool>> DeleteFeeStructureAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<bool>> ToggleFeeStructureStatusAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
