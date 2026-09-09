using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.OrgAdmin.Batches.Infrastructure;
using Vargshala.Application.Features.OrgAdmin.Classes.Infrastructure;
using Vargshala.Application.Features.Subjects.Infrastructure;
using Vargshala.Contracts.Batches;
using Vargshala.Contracts.Common;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Features.OrgAdmin.Batches.Commands.CreateBatch;

public class CreateBatchCommandHandler : IRequestHandler<CreateBatchCommand, ApiResponse<BatchDto>>
{
    private readonly IBatchRepository _batchRepository;
    private readonly IClassRepository _classRepository;
    private readonly ISubjectRepository _subjectRepository;
    private readonly ICurrentUser _currentUser;

    public CreateBatchCommandHandler(
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

    public async Task<ApiResponse<BatchDto>> Handle(CreateBatchCommand command, CancellationToken cancellationToken)
    {
        var orgId = _currentUser.OrganizationId;
        if (!orgId.HasValue || orgId.Value == Guid.Empty)
        {
            return ApiResponse<BatchDto>.FailureResponse("No active organization context found.");
        }

        var req = command.Request;

        // Verify Class exists and belongs to current organization
        var classEntity = await _classRepository.GetByIdAsync(req.ClassId, cancellationToken);
        if (classEntity == null || classEntity.Branch?.OrganizationId != orgId.Value)
        {
            return ApiResponse<BatchDto>.FailureResponse("Class not found in your organization.");
        }

        // Verify Subject exists and belongs to current organization
        var subject = await _subjectRepository.GetByIdAsync(req.SubjectId, cancellationToken);
        if (subject == null || subject.OrganizationId != orgId.Value)
        {
            return ApiResponse<BatchDto>.FailureResponse("Subject not found in your organization.");
        }

        // Unique (ClassId, Code)
        var exists = await _batchRepository.ExistsByCodeAndClassAsync(req.Code.Trim(), req.ClassId, cancellationToken: cancellationToken);
        if (exists)
        {
            return ApiResponse<BatchDto>.FailureResponse($"A batch with code '{req.Code.Trim()}' already exists in this class.");
        }

        var batch = new Batch
        {
            Id = Guid.NewGuid(),
            ClassId = req.ClassId,
            SubjectId = req.SubjectId,
            Name = req.Name.Trim(),
            Code = req.Code.Trim().ToUpperInvariant(),
            StartTime = req.StartTime,
            EndTime = req.EndTime,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = _currentUser.UserId
        };

        await _batchRepository.AddAsync(batch, cancellationToken);
        await _batchRepository.SaveChangesAsync(cancellationToken);

        batch.Class = classEntity;
        batch.Subject = subject;
        return ApiResponse<BatchDto>.SuccessResponse(batch.ToDto(), "Batch created successfully.");
    }
}
