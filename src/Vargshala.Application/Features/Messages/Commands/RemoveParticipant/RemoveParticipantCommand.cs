using MediatR;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.Messages.Commands.RemoveParticipant;

public record RemoveParticipantCommand(Guid ConversationId, Guid TargetUserId) : IRequest<ApiResponse<bool>>;
