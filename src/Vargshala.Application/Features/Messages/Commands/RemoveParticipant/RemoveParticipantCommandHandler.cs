using MediatR;
using Microsoft.EntityFrameworkCore;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Abstractions.Persistence;
using Vargshala.Application.Features.Messages.Security;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Messages.Enums;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Features.Messages.Commands.RemoveParticipant;

public class RemoveParticipantCommandHandler : IRequestHandler<RemoveParticipantCommand, ApiResponse<bool>>
{
    private readonly IVargshalaDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IConversationAuthorizationService _authService;

    public RemoveParticipantCommandHandler(
        IVargshalaDbContext db,
        ICurrentUser currentUser,
        IConversationAuthorizationService authService)
    {
        _db = db;
        _currentUser = currentUser;
        _authService = authService;
    }

    public async Task<ApiResponse<bool>> Handle(RemoveParticipantCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.UserId;
        if (currentUserId == Guid.Empty || !_currentUser.OrganizationId.HasValue)
        {
            return ApiResponse<bool>.FailureResponse("User identity could not be verified.");
        }

        var orgId = _currentUser.OrganizationId.Value;

        // Centralized Authorization Matrix check
        var canRemove = await _authService.CanRemoveParticipantAsync(currentUserId, request.ConversationId, request.TargetUserId, cancellationToken);
        if (!canRemove)
        {
            return ApiResponse<bool>.FailureResponse("You are not authorized to remove this participant.");
        }

        var conversation = await _db.Conversations
            .FirstOrDefaultAsync(c => c.Id == request.ConversationId && c.OrganizationId == orgId && !c.IsDeleted, cancellationToken);

        if (conversation == null)
        {
            return ApiResponse<bool>.FailureResponse("Conversation not found.");
        }

        var participant = await _db.ConversationParticipants
            .FirstOrDefaultAsync(cp => cp.ConversationId == request.ConversationId && cp.UserId == request.TargetUserId && cp.IsActive, cancellationToken);

        if (participant == null)
        {
            return ApiResponse<bool>.FailureResponse("Target user is not an active participant in this conversation.");
        }

        // Deactivate participant
        participant.IsActive = false;
        participant.LeftAt = DateTime.UtcNow;

        // Deactivate admin record if present
        var adminRecord = await _db.ConversationAdmins
            .FirstOrDefaultAsync(ca => ca.ConversationId == request.ConversationId && ca.UserId == request.TargetUserId && ca.IsActive, cancellationToken);
        if (adminRecord != null)
        {
            adminRecord.IsActive = false;
            adminRecord.RemovedAt = DateTime.UtcNow;
        }

        // Fetch names for system message
        var caller = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == currentUserId, cancellationToken);
        var target = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == request.TargetUserId, cancellationToken);
        var callerName = caller != null ? $"{caller.FirstName} {caller.LastName}".Trim() : "An admin";
        var targetName = target != null ? $"{target.FirstName} {target.LastName}".Trim() : "user";

        // Create System Message
        var systemMessage = new Message
        {
            Id = Guid.NewGuid(),
            OrganizationId = orgId,
            ConversationId = request.ConversationId,
            SenderId = currentUserId,
            MessageType = MessageType.System,
            SystemEventType = SystemEventType.MemberRemoved,
            SystemEventUserId = currentUserId,
            TargetUserId = request.TargetUserId,
            MessageText = $"{callerName} removed {targetName}",
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

        return ApiResponse<bool>.SuccessResponse(true, "Participant removed successfully.");
    }
}
