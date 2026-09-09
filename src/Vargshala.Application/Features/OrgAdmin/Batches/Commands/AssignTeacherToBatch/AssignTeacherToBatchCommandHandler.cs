using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.OrgAdmin.Batches.Infrastructure;
using Vargshala.Application.Features.OrgAdmin.Teachers.Infrastructure;
using Vargshala.Contracts.Batches;
using Vargshala.Contracts.Common;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Features.OrgAdmin.Batches.Commands.AssignTeacherToBatch;

public class AssignTeacherToBatchCommandHandler : IRequestHandler<AssignTeacherToBatchCommand, ApiResponse<BatchTeacherDto>>
{
    private readonly IBatchRepository _batchRepository;
    private readonly ITeacherRepository _teacherRepository;
    private readonly ICurrentUser _currentUser;

    public AssignTeacherToBatchCommandHandler(
        IBatchRepository batchRepository,
        ITeacherRepository teacherRepository,
        ICurrentUser currentUser)
    {
        _batchRepository = batchRepository;
        _teacherRepository = teacherRepository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<BatchTeacherDto>> Handle(AssignTeacherToBatchCommand command, CancellationToken cancellationToken)
    {
        var batch = await _batchRepository.GetByIdAsync(command.BatchId, cancellationToken);
        if (batch == null)
        {
            return ApiResponse<BatchTeacherDto>.FailureResponse("Batch not found.");
        }

        var teacher = await _teacherRepository.GetByIdAsync(command.Request.TeacherId, cancellationToken);
        if (teacher == null)
        {
            return ApiResponse<BatchTeacherDto>.FailureResponse("Teacher not found.");
        }

        var existing = await _batchRepository.GetBatchTeacherAsync(command.BatchId, command.Request.TeacherId, cancellationToken);
        if (existing != null)
        {
            if (existing.IsActive)
            {
                return ApiResponse<BatchTeacherDto>.FailureResponse("Teacher is already assigned to this batch.");
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
            return ApiResponse<BatchTeacherDto>.SuccessResponse(existing.ToDto(), "Teacher assigned to batch successfully.");
        }

        var mapping = new BatchTeacher
        {
            Id = Guid.NewGuid(),
            BatchId = command.BatchId,
            TeacherId = command.Request.TeacherId,
            AssignedAt = DateTime.UtcNow,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = _currentUser.UserId
        };

        await _batchRepository.AddTeacherAsync(mapping, cancellationToken);
        await _batchRepository.SaveChangesAsync(cancellationToken);

        mapping.Teacher = teacher;
        return ApiResponse<BatchTeacherDto>.SuccessResponse(mapping.ToDto(), "Teacher assigned to batch successfully.");
    }
}
