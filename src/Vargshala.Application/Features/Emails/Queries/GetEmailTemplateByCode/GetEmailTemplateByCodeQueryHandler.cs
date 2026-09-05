using MediatR;
using Vargshala.Application.Features.Emails.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.EmailTemplates;

namespace Vargshala.Application.Features.Emails.Queries.GetEmailTemplateByCode;

public class GetEmailTemplateByCodeQueryHandler : IRequestHandler<GetEmailTemplateByCodeQuery, ApiResponse<EmailTemplateDto>>
{
    private readonly IEmailTemplateRepository _emailTemplateRepository;

    public GetEmailTemplateByCodeQueryHandler(IEmailTemplateRepository emailTemplateRepository)
    {
        _emailTemplateRepository = emailTemplateRepository;
    }

    public async Task<ApiResponse<EmailTemplateDto>> Handle(
        GetEmailTemplateByCodeQuery request,
        CancellationToken cancellationToken)
    {
        var e = await _emailTemplateRepository.GetByCodeAsync(request.Code, cancellationToken);

        if (e == null)
        {
            return ApiResponse<EmailTemplateDto>.FailureResponse($"Email template '{request.Code}' not found.");
        }

        var dto = new EmailTemplateDto
        {
            Id = e.Id,
            Code = e.Code,
            Name = e.Name,
            Category = e.Category.GetDisplayName(),
            TargetRole = e.TargetRole,
            Subject = e.Subject,
            BodyHtml = e.BodyHtml,
            Description = e.Description,
            AvailablePlaceholders = string.IsNullOrWhiteSpace(e.AvailablePlaceholders)
                ? new List<string>()
                : e.AvailablePlaceholders.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList(),
            IsActive = e.IsActive,
            UpdatedAt = e.UpdatedAt ?? e.CreatedAt,
            UpdatedBy = "System"
        };

        return ApiResponse<EmailTemplateDto>.SuccessResponse(dto);
    }
}
