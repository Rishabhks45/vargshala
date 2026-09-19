using MediatR;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Messages;

namespace Vargshala.Application.Features.Messages.Commands.ToggleReaction;

public record ToggleMessageReactionCommand(Guid MessageId, string Emoji) 
    : IRequest<ApiResponse<ToggleReactionResultDto>>;

