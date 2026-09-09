using Mapster;
using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.Subjects.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Subjects;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Features.Subjects.Commands.CreateSubject;

public class CreateSubjectCommandHandler : IRequestHandler<CreateSubjectCommand, ApiResponse<SubjectDto>>
{
    private readonly ISubjectRepository _subjectRepository;
    private readonly ICurrentUser _currentUser;

    public CreateSubjectCommandHandler(
        ISubjectRepository subjectRepository,
        ICurrentUser currentUser)
    {
        _subjectRepository = subjectRepository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<SubjectDto>> Handle(
        CreateSubjectCommand command,
        CancellationToken cancellationToken)
    {
        if (_currentUser.OrganizationId is null)
        {
            return ApiResponse<SubjectDto>.FailureResponse("You must belong to an organization to create subjects.");
        }

        var orgId = _currentUser.OrganizationId.Value;
        var req = command.Request;

        var codeExists = await _subjectRepository.ExistsByCodeAndOrgAsync(req.Code.Trim(), orgId, cancellationToken: cancellationToken);
        if (codeExists)
        {
            return ApiResponse<SubjectDto>.FailureResponse($"A subject with code '{req.Code.Trim()}' already exists in your organization.");
        }

        var subject = new Subject
        {
            Id = Guid.NewGuid(),
            OrganizationId = orgId,
            Name = req.Name.Trim(),
            Code = req.Code.Trim().ToUpperInvariant(),
            Description = string.IsNullOrWhiteSpace(req.Description) ? null : req.Description.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = _currentUser.UserId
        };

        await _subjectRepository.AddAsync(subject, cancellationToken);
        await _subjectRepository.SaveChangesAsync(cancellationToken);

        var dto = subject.Adapt<SubjectDto>();
        return ApiResponse<SubjectDto>.SuccessResponse(dto, "Subject created successfully.");
    }
}
