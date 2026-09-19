using MediatR;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Messages;

namespace Vargshala.Application.Features.Messages.Commands.RestoreMessage;

public record RestoreMessageCommand(Guid MessageId) 
    : IRequest<ApiResponse<ChatMessageDto>>;
