using Vargshala.Contracts.Classes;
using Vargshala.Contracts.Common;

namespace Vargshala.Web.Services;

public interface IClassService
{
    Task<ApiResponse<PagedResponse<ClassDto>>> GetClassesPagedAsync(
        PagedRequest? request = null,
        Guid? branchId = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<List<ClassLookupDto>>> GetAllActiveClassesAsync(
        Guid? branchId = null,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<ClassDto>> GetClassByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<ClassDto>> CreateClassAsync(
        CreateClassRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<ClassDto>> UpdateClassAsync(
        UpdateClassRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<bool>> DeleteClassAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<bool>> ToggleClassStatusAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
