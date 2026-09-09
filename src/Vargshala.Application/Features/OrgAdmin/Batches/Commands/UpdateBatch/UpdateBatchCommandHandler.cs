using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.OrgAdmin.Batches.Infrastructure;
using Vargshala.Application.Features.OrgAdmin.Classes.Infrastructure;
using Vargshala.Application.Features.Subjects.Infrastructure;
using Vargshala.Contracts.Batches;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Batches.Commands.UpdateBatch;

public class UpdateBatchCommandHandler : IRequestHandler<UpdateBatchCommand, ApiResponse<BatchDto>>
{
    private readonly IBatchRepository _batchRepository;
    private readonly IClassRepository _classRepository;
    private readonly ISubjectRepository _subjectRepository;
    private readonly ICurrentUser _currentUser;

    public UpdateBatchCommandHandler(
        IBatchRepository batchRepository,
        IClassRepository classRepository,
        ISubjectRepository subjectRepository,
        ICurrentUser currentUser)
    {
        _batchRepository = batchRepository;
        _classRepository = classRepository;
        _subjectRepository = subjectRepository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<BatchDto>> Handle(UpdateBatchCommand command, CancellationToken cancellationToken)
    {
        var orgId = _currentUser.OrganizationId;
        if (!orgId.HasValue || orgId.Value == Guid.Empty)
        {
            return ApiResponse<BatchDto>.FailureResponse("No active organization context found.");
        }

        var req = command.Request;

        var batch = await _batchRepository.GetByIdForUpdateAsync(req.Id, cancellationToken);
        if (batch == null)
        {
            return ApiResponse<BatchDto>.FailureResponse("Batch not found.");
        }

        // Verify Class
        var classEntity = await _classRepository.GetByIdAsync(req.ClassId, cancellationToken);
        if (classEntity == null || classEntity.Branch?.OrganizationId != orgId.Value)
        {
            return ApiResponse<BatchDto>.FailureResponse("Class not found in your organization.");
        }

        // Verify Subject
        var subject = await _subjectRepository.GetByIdAsync(req.SubjectId, cancellationToken);
        if (subject == null || subject.OrganizationId != orgId.Value)
        {
            return ApiResponse<BatchDto>.FailureResponse("Subject not found in your organization.");
        }

        // Unique (ClassId, Code)
        var exists = await _batchRepository.ExistsByCodeAndClassAsync(req.Code.Trim(), req.ClassId, excludeId: req.Id, cancellationToken: cancellationToken);
        if (exists)
        {
            return ApiResponse<BatchDto>.FailureResponse($"A batch with code '{req.Code.Trim()}' already exists in this class.");
        }

        batch.ClassId = req.ClassId;
        batch.SubjectId = req.SubjectId;
        batch.Name = req.Name.Trim();
        batch.Code = req.Code.Trim().ToUpperInvariant();
        batch.StartTime = req.StartTime;
        batch.EndTime = req.EndTime;
        batch.IsActive = req.IsActive;
        batch.UpdatedAt = DateTime.UtcNow;
        batch.UpdatedBy = _currentUser.UserId;

        _batchRepository.Update(batch);
        await _batchRepository.SaveChangesAsync(cancellationToken);

        batch.Class = classEntity;
        batch.Subject = subject;
        return ApiResponse<BatchDto>.SuccessResponse(batch.ToDto(), "Batch updated successfully.");
    }
}
