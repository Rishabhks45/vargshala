using Microsoft.EntityFrameworkCore;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Abstractions.Persistence;
using Vargshala.Application.Features.Messages.Security;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Messages.Enums;
using Vargshala.Domain.Entities;

namespace Vargshala.Infrastructure.Services.Messages;

/// <summary>
/// Source of Truth Communication Authorization Matrix implementation.
/// Strictly enforces backend permissions for messaging, group creation, administration, and user discovery.
/// </summary>
public class ConversationAuthorizationService : IConversationAuthorizationService
{
    private readonly IVargshalaDbContext _db;
    private readonly ICurrentUser _currentUser;

    public ConversationAuthorizationService(
        IVargshalaDbContext db,
        ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<bool> CanMessageUserAsync(
        Guid currentUserId, 
        Guid targetUserId, 
        CancellationToken cancellationToken = default)
    {
        if (currentUserId == targetUserId || currentUserId == Guid.Empty || targetUserId == Guid.Empty)
        {
            return false;
        }

        var caller = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == currentUserId && !u.IsDeleted && u.IsActive, cancellationToken);

        var target = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == targetUserId && !u.IsDeleted && u.IsActive, cancellationToken);

        if (caller == null || target == null)
        {
            return false;
        }

        // Multi-Tenancy Invariant: Both users must belong to the same Organization. Other organization is ALWAYS DENIED.
        if (!caller.OrganizationId.HasValue || !target.OrganizationId.HasValue || caller.OrganizationId.Value != target.OrganizationId.Value)
        {
            return false;
        }

        var orgId = caller.OrganizationId.Value;

        // OrganizationAdmin has organization-wide communication permission across all branches
        if (caller.Role == UserRole.SuperAdmin || caller.Role == UserRole.OrganizationAdmin)
        {
            return true;
        }

        // BranchAdmin communication rules
        if (caller.Role == UserRole.BranchAdmin)
        {
            // BranchAdmin -> OrganizationAdmin (Allowed if same organization)
            if (target.Role == UserRole.OrganizationAdmin || target.Role == UserRole.SuperAdmin)
            {
                return true;
            }

            var callerBranchIds = await GetUserBranchIdsAsync(currentUserId, cancellationToken);
            if (!callerBranchIds.Any())
            {
                return false;
            }

            if (target.Role == UserRole.Teacher)
            {
                // BranchAdmin -> Teacher (Allowed only for teachers belonging to BranchAdmin's authorized branch)
                return await DoesTeacherBelongToBranchesAsync(targetUserId, callerBranchIds, cancellationToken);
            }

            if (target.Role == UserRole.Student)
            {
                // BranchAdmin -> Student (Allowed only for students belonging to BranchAdmin's authorized branch)
                return await DoesStudentBelongToBranchesAsync(targetUserId, callerBranchIds, cancellationToken);
            }

            // BranchAdmin -> Other Branch or unauthorized role: DENIED
            return false;
        }

        // Teacher communication rules
        if (caller.Role == UserRole.Teacher)
        {
            if (target.Role == UserRole.Teacher)
            {
                // Teacher -> Teacher (Allowed when both teachers belong to the SAME authorized branch)
                var callerBranches = await GetTeacherBranchIdsAsync(currentUserId, cancellationToken);
                var targetBranches = await GetTeacherBranchIdsAsync(targetUserId, cancellationToken);

                return callerBranches.Intersect(targetBranches).Any();
            }

            if (target.Role == UserRole.Student)
            {
                // Teacher -> Student (Allowed ONLY when actively assigned to student's batch: Teacher -> BatchTeachers -> Batch -> BatchStudents -> Student)
                return await (
                    from bt in _db.BatchTeachers.AsNoTracking()
                    join bs in _db.BatchStudents.AsNoTracking() on bt.BatchId equals bs.BatchId
                    where bt.Teacher.UserId == currentUserId
                      && bs.Student.UserId == targetUserId
                      && bt.IsActive && !bt.Batch.IsDeleted && bt.Batch.IsActive
                      && bs.IsActive && !bs.Student.IsDeleted && bs.Student.IsActive
                    select bt.BatchId
                ).AnyAsync(cancellationToken);
            }

            return false;
        }

        // Student communication rules
        if (caller.Role == UserRole.Student)
        {
            if (target.Role == UserRole.Student)
            {
                // Student -> Student (Allowed ONLY when both students share at least one ACTIVE batch)
                return await (
                    from bs1 in _db.BatchStudents.AsNoTracking()
                    join bs2 in _db.BatchStudents.AsNoTracking() on bs1.BatchId equals bs2.BatchId
                    where bs1.Student.UserId == currentUserId
                      && bs2.Student.UserId == targetUserId
                      && bs1.IsActive && !bs1.Batch.IsDeleted && bs1.Batch.IsActive
                      && bs2.IsActive && !bs2.Batch.IsDeleted && bs2.Batch.IsActive
                    select bs1.BatchId
                ).AnyAsync(cancellationToken);
            }

            if (target.Role == UserRole.Teacher)
            {
                // Student -> Teacher (Allowed ONLY if teacher is assigned to one of student's active batches)
                return await (
                    from bs in _db.BatchStudents.AsNoTracking()
                    join bt in _db.BatchTeachers.AsNoTracking() on bs.BatchId equals bt.BatchId
                    where bs.Student.UserId == currentUserId
                      && bt.Teacher.UserId == targetUserId
                      && bs.IsActive && !bs.Batch.IsDeleted && bs.Batch.IsActive
                      && bt.IsActive && !bt.Teacher.IsDeleted && bt.Teacher.IsActive
                    select bs.BatchId
                ).AnyAsync(cancellationToken);
            }

            return false;
        }

        return false;
    }

    public async Task<bool> CanCreateGroupAsync(
        Guid currentUserId, 
        IEnumerable<Guid> memberUserIds, 
        ConversationType groupType, 
        Guid? batchId, 
        Guid? branchId, 
        CancellationToken cancellationToken = default)
    {
        var caller = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == currentUserId && !u.IsDeleted && u.IsActive, cancellationToken);

        if (caller == null || !caller.OrganizationId.HasValue)
        {
            return false;
        }

        var distinctMembers = memberUserIds.Where(m => m != currentUserId).Distinct().ToList();

        // OrganizationAdmin can create groups across the organization
        if (caller.Role == UserRole.SuperAdmin || caller.Role == UserRole.OrganizationAdmin)
        {
            if (distinctMembers.Any())
            {
                var memberCountInOrg = await _db.Users
                    .AsNoTracking()
                    .CountAsync(u => distinctMembers.Contains(u.Id) && u.OrganizationId == caller.OrganizationId && !u.IsDeleted && u.IsActive, cancellationToken);

                if (memberCountInOrg != distinctMembers.Count)
                {
                    return false;
                }
            }
            return true;
        }

        // Student-created group rules:
        // Allowed members: Group creator, Students who share an ACTIVE batch with creator
        // Must NOT allow: Other-batch students, Unrelated teachers, Other branches, Other organizations
        if (caller.Role == UserRole.Student)
        {
            if (groupType != ConversationType.Group)
            {
                return false;
            }

            if (!distinctMembers.Any())
            {
                return true;
            }

            // Retrieve all active batch IDs for caller student
            var callerBatchIds = await _db.BatchStudents
                .AsNoTracking()
                .Where(bs => bs.Student.UserId == currentUserId && bs.IsActive && !bs.Batch.IsDeleted && bs.Batch.IsActive)
                .Select(bs => bs.BatchId)
                .Distinct()
                .ToListAsync(cancellationToken);

            if (!callerBatchIds.Any())
            {
                return false;
            }

            // Verify that every distinct member is a student and shares at least one of caller's active batches
            var validMemberCount = await _db.BatchStudents
                .AsNoTracking()
                .Where(bs => distinctMembers.Contains(bs.Student.UserId) 
                          && callerBatchIds.Contains(bs.BatchId) 
                          && bs.IsActive 
                          && !bs.Batch.IsDeleted 
                          && bs.Batch.IsActive)
                .Select(bs => bs.Student.UserId)
                .Distinct()
                .CountAsync(cancellationToken);

            return validMemberCount == distinctMembers.Count;
        }

        // Teacher-created group rules
        if (caller.Role == UserRole.Teacher)
        {
            if (groupType == ConversationType.BatchGroup)
            {
                // Teacher can create a batch group only for a batch assigned to that teacher
                if (!batchId.HasValue)
                {
                    return false;
                }

                var isTeacherAssigned = await _db.BatchTeachers
                    .AsNoTracking()
                    .AnyAsync(bt => bt.Teacher.UserId == currentUserId && bt.BatchId == batchId.Value && bt.IsActive && !bt.Batch.IsDeleted, cancellationToken);

                if (!isTeacherAssigned)
                {
                    return false;
                }

                // Members can be students belonging to that batch
                if (distinctMembers.Any())
                {
                    var validStudentsInBatch = await _db.BatchStudents
                        .AsNoTracking()
                        .Where(bs => bs.BatchId == batchId.Value && distinctMembers.Contains(bs.Student.UserId) && bs.IsActive)
                        .Select(bs => bs.Student.UserId)
                        .Distinct()
                        .CountAsync(cancellationToken);

                    if (validStudentsInBatch != distinctMembers.Count)
                    {
                        return false;
                    }
                }

                return true;
            }

            // Teacher group with teachers from the same authorized branch
            var teacherBranches = await GetTeacherBranchIdsAsync(currentUserId, cancellationToken);
            if (!teacherBranches.Any())
            {
                return false;
            }

            if (distinctMembers.Any())
            {
                foreach (var memberId in distinctMembers)
                {
                    var memberTeacherBranches = await GetTeacherBranchIdsAsync(memberId, cancellationToken);
                    if (!memberTeacherBranches.Intersect(teacherBranches).Any())
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        // BranchAdmin-created group rules
        if (caller.Role == UserRole.BranchAdmin)
        {
            var branchIds = await GetUserBranchIdsAsync(currentUserId, cancellationToken);
            if (!branchIds.Any())
            {
                return false;
            }

            foreach (var memberId in distinctMembers)
            {
                var isTeacherInBranch = await DoesTeacherBelongToBranchesAsync(memberId, branchIds, cancellationToken);
                var isStudentInBranch = await DoesStudentBelongToBranchesAsync(memberId, branchIds, cancellationToken);
                var isBranchAdmin = await _db.UserBranchAccesses
                    .AsNoTracking()
                    .AnyAsync(uba => uba.UserId == memberId && branchIds.Contains(uba.BranchId) && uba.IsActive, cancellationToken);

                if (!isTeacherInBranch && !isStudentInBranch && !isBranchAdmin)
                {
                    return false;
                }
            }

            return true;
        }

        return false;
    }

    public async Task<bool> CanAddParticipantAsync(
        Guid currentUserId, 
        Guid conversationId, 
        Guid targetUserId, 
        CancellationToken cancellationToken = default)
    {
        var isAdmin = await IsActiveGroupAdminAsync(currentUserId, conversationId, cancellationToken);
        if (!isAdmin)
        {
            return false;
        }

        // Check if target is already an active participant
        var isAlreadyParticipant = await _db.ConversationParticipants
            .AsNoTracking()
            .AnyAsync(cp => cp.ConversationId == conversationId && cp.UserId == targetUserId && cp.IsActive, cancellationToken);

        if (isAlreadyParticipant)
        {
            return false;
        }

        // Conversation type authorization rules
        var conversation = await _db.Conversations
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == conversationId && !c.IsDeleted && c.IsActive, cancellationToken);

        if (conversation == null)
        {
            return false;
        }

        if (conversation.Type == ConversationType.Direct)
        {
            return false; // Cannot add members to direct 1-on-1 chat
        }

        return await CanCreateGroupAsync(currentUserId, new[] { targetUserId }, conversation.Type, conversation.BatchId, conversation.BranchId, cancellationToken);
    }

    public async Task<bool> CanRemoveParticipantAsync(
        Guid currentUserId, 
        Guid conversationId, 
        Guid targetUserId, 
        CancellationToken cancellationToken = default)
    {
        var isAdmin = await IsActiveGroupAdminAsync(currentUserId, conversationId, cancellationToken);
        if (!isAdmin)
        {
            return false;
        }

        // Target must belong to the conversation
        return await _db.ConversationParticipants
            .AsNoTracking()
            .AnyAsync(cp => cp.ConversationId == conversationId && cp.UserId == targetUserId && cp.IsActive, cancellationToken);
    }

    public async Task<bool> CanPromoteAdminAsync(
        Guid currentUserId, 
        Guid conversationId, 
        Guid targetUserId, 
        CancellationToken cancellationToken = default)
    {
        var isCallerAdmin = await IsActiveGroupAdminAsync(currentUserId, conversationId, cancellationToken);
        if (!isCallerAdmin)
        {
            return false;
        }

        // Target must be an active participant
        var participant = await _db.ConversationParticipants
            .AsNoTracking()
            .FirstOrDefaultAsync(cp => cp.ConversationId == conversationId && cp.UserId == targetUserId && cp.IsActive, cancellationToken);

        if (participant == null)
        {
            return false;
        }

        // Cannot promote if already active admin
        var isAlreadyAdmin = await _db.ConversationAdmins
            .AsNoTracking()
            .AnyAsync(ca => ca.ConversationId == conversationId && ca.UserId == targetUserId && ca.IsActive, cancellationToken);

        return !isAlreadyAdmin;
    }

    public async Task<bool> CanChangeGroupPhotoAsync(
        Guid currentUserId, 
        Guid conversationId, 
        CancellationToken cancellationToken = default)
    {
        return await IsActiveGroupAdminAsync(currentUserId, conversationId, cancellationToken);
    }

    public async Task<bool> CanSendMessageAsync(
        Guid currentUserId, 
        Guid conversationId, 
        CancellationToken cancellationToken = default)
    {
        var caller = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == currentUserId && !u.IsDeleted && u.IsActive, cancellationToken);

        if (caller == null)
        {
            return false;
        }

        var conversation = await _db.Conversations
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == conversationId && !c.IsDeleted && c.IsActive, cancellationToken);

        if (conversation == null)
        {
            return false;
        }

        // Must be active participant
        var participant = await _db.ConversationParticipants
            .AsNoTracking()
            .FirstOrDefaultAsync(cp => cp.ConversationId == conversationId && cp.UserId == currentUserId && cp.IsActive, cancellationToken);

        if (participant == null && caller.Role != UserRole.SuperAdmin && caller.Role != UserRole.OrganizationAdmin)
        {
            return false;
        }

        // Direct conversation check
        if (conversation.Type == ConversationType.Direct)
        {
            return participant != null;
        }

        // Announcement / Broadcast channel check
        if (conversation.IsAnnouncement)
        {
            return await CanReplyToAnnouncementAsync(currentUserId, conversationId, cancellationToken);
        }

        return true;
    }

    public async Task<bool> CanReplyToAnnouncementAsync(
        Guid currentUserId, 
        Guid conversationId, 
        CancellationToken cancellationToken = default)
    {
        var conversation = await _db.Conversations
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == conversationId && !c.IsDeleted && c.IsActive, cancellationToken);

        if (conversation == null)
        {
            return false;
        }

        var caller = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == currentUserId && !u.IsDeleted && u.IsActive, cancellationToken);

        if (caller == null)
        {
            return false;
        }

        // Admins can always post in announcements
        if (caller.Role == UserRole.SuperAdmin || caller.Role == UserRole.OrganizationAdmin)
        {
            return true;
        }

        var isGroupAdmin = await IsActiveGroupAdminAsync(currentUserId, conversationId, cancellationToken);
        if (isGroupAdmin)
        {
            return true;
        }

        // If replies are disabled
        if (!conversation.AllowReplies)
        {
            return false;
        }

        if (conversation.WhoCanReply == WhoCanReply.AdminsOnly)
        {
            return false;
        }

        if (conversation.WhoCanReply == WhoCanReply.TeachersAndAdmins)
        {
            return caller.Role == UserRole.Teacher || caller.Role == UserRole.BranchAdmin;
        }

        if (conversation.WhoCanReply == WhoCanReply.Everyone)
        {
            return true;
        }

        // Check custom permission table
        return await _db.AnnouncementReplyPermissions
            .AsNoTracking()
            .AnyAsync(p => p.ConversationId == conversationId && p.Role == caller.Role && p.CanReply, cancellationToken);
    }

    public async Task<List<Guid>> GetEligibleDirectMessageRecipientUserIdsAsync(
        Guid currentUserId, 
        CancellationToken cancellationToken = default)
    {
        var caller = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == currentUserId && !u.IsDeleted && u.IsActive, cancellationToken);

        if (caller == null || !caller.OrganizationId.HasValue)
        {
            return new List<Guid>();
        }

        var orgId = caller.OrganizationId.Value;

        // OrganizationAdmin: Can message ANY user in own organization
        if (caller.Role == UserRole.SuperAdmin || caller.Role == UserRole.OrganizationAdmin)
        {
            return await _db.Users
                .AsNoTracking()
                .Where(u => u.OrganizationId == orgId && u.Id != currentUserId && !u.IsDeleted && u.IsActive)
                .Select(u => u.Id)
                .ToListAsync(cancellationToken);
        }

        // BranchAdmin: Can message same-branch teachers, same-branch students, and own-org OrganizationAdmin
        if (caller.Role == UserRole.BranchAdmin)
        {
            var branchIds = await GetUserBranchIdsAsync(currentUserId, cancellationToken);
            if (!branchIds.Any())
            {
                return new List<Guid>();
            }

            var orgAdmins = await _db.Users
                .AsNoTracking()
                .Where(u => u.OrganizationId == orgId && u.Role == UserRole.OrganizationAdmin && !u.IsDeleted && u.IsActive)
                .Select(u => u.Id)
                .ToListAsync(cancellationToken);

            var teachersInBranch = await (
                from uba in _db.UserBranchAccesses.AsNoTracking()
                join u in _db.Users.AsNoTracking() on uba.UserId equals u.Id
                where branchIds.Contains(uba.BranchId) && uba.IsActive && u.Role == UserRole.Teacher && !u.IsDeleted && u.IsActive
                select u.Id
            ).ToListAsync(cancellationToken);

            var studentsInBranch = await (
                from uba in _db.UserBranchAccesses.AsNoTracking()
                join u in _db.Users.AsNoTracking() on uba.UserId equals u.Id
                where branchIds.Contains(uba.BranchId) && uba.IsActive && u.Role == UserRole.Student && !u.IsDeleted && u.IsActive
                select u.Id
            ).ToListAsync(cancellationToken);

            return orgAdmins.Concat(teachersInBranch).Concat(studentsInBranch).Distinct().Where(id => id != currentUserId).ToList();
        }

        // Teacher: Same-branch teachers + actively assigned students
        if (caller.Role == UserRole.Teacher)
        {
            var teacherBranches = await GetTeacherBranchIdsAsync(currentUserId, cancellationToken);

            var sameBranchTeachers = await (
                from uba in _db.UserBranchAccesses.AsNoTracking()
                join u in _db.Users.AsNoTracking() on uba.UserId equals u.Id
                where teacherBranches.Contains(uba.BranchId) && uba.IsActive && u.Role == UserRole.Teacher && !u.IsDeleted && u.IsActive && u.Id != currentUserId
                select u.Id
            ).ToListAsync(cancellationToken);

            var assignedStudents = await (
                from bt in _db.BatchTeachers.AsNoTracking()
                join bs in _db.BatchStudents.AsNoTracking() on bt.BatchId equals bs.BatchId
                join s in _db.Students.AsNoTracking() on bs.StudentId equals s.Id
                where bt.Teacher.UserId == currentUserId
                  && bt.IsActive && !bt.Batch.IsDeleted && bt.Batch.IsActive
                  && bs.IsActive && !s.IsDeleted && s.IsActive
                select s.UserId
            ).ToListAsync(cancellationToken);

            return sameBranchTeachers.Concat(assignedStudents).Distinct().ToList();
        }

        // Student: Same-batch students + assigned teachers of active batches
        if (caller.Role == UserRole.Student)
        {
            var sameBatchStudents = await (
                from bs1 in _db.BatchStudents.AsNoTracking()
                join bs2 in _db.BatchStudents.AsNoTracking() on bs1.BatchId equals bs2.BatchId
                join s in _db.Students.AsNoTracking() on bs2.StudentId equals s.Id
                where bs1.Student.UserId == currentUserId
                  && bs1.IsActive && !bs1.Batch.IsDeleted && bs1.Batch.IsActive
                  && bs2.IsActive && !s.IsDeleted && s.IsActive
                  && s.UserId != currentUserId
                select s.UserId
            ).ToListAsync(cancellationToken);

            var assignedTeachers = await (
                from bs in _db.BatchStudents.AsNoTracking()
                join bt in _db.BatchTeachers.AsNoTracking() on bs.BatchId equals bt.BatchId
                join t in _db.Teachers.AsNoTracking() on bt.TeacherId equals t.Id
                where bs.Student.UserId == currentUserId
                  && bs.IsActive && !bs.Batch.IsDeleted && bs.Batch.IsActive
                  && bt.IsActive && !t.IsDeleted && t.IsActive
                select t.UserId
            ).ToListAsync(cancellationToken);

            return sameBatchStudents.Concat(assignedTeachers).Distinct().ToList();
        }

        return new List<Guid>();
    }

    #region Helper Private Methods

    private async Task<bool> IsActiveGroupAdminAsync(Guid userId, Guid conversationId, CancellationToken cancellationToken)
    {
        var caller = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted && u.IsActive, cancellationToken);

        if (caller != null && (caller.Role == UserRole.SuperAdmin || caller.Role == UserRole.OrganizationAdmin))
        {
            return true;
        }

        var isExplicitAdmin = await _db.ConversationAdmins
            .AsNoTracking()
            .AnyAsync(ca => ca.ConversationId == conversationId && ca.UserId == userId && ca.IsActive, cancellationToken);

        if (isExplicitAdmin)
        {
            return true;
        }

        var isParticipantAdmin = await _db.ConversationParticipants
            .AsNoTracking()
            .AnyAsync(cp => cp.ConversationId == conversationId 
                         && cp.UserId == userId 
                         && cp.IsActive 
                         && (cp.IsAdmin || cp.Role == ConversationParticipantRole.Admin), cancellationToken);

        return isParticipantAdmin;
    }

    private async Task<List<Guid>> GetUserBranchIdsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var branches = await _db.UserBranchAccesses
            .AsNoTracking()
            .Where(uba => uba.UserId == userId && uba.IsActive)
            .Select(uba => uba.BranchId)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (!branches.Any() && _currentUser.UserId == userId && _currentUser.BranchId.HasValue)
        {
            branches.Add(_currentUser.BranchId.Value);
        }

        return branches;
    }

    private async Task<List<Guid>> GetTeacherBranchIdsAsync(Guid teacherUserId, CancellationToken cancellationToken)
    {
        var ubaBranches = await GetUserBranchIdsAsync(teacherUserId, cancellationToken);

        var batchBranches = await (
            from bt in _db.BatchTeachers.AsNoTracking()
            join b in _db.Batches.AsNoTracking() on bt.BatchId equals b.Id
            join c in _db.Classes.AsNoTracking() on b.ClassId equals c.Id
            where bt.Teacher.UserId == teacherUserId && bt.IsActive && !b.IsDeleted
            select c.BranchId
        ).Distinct().ToListAsync(cancellationToken);

        return ubaBranches.Concat(batchBranches).Distinct().ToList();
    }

    private async Task<bool> DoesTeacherBelongToBranchesAsync(Guid teacherUserId, IEnumerable<Guid> branchIds, CancellationToken cancellationToken)
    {
        var teacherBranches = await GetTeacherBranchIdsAsync(teacherUserId, cancellationToken);
        return teacherBranches.Intersect(branchIds).Any();
    }

    private async Task<bool> DoesStudentBelongToBranchesAsync(Guid studentUserId, IEnumerable<Guid> branchIds, CancellationToken cancellationToken)
    {
        var ubaMatch = await _db.UserBranchAccesses
            .AsNoTracking()
            .AnyAsync(uba => uba.UserId == studentUserId && branchIds.Contains(uba.BranchId) && uba.IsActive, cancellationToken);

        if (ubaMatch)
        {
            return true;
        }

        return await (
            from bs in _db.BatchStudents.AsNoTracking()
            join b in _db.Batches.AsNoTracking() on bs.BatchId equals b.Id
            join c in _db.Classes.AsNoTracking() on b.ClassId equals c.Id
            where bs.Student.UserId == studentUserId && bs.IsActive && !b.IsDeleted && branchIds.Contains(c.BranchId)
            select c.BranchId
        ).AnyAsync(cancellationToken);
    }

    #endregion
}
