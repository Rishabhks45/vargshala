using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Vargshala.Application.Abstractions.Persistence;
using Vargshala.Application.Common;
using Vargshala.Application.Features.OrgAdmin.Fees.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Fees;
using Vargshala.Domain.Entities;

namespace Vargshala.Infrastructure.Persistence.Repositories;

public class FeeRepository : IFeeRepository
{
    private readonly IVargshalaDbContext _db;

    public FeeRepository(IVargshalaDbContext db)
    {
        _db = db;
    }

    #region Search & Sort Mappings
    private static Func<string, Expression<Func<StudentFee, bool>>> SearchPredicate => term =>
    {
        var lowerTerm = $"%{term.ToLower()}%";
        return sf => (sf.Student != null && sf.Student.User != null && sf.Student.User.FirstName != null && EF.Functions.Like(sf.Student.User.FirstName.ToLower(), lowerTerm))
                  || (sf.Student != null && sf.Student.User != null && sf.Student.User.LastName != null && EF.Functions.Like(sf.Student.User.LastName.ToLower(), lowerTerm))
                  || (sf.Student != null && sf.Student.RollNumber != null && EF.Functions.Like(sf.Student.RollNumber.ToLower(), lowerTerm))
                  || (sf.Student != null && sf.Student.StudentCode != null && EF.Functions.Like(sf.Student.StudentCode.ToLower(), lowerTerm))
                  || (sf.FeeStructure != null && sf.FeeStructure.Name != null && EF.Functions.Like(sf.FeeStructure.Name.ToLower(), lowerTerm))
                  || (sf.Status != null && EF.Functions.Like(sf.Status.ToLower(), lowerTerm));
    };

    private static readonly Dictionary<string, Expression<Func<StudentFee, object>>> SortMappings = new(StringComparer.OrdinalIgnoreCase)
    {
        ["name"] = sf => sf.Student.User.FirstName,
        ["studentname"] = sf => sf.Student.User.FirstName,
        ["rollnumber"] = sf => sf.Student.RollNumber!,
        ["feestructure"] = sf => sf.FeeStructure.Name,
        ["feestructurename"] = sf => sf.FeeStructure.Name,
        ["total"] = sf => sf.FinalAmount,
        ["totalfee"] = sf => sf.FinalAmount,
        ["finalamount"] = sf => sf.FinalAmount,
        ["paid"] = sf => sf.PaidAmount,
        ["paidamount"] = sf => sf.PaidAmount,
        ["due"] = sf => (sf.FinalAmount - sf.PaidAmount),
        ["dueamount"] = sf => (sf.FinalAmount - sf.PaidAmount),
        ["status"] = sf => sf.Status,
        ["assignedat"] = sf => sf.AssignedAt,
        ["createdat"] = sf => sf.CreatedAt
    };
    #endregion

    public async Task<StudentFee?> GetStudentFeeByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.StudentFees
            .Include(sf => sf.Student).ThenInclude(s => s.User)
            .Include(sf => sf.Student).ThenInclude(s => s.BatchStudents).ThenInclude(bs => bs.Batch)
            .Include(sf => sf.FeeStructure).ThenInclude(fs => fs.Branch)
            .Include(sf => sf.FeeStructure).ThenInclude(fs => fs.Class)
            .Include(sf => sf.Installments)
            .Include(sf => sf.Discounts)
            .FirstOrDefaultAsync(sf => sf.Id == id && !sf.IsDeleted, ct);
    }

    public async Task<StudentFee?> GetStudentFeeDetailByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.StudentFees
            .AsNoTracking()
            .Include(sf => sf.Student).ThenInclude(s => s.User)
            .Include(sf => sf.Student).ThenInclude(s => s.BatchStudents).ThenInclude(bs => bs.Batch)
            .Include(sf => sf.FeeStructure).ThenInclude(fs => fs.Branch)
            .Include(sf => sf.FeeStructure).ThenInclude(fs => fs.Class)
            .Include(sf => sf.Installments)
            .Include(sf => sf.Discounts).ThenInclude(d => d.ApprovedByUser)
            .FirstOrDefaultAsync(sf => sf.Id == id && !sf.IsDeleted, ct);
    }

    public async Task<StudentFee?> GetActiveStudentFeeByStudentIdAsync(Guid studentId, CancellationToken ct = default)
    {
        return await _db.StudentFees
            .Include(sf => sf.Student).ThenInclude(s => s.User)
            .Include(sf => sf.FeeStructure).ThenInclude(fs => fs.Branch)
            .Include(sf => sf.FeeStructure).ThenInclude(fs => fs.Class)
            .Include(sf => sf.Installments)
            .Include(sf => sf.Discounts)
            .Where(sf => sf.StudentId == studentId && !sf.IsDeleted && sf.Status != "Cancelled")
            .OrderByDescending(sf => sf.AssignedAt)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<(List<StudentFee> Items, int TotalRecords)> GetPagedStudentFeesAsync(
        Guid orgId,
        PagedRequest request,
        Guid? branchId = null,
        Guid? classId = null,
        Guid? batchId = null,
        string? status = null,
        CancellationToken ct = default)
    {
        var query = _db.StudentFees
            .AsNoTracking()
            .Include(sf => sf.Student).ThenInclude(s => s.User)
            .Include(sf => sf.Student).ThenInclude(s => s.BatchStudents).ThenInclude(bs => bs.Batch)
            .Include(sf => sf.FeeStructure).ThenInclude(fs => fs.Branch)
            .Include(sf => sf.FeeStructure).ThenInclude(fs => fs.Class)
            .Include(sf => sf.Installments)
            .Where(sf => sf.OrganizationId == orgId && !sf.IsDeleted);

        if (branchId.HasValue && branchId.Value != Guid.Empty)
        {
            query = query.Where(sf => sf.FeeStructure.BranchId == branchId.Value);
        }

        if (classId.HasValue && classId.Value != Guid.Empty)
        {
            query = query.Where(sf => sf.FeeStructure.ClassId == classId.Value || sf.Student.ClassName == classId.Value.ToString());
        }

        if (batchId.HasValue && batchId.Value != Guid.Empty)
        {
            query = query.Where(sf => sf.Student.BatchStudents.Any(bs => bs.BatchId == batchId.Value));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(sf => sf.Status.ToLower() == status.Trim().ToLower());
        }

        return await query.ToPagedResultAsync(
            request,
            searchPredicate: SearchPredicate,
            sortMappings: SortMappings,
            defaultSortExpression: sf => sf.AssignedAt,
            defaultAscending: false,
            cancellationToken: ct);
    }

    public async Task<FeeStatisticsDto> GetFeeStatisticsAsync(Guid orgId, Guid? branchId = null, CancellationToken ct = default)
    {
        var query = _db.StudentFees
            .AsNoTracking()
            .Where(sf => sf.OrganizationId == orgId && !sf.IsDeleted && sf.Status != "Cancelled");

        if (branchId.HasValue && branchId.Value != Guid.Empty)
        {
            query = query.Where(sf => sf.FeeStructure.BranchId == branchId.Value);
        }

        var totalExpected = await query.SumAsync(sf => (decimal?)sf.FinalAmount, ct) ?? 0m;
        var totalCollected = await query.SumAsync(sf => (decimal?)sf.PaidAmount, ct) ?? 0m;
        var totalPending = Math.Max(0, totalExpected - totalCollected);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var overdueCount = await query.CountAsync(
            sf => sf.Status == "Overdue" ||
                  (sf.PaidAmount < sf.FinalAmount && sf.Installments.Any(i => i.DueDate < today && i.PaidAmount < i.Amount)),
            ct);

        return new FeeStatisticsDto
        {
            TotalExpected = totalExpected,
            TotalCollected = totalCollected,
            TotalPending = totalPending,
            OverdueStudentsCount = overdueCount
        };
    }

    public async Task<Payment?> GetPaymentReceiptByIdAsync(Guid paymentId, CancellationToken ct = default)
    {
        return await _db.Payments
            .AsNoTracking()
            .Include(p => p.Branch)
            .Include(p => p.Student).ThenInclude(s => s.User)
            .Include(p => p.CreatedByUser)
            .Include(p => p.Allocations).ThenInclude(a => a.FeeInstallment)
            .FirstOrDefaultAsync(p => p.Id == paymentId && !p.IsDeleted, ct);
    }

    public async Task<List<StudentLookupForFeeDto>> GetStudentsForFeeAssignmentAsync(
        Guid orgId,
        Guid? branchId = null,
        Guid? classId = null,
        CancellationToken ct = default)
    {
        var query = _db.Students
            .AsNoTracking()
            .Include(s => s.User)
                .ThenInclude(u => u.UserBranchAccesses)
            .Include(s => s.StudentFees)
                .ThenInclude(sf => sf.FeeStructure)
            .Include(s => s.BatchStudents)
                .ThenInclude(bs => bs.Batch)
                    .ThenInclude(b => b.Class)
                        .ThenInclude(c => c.Branch)
            .Where(s => s.User.OrganizationId == orgId && !s.IsDeleted);

        if (branchId.HasValue && branchId.Value != Guid.Empty)
        {
            query = query.Where(s =>
                s.BatchStudents.Any(bs => bs.IsActive && !bs.Batch.IsDeleted && !bs.Batch.Class.IsDeleted && bs.Batch.Class.BranchId == branchId.Value) ||
                s.User.UserBranchAccesses.Any(uba => uba.IsActive && uba.BranchId == branchId.Value));
        }

        if (classId.HasValue && classId.Value != Guid.Empty)
        {
            query = query.Where(s => s.BatchStudents.Any(bs => bs.IsActive && !bs.Batch.IsDeleted && bs.Batch.ClassId == classId.Value));
        }

        var students = await query.ToListAsync(ct);

        return students.Select(s =>
        {
            var activeFee = branchId.HasValue && branchId.Value != Guid.Empty
                ? s.StudentFees.FirstOrDefault(f => !f.IsDeleted && f.Status != "Cancelled" && (f.FeeStructure == null || f.FeeStructure.BranchId == branchId.Value))
                : s.StudentFees.FirstOrDefault(f => !f.IsDeleted && f.Status != "Cancelled");
            var pendingDue = activeFee != null ? Math.Max(0, activeFee.FinalAmount - activeFee.PaidAmount) : 0m;

            var relevantBatches = branchId.HasValue && branchId.Value != Guid.Empty
                ? s.BatchStudents.Where(bs => bs.IsActive && !bs.Batch.IsDeleted && !bs.Batch.Class.IsDeleted && bs.Batch.Class.BranchId == branchId.Value).ToList()
                : s.BatchStudents.Where(bs => bs.IsActive && !bs.Batch.IsDeleted && !bs.Batch.Class.IsDeleted).ToList();

            var primaryBatch = relevantBatches.FirstOrDefault(bs => bs.IsPrimary)
                               ?? relevantBatches.FirstOrDefault();
            var branch = primaryBatch?.Batch?.Class?.Branch;

            return new StudentLookupForFeeDto
            {
                StudentId = s.Id,
                UserId = s.UserId,
                FullName = $"{s.User?.FirstName} {s.User?.LastName}".Trim(),
                RollNumber = s.RollNumber,
                StudentCode = s.StudentCode,
                BranchId = branch?.Id ?? (branchId.HasValue && branchId.Value != Guid.Empty ? branchId.Value : Guid.Empty),
                BranchName = branch?.Name ?? string.Empty,
                ClassId = primaryBatch?.Batch?.ClassId,
                ClassName = primaryBatch?.Batch?.Class?.Name ?? s.ClassName,
                HasFeeAssigned = activeFee != null,
                PendingDue = pendingDue
            };
        }).OrderBy(s => s.FullName).ToList();
    }

    public async Task<string> GenerateReceiptNumberAsync(Guid orgId, CancellationToken ct = default)
    {
        var yearMonth = DateTime.UtcNow.ToString("yyyyMM");
        var prefix = $"REC-{yearMonth}-";

        var existingNumbers = await _db.Payments
            .IgnoreQueryFilters()
            .Where(p => p.OrganizationId == orgId && p.ReceiptNumber != null && p.ReceiptNumber.StartsWith(prefix))
            .Select(p => p.ReceiptNumber!)
            .ToListAsync(ct);

        var existingSet = new HashSet<string>(existingNumbers, StringComparer.OrdinalIgnoreCase);

        int maxSeq = 0;
        foreach (var r in existingNumbers)
        {
            if (r.Length > prefix.Length && int.TryParse(r.Substring(prefix.Length), out int parsed))
            {
                if (parsed > maxSeq) maxSeq = parsed;
            }
        }

        int nextSeq = Math.Max(existingNumbers.Count + 1, maxSeq + 1);
        string candidate = $"{prefix}{nextSeq:D4}";
        while (existingSet.Contains(candidate))
        {
            nextSeq++;
            candidate = $"{prefix}{nextSeq:D4}";
        }

        return candidate;
    }

    public async Task AddStudentFeeAsync(StudentFee fee, CancellationToken ct = default)
    {
        await _db.StudentFees.AddAsync(fee, ct);
    }

    public async Task AddPaymentAsync(Payment payment, CancellationToken ct = default)
    {
        await _db.Payments.AddAsync(payment, ct);
    }

    public void UpdateStudentFee(StudentFee fee)
    {
        _db.StudentFees.Update(fee);
    }

    public void UpdateInstallment(FeeInstallment installment)
    {
        _db.FeeInstallments.Update(installment);
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _db.SaveChangesAsync(ct);
    }
}
