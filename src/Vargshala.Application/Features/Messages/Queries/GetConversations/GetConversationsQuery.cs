using MediatR;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Messages;

namespace Vargshala.Application.Features.Messages.Queries.GetConversations;

public record GetConversationsQuery(GetConversationsPagedRequest? Request = null) 
    : IRequest<ApiResponse<PagedResponse<ChatConversationDto>>>;
