using Vargshala.Contracts.Common;
using Vargshala.Contracts.Fees;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Features.OrgAdmin.Fees.Infrastructure;

public interface IFeeRepository
{
    Task<StudentFee?> GetStudentFeeByIdAsync(Guid id, CancellationToken ct = default);
    Task<StudentFee?> GetStudentFeeDetailByIdAsync(Guid id, CancellationToken ct = default);
    Task<StudentFee?> GetActiveStudentFeeByStudentIdAsync(Guid studentId, CancellationToken ct = default);
    Task<(List<StudentFee> Items, int TotalRecords)> GetPagedStudentFeesAsync(
        Guid orgId,
        PagedRequest request,
        Guid? branchId = null,
        Guid? classId = null,
        Guid? batchId = null,
        string? status = null,
        CancellationToken ct = default);
    Task<FeeStatisticsDto> GetFeeStatisticsAsync(Guid orgId, Guid? branchId = null, CancellationToken ct = default);
    Task<Payment?> GetPaymentReceiptByIdAsync(Guid paymentId, CancellationToken ct = default);
    Task<List<StudentLookupForFeeDto>> GetStudentsForFeeAssignmentAsync(
        Guid orgId,
        Guid? branchId = null,
        Guid? classId = null,
        CancellationToken ct = default);
    Task<string> GenerateReceiptNumberAsync(Guid orgId, CancellationToken ct = default);
    Task AddStudentFeeAsync(StudentFee fee, CancellationToken ct = default);
    Task AddPaymentAsync(Payment payment, CancellationToken ct = default);
    void UpdateStudentFee(StudentFee fee);
    void UpdateInstallment(FeeInstallment installment);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
