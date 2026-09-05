using MediatR;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.EmailTemplates;

namespace Vargshala.Application.Features.Emails.Queries.GetEmailTemplateByCode;

public record GetEmailTemplateByCodeQuery(string Code) : IRequest<ApiResponse<EmailTemplateDto>>;
