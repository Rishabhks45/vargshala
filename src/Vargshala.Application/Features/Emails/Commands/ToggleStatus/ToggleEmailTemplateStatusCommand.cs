using MediatR;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.Emails.Commands.ToggleStatus;

public record ToggleEmailTemplateStatusCommand(Guid TemplateId) : IRequest<ApiResponse<bool>>;
