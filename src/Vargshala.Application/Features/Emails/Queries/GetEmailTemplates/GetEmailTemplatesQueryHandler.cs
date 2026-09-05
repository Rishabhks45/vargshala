using MediatR;
using Vargshala.Application.Features.Emails.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.EmailTemplates;

namespace Vargshala.Application.Features.Emails.Queries.GetEmailTemplates;

public class GetEmailTemplatesQueryHandler : IRequestHandler<GetEmailTemplatesQuery, ApiResponse<PagedResponse<EmailTemplateDto>>>
{
    private readonly IEmailTemplateRepository _emailTemplateRepository;

    public GetEmailTemplatesQueryHandler(IEmailTemplateRepository emailTemplateRepository)
    {
        _emailTemplateRepository = emailTemplateRepository;
    }

    public async Task<ApiResponse<PagedResponse<EmailTemplateDto>>> Handle(
        GetEmailTemplatesQuery request,
        CancellationToken cancellationToken)
    {
        var pagedRequest = request.Request ?? new PagedRequest();

        var (items, totalRecords) = await _emailTemplateRepository.GetTemplatesPagedAsync(
            pagedRequest,
            request.Category,
            request.IsActive,
            cancellationToken);

        var dtos = items.Select(e => new EmailTemplateDto
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
        }).ToList();

        var pagedResponse = PagedResponse<EmailTemplateDto>.Create(dtos, totalRecords, pagedRequest.PageNumber, pagedRequest.PageSize);
        return ApiResponse<PagedResponse<EmailTemplateDto>>.SuccessResponse(pagedResponse);
    }
}
