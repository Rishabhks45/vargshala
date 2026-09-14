using Vargshala.Contracts.Common;
using Vargshala.Contracts.Fees;

namespace Vargshala.Web.Services;

public interface IFeeService
{
    Task<ApiResponse<PagedResponse<StudentFeeDto>>> GetStudentFeesPagedAsync(
        PagedRequest? request = null,
        Guid? branchId = null,
        Guid? classId = null,
        Guid? batchId = null,
        string? status = null,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<FeeStatisticsDto>> GetFeeStatisticsAsync(
        Guid? branchId = null,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<StudentFeeDetailDto>> GetStudentFeeDetailsAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<List<StudentLookupForFeeDto>>> GetStudentsLookupAsync(
        Guid? branchId = null,
        Guid? classId = null,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<StudentFeeDto>> AssignFeeAsync(
        AssignStudentFeeRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<PaymentDto>> CollectFeeAsync(
        CollectPaymentRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<PaymentDto>> GetPaymentReceiptAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default);

    Task<byte[]?> GetPaymentReceiptPdfAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default);

    Task<byte[]?> GetStudentFeeReceiptPdfAsync(
        Guid feeId,
        CancellationToken cancellationToken = default);
}
