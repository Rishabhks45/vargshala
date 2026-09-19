using Microsoft.EntityFrameworkCore;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Abstractions.Persistence;
using Vargshala.Application.Abstractions.Security;
using Vargshala.Domain.Entities;

namespace Vargshala.Infrastructure.Services.Security;

public class BranchAuthorizationService : IBranchAuthorizationService
{
    private readonly IVargshalaDbContext _db;
    private readonly ICurrentUser _currentUser;

    public BranchAuthorizationService(
        IVargshalaDbContext db,
        ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<BranchAuthorizationResult> ValidateBranchAccessAsync(CancellationToken cancellationToken = default)
    {
        var branchId = _currentUser.BranchId;
        var userId = _currentUser.UserId;
        var orgId = _currentUser.OrganizationId;

        if (!branchId.HasValue || branchId.Value == Guid.Empty ||
            userId == Guid.Empty ||
            !orgId.HasValue || orgId.Value == Guid.Empty)
        {
            return BranchAuthorizationResult.Unauthorized();
        }

        var activeAccesses = await _db.UserBranchAccesses
            .AsNoTracking()
            .Where(a => a.UserId == userId && a.IsActive && !a.Branch.IsDeleted && a.Branch.OrganizationId == orgId.Value)
            .ToListAsync(cancellationToken);

        if (activeAccesses.Count == 0)
        {
            return BranchAuthorizationResult.Forbidden("Access Denied: No active branch assignment found for this account. Please contact your Institute Administrator.");
        }

        if (activeAccesses.Count > 1)
        {
            return BranchAuthorizationResult.Conflict("Configuration Conflict: Multiple active branch assignments detected for this Branch Admin account. Exactly one active branch is permitted.");
        }

        if (activeAccesses[0].BranchId != branchId.Value)
        {
            return BranchAuthorizationResult.Forbidden("Access Denied: Token branch identity does not match current active branch assignment.");
        }

        return BranchAuthorizationResult.Success(branchId.Value);
    }

    public async Task<bool> CanAccessClassAsync(Guid classId, Guid branchId, CancellationToken cancellationToken = default)
    {
        return await _db.Classes
            .AsNoTracking()
            .AnyAsync(c => c.Id == classId && c.BranchId == branchId && !c.IsDeleted, cancellationToken);
    }

    public async Task<bool> CanAccessBatchAsync(Guid batchId, Guid branchId, CancellationToken cancellationToken = default)
    {
        return await _db.Batches
            .AsNoTracking()
            .AnyAsync(b => b.Id == batchId && b.Class != null && b.Class.BranchId == branchId && !b.IsDeleted, cancellationToken);
    }

    public async Task<bool> CanAccessClassSessionAsync(Guid sessionId, Guid branchId, CancellationToken cancellationToken = default)
    {
        return await _db.ClassSessions
            .AsNoTracking()
            .AnyAsync(cs => cs.Id == sessionId && cs.Batch != null && cs.Batch.Class != null && cs.Batch.Class.BranchId == branchId && !cs.IsDeleted, cancellationToken);
    }

    public async Task<bool> CanAccessStudentAsync(Guid studentId, Guid branchId, CancellationToken cancellationToken = default)
    {
        return await _db.Students
            .AsNoTracking()
            .AnyAsync(s => s.Id == studentId && !s.IsDeleted && s.BatchStudents.Any(bs => bs.IsActive && bs.Batch.Class.BranchId == branchId), cancellationToken);
    }

    public async Task<bool> CanAccessTeacherAsync(Guid teacherId, Guid branchId, CancellationToken cancellationToken = default)
    {
        var teacher = await _db.Teachers
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == teacherId && !t.IsDeleted, cancellationToken);

        if (teacher == null) return false;

        var hasDirectBranchAccess = await _db.UserBranchAccesses
            .AsNoTracking()
            .AnyAsync(uba => uba.UserId == teacher.UserId && uba.BranchId == branchId && uba.IsActive && !uba.Branch.IsDeleted, cancellationToken);

        if (hasDirectBranchAccess) return true;

        var hasBatchInBranch = await _db.BatchTeachers
            .AsNoTracking()
            .AnyAsync(bt => bt.TeacherId == teacherId && bt.IsActive && bt.Batch.Class.BranchId == branchId && !bt.Batch.IsDeleted, cancellationToken);

        if (hasBatchInBranch) return true;

        // If no explicit branch access entries exist at all for teacher, consider visible in institute
        var hasAnyBranchAccess = await _db.UserBranchAccesses
            .AsNoTracking()
            .AnyAsync(uba => uba.UserId == teacher.UserId && uba.IsActive, cancellationToken);

        return !hasAnyBranchAccess;
    }

    public async Task EnsureTeacherBranchAccessAsync(Guid teacherId, Guid branchId, CancellationToken cancellationToken = default)
    {
        var teacher = await _db.Teachers
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == teacherId && !t.IsDeleted, cancellationToken);

        if (teacher == null) return;

        var accessExists = await _db.UserBranchAccesses
            .AnyAsync(uba => uba.UserId == teacher.UserId && uba.BranchId == branchId && !uba.Branch.IsDeleted, cancellationToken);

        if (!accessExists)
        {
            await _db.UserBranchAccesses.AddAsync(new UserBranchAccess
            {
                Id = Guid.NewGuid(),
                UserId = teacher.UserId,
                BranchId = branchId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }, cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> CanAccessFeeStructureAsync(Guid feeStructureId, Guid branchId, CancellationToken cancellationToken = default)
    {
        return await _db.FeeStructures
            .AsNoTracking()
            .AnyAsync(fs => fs.Id == feeStructureId && fs.BranchId == branchId && !fs.IsDeleted, cancellationToken);
    }

    public async Task<bool> CanAccessStudentFeeAsync(Guid studentFeeId, Guid branchId, CancellationToken cancellationToken = default)
    {
        return await _db.StudentFees
            .AsNoTracking()
            .AnyAsync(sf => sf.Id == studentFeeId && !sf.IsDeleted &&
                ((sf.FeeStructure != null && sf.FeeStructure.BranchId == branchId) ||
                 sf.Student.BatchStudents.Any(bs => bs.IsActive && bs.Batch.Class.BranchId == branchId) ||
                 sf.Student.User.UserBranchAccesses.Any(uba => uba.IsActive && uba.BranchId == branchId)), cancellationToken);
    }
}
