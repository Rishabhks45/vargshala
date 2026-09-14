using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.Messages.Common;
using Vargshala.Application.Features.Messages.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Messages;

namespace Vargshala.Application.Features.Messages.Queries.GetConversations;

public class GetConversationsQueryHandler 
    : IRequestHandler<GetConversationsQuery, ApiResponse<PagedResponse<ChatConversationDto>>>
{
    private readonly IMessageRepository _messageRepository;
    private readonly ICurrentUser _currentUser;

    public GetConversationsQueryHandler(
        IMessageRepository messageRepository,
        ICurrentUser currentUser)
    {
        _messageRepository = messageRepository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<PagedResponse<ChatConversationDto>>> Handle(
        GetConversationsQuery query, 
        CancellationToken cancellationToken)
    {
        if (_currentUser.OrganizationId is null)
        {
            return ApiResponse<PagedResponse<ChatConversationDto>>.FailureResponse("No organization associated with this user.");
        }

        if (_currentUser.UserId == Guid.Empty)
        {
            return ApiResponse<PagedResponse<ChatConversationDto>>.FailureResponse("User identity could not be verified.");
        }

        var orgId = _currentUser.OrganizationId.Value;
        var currentUserId = _currentUser.UserId;
        var request = query.Request ?? new GetConversationsPagedRequest();

        var (conversations, totalRecords) = await _messageRepository.GetUserConversationsPagedAsync(
            orgId,
            currentUserId,
            request,
            request.Type,
            cancellationToken);

        var dtos = new List<ChatConversationDto>(conversations.Count);
        foreach (var c in conversations)
        {
            var dto = c.ToDto(currentUserId);
            dto.UnreadCount = await _messageRepository.GetUnreadCountAsync(c.Id, currentUserId, cancellationToken);
            dtos.Add(dto);
        }

        var response = PagedResponse<ChatConversationDto>.Create(dtos, totalRecords, request.PageNumber, request.PageSize);
        return ApiResponse<PagedResponse<ChatConversationDto>>.SuccessResponse(response);
    }
}
