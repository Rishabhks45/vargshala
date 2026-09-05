using MediatR;
using Vargshala.Application.Features.Emails.Infrastructure;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.Emails.Commands.ToggleStatus;

public class ToggleEmailTemplateStatusCommandHandler : IRequestHandler<ToggleEmailTemplateStatusCommand, ApiResponse<bool>>
{
    private readonly IEmailTemplateRepository _emailTemplateRepository;

    public ToggleEmailTemplateStatusCommandHandler(IEmailTemplateRepository emailTemplateRepository)
    {
        _emailTemplateRepository = emailTemplateRepository;
    }

    public async Task<ApiResponse<bool>> Handle(
        ToggleEmailTemplateStatusCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await _emailTemplateRepository.GetByIdForUpdateAsync(request.TemplateId, cancellationToken);

        if (entity == null)
        {
            return ApiResponse<bool>.FailureResponse("Email template not found.");
        }

        entity.IsActive = !entity.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _emailTemplateRepository.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(entity.IsActive, $"Template '{entity.Name}' is now {(entity.IsActive ? "Active" : "Disabled")}.");
    }
}
