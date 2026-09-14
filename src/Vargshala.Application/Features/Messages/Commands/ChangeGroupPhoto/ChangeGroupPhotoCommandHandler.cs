using MediatR;
using Microsoft.EntityFrameworkCore;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Abstractions.Persistence;
using Vargshala.Application.Features.Messages.Security;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Messages.Enums;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Features.Messages.Commands.ChangeGroupPhoto;

public class ChangeGroupPhotoCommandHandler : IRequestHandler<ChangeGroupPhotoCommand, ApiResponse<bool>>
{
    private readonly IVargshalaDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IConversationAuthorizationService _authService;

    public ChangeGroupPhotoCommandHandler(
        IVargshalaDbContext db,
        ICurrentUser currentUser,
        IConversationAuthorizationService authService)
    {
        _db = db;
        _currentUser = currentUser;
        _authService = authService;
    }

    public async Task<ApiResponse<bool>> Handle(ChangeGroupPhotoCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.UserId;
        if (currentUserId == Guid.Empty || !_currentUser.OrganizationId.HasValue)
        {
            return ApiResponse<bool>.FailureResponse("User identity could not be verified.");
        }

        var orgId = _currentUser.OrganizationId.Value;

        // Centralized Authorization Matrix check (Group Admin only)
        var canChange = await _authService.CanChangeGroupPhotoAsync(currentUserId, request.ConversationId, cancellationToken);
        if (!canChange)
        {
            return ApiResponse<bool>.FailureResponse("You do not have permission to change the group photo. Only Group Admins can perform this action.");
        }

        var conversation = await _db.Conversations
            .FirstOrDefaultAsync(c => c.Id == request.ConversationId && c.OrganizationId == orgId && !c.IsDeleted, cancellationToken);

        if (conversation == null)
        {
            return ApiResponse<bool>.FailureResponse("Conversation not found.");
        }

        // Update photo URL
        conversation.GroupPhotoUrl = request.GroupPhotoUrl?.Trim();

        // Fetch admin caller name
        var caller = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == currentUserId, cancellationToken);
        var callerName = caller != null ? $"{caller.FirstName} {caller.LastName}".Trim() : "An admin";

        // Create System Message
        var systemMessage = new Message
        {
            Id = Guid.NewGuid(),
            OrganizationId = orgId,
            ConversationId = request.ConversationId,
            SenderId = currentUserId,
            MessageType = MessageType.System,
            SystemEventType = SystemEventType.GroupPhotoChanged,
            SystemEventUserId = currentUserId,
            MessageText = $"{callerName} changed the group photo",
            SentAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            IsPinned = false,
            IsDeleted = false
        };
        await _db.Messages.AddAsync(systemMessage, cancellationToken);

        // Update conversation last message cache
        conversation.LastMessageId = systemMessage.Id;
        conversation.LastMessageAt = systemMessage.SentAt;
        conversation.LastMessageText = systemMessage.MessageText;
        conversation.LastMessageSenderId = currentUserId;

        await _db.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "Group photo updated successfully.");
    }
}
