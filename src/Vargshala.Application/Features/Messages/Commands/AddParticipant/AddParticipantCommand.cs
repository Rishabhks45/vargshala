using MediatR;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.Messages.Commands.AddParticipant;

public record AddParticipantCommand(Guid ConversationId, Guid TargetUserId) : IRequest<ApiResponse<bool>>;
