using MediatR;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.EmailTemplates;

namespace Vargshala.Application.Features.Emails.Commands.UpdateEmailTemplate;

public record UpdateEmailTemplateCommand(UpdateEmailTemplateRequest Request) : IRequest<ApiResponse<bool>>;
