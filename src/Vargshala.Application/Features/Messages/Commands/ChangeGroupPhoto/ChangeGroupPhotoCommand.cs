using MediatR;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.Messages.Commands.ChangeGroupPhoto;

public record ChangeGroupPhotoCommand(Guid ConversationId, string GroupPhotoUrl) : IRequest<ApiResponse<bool>>;
