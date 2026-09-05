using MediatR;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.EmailTemplates;

namespace Vargshala.Application.Features.Emails.Queries.GetEmailTemplates;

public record GetEmailTemplatesQuery(
    PagedRequest? Request = null,
    EmailTemplateCategory? Category = null,
    bool? IsActive = null) : IRequest<ApiResponse<PagedResponse<EmailTemplateDto>>>;
