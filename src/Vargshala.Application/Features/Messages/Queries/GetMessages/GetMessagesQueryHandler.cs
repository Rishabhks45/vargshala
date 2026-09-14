using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.Messages.Common;
using Vargshala.Application.Features.Messages.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Messages;

namespace Vargshala.Application.Features.Messages.Queries.GetMessages;

public class GetMessagesQueryHandler 
    : IRequestHandler<GetMessagesQuery, ApiResponse<PagedResponse<ChatMessageDto>>>
{
    private readonly IMessageRepository _messageRepository;
    private readonly ICurrentUser _currentUser;

    public GetMessagesQueryHandler(
        IMessageRepository messageRepository,
        ICurrentUser currentUser)
    {
        _messageRepository = messageRepository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<PagedResponse<ChatMessageDto>>> Handle(
        GetMessagesQuery query, 
        CancellationToken cancellationToken)
    {
        if (_currentUser.OrganizationId is null)
        {
            return ApiResponse<PagedResponse<ChatMessageDto>>.FailureResponse("No organization associated with this user.");
        }

        if (_currentUser.UserId == Guid.Empty)
        {
            return ApiResponse<PagedResponse<ChatMessageDto>>.FailureResponse("User identity could not be verified.");
        }

        var orgId = _currentUser.OrganizationId.Value;
        var currentUserId = _currentUser.UserId;
        var userRole = _currentUser.UserRole ?? UserRole.Student;

        // Verify conversation belongs to this organization
        var exists = await _messageRepository.ConversationExistsAsync(query.ConversationId, orgId, cancellationToken);
        if (!exists)
        {
            return ApiResponse<PagedResponse<ChatMessageDto>>.FailureResponse("Conversation was not found.");
        }

        // Check if user is participant or has admin access
        var isParticipant = await _messageRepository.IsParticipantAsync(query.ConversationId, currentUserId, cancellationToken);
        if (!isParticipant && userRole != UserRole.SuperAdmin && userRole != UserRole.OrganizationAdmin)
        {
            return ApiResponse<PagedResponse<ChatMessageDto>>.FailureResponse("You are not a participant in this conversation.");
        }

        var request = query.Request ?? new PagedRequest { PageNumber = 1, PageSize = 50 };

        var (messages, totalRecords) = await _messageRepository.GetMessagesPagedAsync(
            query.ConversationId,
            request,
            cancellationToken);

        // Messages fetched descending by sent time, reverse to display chronologically in chat window
        var dtos = messages
            .OrderBy(m => m.SentAt)
            .Select(m => m.ToDto(currentUserId))
            .ToList();

        var response = PagedResponse<ChatMessageDto>.Create(dtos, totalRecords, request.PageNumber, request.PageSize);
        return ApiResponse<PagedResponse<ChatMessageDto>>.SuccessResponse(response);
    }
}
