using MediatR;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Messages;

namespace Vargshala.Application.Features.Messages.Queries.GetMessages;

public record GetMessagesQuery(Guid ConversationId, PagedRequest? Request = null) 
    : IRequest<ApiResponse<PagedResponse<ChatMessageDto>>>;
