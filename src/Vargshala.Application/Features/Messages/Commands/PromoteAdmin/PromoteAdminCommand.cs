using MediatR;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.Messages.Commands.PromoteAdmin;

public record PromoteAdminCommand(Guid ConversationId, Guid TargetUserId) : IRequest<ApiResponse<bool>>;
