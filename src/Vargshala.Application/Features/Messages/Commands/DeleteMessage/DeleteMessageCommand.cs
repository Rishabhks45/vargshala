using MediatR;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.Messages.Commands.DeleteMessage;

public record DeleteMessageCommand(Guid MessageId) 
    : IRequest<ApiResponse<bool>>;
