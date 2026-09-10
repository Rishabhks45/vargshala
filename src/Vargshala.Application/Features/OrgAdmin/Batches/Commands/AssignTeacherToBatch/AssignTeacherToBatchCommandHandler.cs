using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.OrgAdmin.Batches.Infrastructure;
using Vargshala.Application.Features.OrgAdmin.Branches.Infrastructure;
using Vargshala.Application.Features.OrgAdmin.Teachers.Infrastructure;
using Vargshala.Application.Features.Subjects.Infrastructure;
using Vargshala.Contracts.Batches;
using Vargshala.Contracts.Common;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Features.OrgAdmin.Batches.Commands.AssignTeacherToBatch;

public class AssignTeacherToBatchCommandHandler : IRequestHandler<AssignTeacherToBatchCommand, ApiResponse<BatchTeacherDto>>
{
    private readonly IBatchRepository _batchRepository;
    private readonly ITeacherRepository _teacherRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly ISubjectRepository _subjectRepository;
    private readonly ICurrentUser _currentUser;

    public AssignTeacherToBatchCommandHandler(
        IBatchRepository batchRepository,
        ITeacherRepository teacherRepository,
        IBranchRepository branchRepository,
        ISubjectRepository subjectRepository,
        ICurrentUser currentUser)
    {
        _batchRepository = batchRepository;
        _teacherRepository = teacherRepository;
        _branchRepository = branchRepository;
        _subjectRepository = subjectRepository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<BatchTeacherDto>> Handle(AssignTeacherToBatchCommand command, CancellationToken cancellationToken)
    {
        var batch = await _batchRepository.GetByIdAsync(command.BatchId, cancellationToken);
        if (batch == null)
        {
            return ApiResponse<BatchTeacherDto>.FailureResponse("Batch not found.");
        }

        var teacher = await _teacherRepository.GetByIdWithUserAsync(command.Request.TeacherId, cancellationToken);
        if (teacher == null || teacher.IsDeleted || !teacher.IsActive || (teacher.User != null && (!teacher.User.IsActive || teacher.User.IsDeleted)))
        {
            return ApiResponse<BatchTeacherDto>.FailureResponse("Teacher not found or is inactive.");
        }

        // Branch authorization check: if teacher has specific branch access, ensure batch's branch is allowed
        if (batch.Class != null)
        {
            var userBranches = await _branchRepository.GetUserBranchesAsync(teacher.UserId, cancellationToken);
            if (userBranches.Any() && !userBranches.Any(ub => ub.BranchId == batch.Class.BranchId))
            {
                return ApiResponse<BatchTeacherDto>.FailureResponse("This teacher is not authorized for the batch's branch.");
            }
        }

        var targetSubjectId = command.Request.SubjectId ?? batch.SubjectId;
        Subject? subject = null;
        if (batch.Subject != null && batch.SubjectId == targetSubjectId)
        {
            subject = batch.Subject;
        }
        else
        {
            subject = await _subjectRepository.GetByIdAsync(targetSubjectId, cancellationToken);
            if (subject == null)
            {
                return ApiResponse<BatchTeacherDto>.FailureResponse("Subject not found.");
            }
        }

        var existing = await _batchRepository.GetBatchTeacherAsync(command.BatchId, command.Request.TeacherId, targetSubjectId, cancellationToken);
        if (existing != null)
        {
            if (existing.IsActive)
            {
                return ApiResponse<BatchTeacherDto>.FailureResponse("Teacher is already assigned to this subject in the batch.");
            }

            // Reactivate
            existing.IsActive = true;
            existing.AssignedAt = DateTime.UtcNow;
            existing.RemovedAt = null;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedBy = _currentUser.UserId;
            _batchRepository.UpdateTeacher(existing);
            await _batchRepository.SaveChangesAsync(cancellationToken);

            existing.Teacher = teacher;
            existing.Subject = subject;
            return ApiResponse<BatchTeacherDto>.SuccessResponse(existing.ToDto(), "Teacher assigned to batch successfully.");
        }

        var mapping = new BatchTeacher
        {
            Id = Guid.NewGuid(),
            BatchId = command.BatchId,
            TeacherId = command.Request.TeacherId,
            SubjectId = targetSubjectId,
            AssignedAt = DateTime.UtcNow,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = _currentUser.UserId
        };

        await _batchRepository.AddTeacherAsync(mapping, cancellationToken);
        await _batchRepository.SaveChangesAsync(cancellationToken);

        mapping.Teacher = teacher;
        mapping.Subject = subject;
        return ApiResponse<BatchTeacherDto>.SuccessResponse(mapping.ToDto(), "Teacher assigned to batch successfully.");
    }
}
