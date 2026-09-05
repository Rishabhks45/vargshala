using MediatR;
using Vargshala.Application.Features.Emails.Infrastructure;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.Emails.Commands.UpdateEmailTemplate;

public class UpdateEmailTemplateCommandHandler : IRequestHandler<UpdateEmailTemplateCommand, ApiResponse<bool>>
{
    private readonly IEmailTemplateRepository _emailTemplateRepository;

    public UpdateEmailTemplateCommandHandler(IEmailTemplateRepository emailTemplateRepository)
    {
        _emailTemplateRepository = emailTemplateRepository;
    }

    public async Task<ApiResponse<bool>> Handle(
        UpdateEmailTemplateCommand request,
        CancellationToken cancellationToken)
    {
        var model = request.Request;
        var entity = await _emailTemplateRepository.GetByIdForUpdateAsync(model.Id, cancellationToken);

        if (entity == null)
        {
            return ApiResponse<bool>.FailureResponse("Email template not found.");
        }

        entity.Name = model.Name;
        entity.Subject = model.Subject;
        entity.BodyHtml = model.BodyHtml;
        entity.Description = model.Description;
        entity.TargetRole = model.TargetRole;
        entity.IsActive = model.IsActive;
        entity.AvailablePlaceholders = model.AvailablePlaceholders != null && model.AvailablePlaceholders.Count > 0
            ? string.Join(",", model.AvailablePlaceholders)
            : null;
        entity.UpdatedAt = DateTime.UtcNow;

        await _emailTemplateRepository.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, $"Template '{entity.Name}' updated successfully.");
    }
}
