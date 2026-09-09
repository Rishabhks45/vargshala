using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.OrgAdmin.Batches.Infrastructure;
using Vargshala.Application.Features.OrgAdmin.Students.Infrastructure;
using Vargshala.Contracts.Batches;
using Vargshala.Contracts.Common;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Features.OrgAdmin.Batches.Commands.EnrollStudentToBatch;

public class EnrollStudentToBatchCommandHandler : IRequestHandler<EnrollStudentToBatchCommand, ApiResponse<BatchStudentDto>>
{
    private readonly IBatchRepository _batchRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ICurrentUser _currentUser;

    public EnrollStudentToBatchCommandHandler(
        IBatchRepository batchRepository,
        IStudentRepository studentRepository,
        ICurrentUser currentUser)
    {
        _batchRepository = batchRepository;
        _studentRepository = studentRepository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<BatchStudentDto>> Handle(EnrollStudentToBatchCommand command, CancellationToken cancellationToken)
    {
        var batch = await _batchRepository.GetByIdAsync(command.BatchId, cancellationToken);
        if (batch == null)
        {
            return ApiResponse<BatchStudentDto>.FailureResponse("Batch not found.");
        }

        var student = await _studentRepository.GetByIdAsync(command.Request.StudentId, cancellationToken);
        if (student == null)
        {
            return ApiResponse<BatchStudentDto>.FailureResponse("Student not found.");
        }

        var existing = await _batchRepository.GetBatchStudentAsync(command.BatchId, command.Request.StudentId, cancellationToken);
        if (existing != null)
        {
            if (existing.IsActive)
            {
                return ApiResponse<BatchStudentDto>.FailureResponse("Student is already enrolled in this batch.");
            }

            // Re-enroll
            existing.IsActive = true;
            existing.IsPrimary = command.Request.IsPrimary;
            existing.EnrollmentType = string.IsNullOrWhiteSpace(command.Request.EnrollmentType) ? "Regular" : command.Request.EnrollmentType;
            existing.JoinedAt = DateTime.UtcNow;
            existing.LeftAt = null;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedBy = _currentUser.UserId;

            _batchRepository.UpdateStudent(existing);
            await _batchRepository.SaveChangesAsync(cancellationToken);

            existing.Student = student;
            return ApiResponse<BatchStudentDto>.SuccessResponse(existing.ToDto(), "Student re-enrolled in batch successfully.");
        }

        var mapping = new BatchStudent
        {
            Id = Guid.NewGuid(),
            BatchId = command.BatchId,
            StudentId = command.Request.StudentId,
            IsPrimary = command.Request.IsPrimary,
            EnrollmentType = string.IsNullOrWhiteSpace(command.Request.EnrollmentType) ? "Regular" : command.Request.EnrollmentType,
            JoinedAt = DateTime.UtcNow,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = _currentUser.UserId
        };

        await _batchRepository.AddStudentAsync(mapping, cancellationToken);
        await _batchRepository.SaveChangesAsync(cancellationToken);

        mapping.Student = student;
        return ApiResponse<BatchStudentDto>.SuccessResponse(mapping.ToDto(), "Student enrolled in batch successfully.");
    }
}
