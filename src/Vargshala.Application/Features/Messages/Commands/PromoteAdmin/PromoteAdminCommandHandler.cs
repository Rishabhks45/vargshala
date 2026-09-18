using MediatR;
using Microsoft.EntityFrameworkCore;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Abstractions.Persistence;
using Vargshala.Application.Features.Messages.Security;
using Vargshala.Contracts.Common;
using Vargshala.SharedKernel.Enums;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Features.Messages.Commands.PromoteAdmin;

public class PromoteAdminCommandHandler : IRequestHandler<PromoteAdminCommand, ApiResponse<bool>>
{
    private readonly IVargshalaDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IConversationAuthorizationService _authService;

    public PromoteAdminCommandHandler(
        IVargshalaDbContext db,
        ICurrentUser currentUser,
        IConversationAuthorizationService authService)
    {
        _db = db;
        _currentUser = currentUser;
        _authService = authService;
    }

    public async Task<ApiResponse<bool>> Handle(PromoteAdminCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.UserId;
        if (currentUserId == Guid.Empty || !_currentUser.OrganizationId.HasValue)
        {
            return ApiResponse<bool>.FailureResponse("User identity could not be verified.");
        }

        var orgId = _currentUser.OrganizationId.Value;

        // Centralized Authorization Matrix check
        var canPromote = await _authService.CanPromoteAdminAsync(currentUserId, request.ConversationId, request.TargetUserId, cancellationToken);
        if (!canPromote)
        {
            return ApiResponse<bool>.FailureResponse("You are not authorized to promote this user to Group Admin.");
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

        // Promote participant
        participant.IsAdmin = true;
        participant.Role = ConversationParticipantRole.Admin;

        // Add ConversationAdmin entry
        var adminEntry = new ConversationAdmin
        {
            Id = Guid.NewGuid(),
            OrganizationId = orgId,
            ConversationId = request.ConversationId,
            UserId = request.TargetUserId,
            AssignedBy = currentUserId,
            AssignedAt = DateTime.UtcNow,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        await _db.ConversationAdmins.AddAsync(adminEntry, cancellationToken);

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
            SystemEventType = SystemEventType.AdminPromoted,
            SystemEventUserId = currentUserId,
            TargetUserId = request.TargetUserId,
            MessageText = $"{callerName} promoted {targetName} to Group Admin",
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

        return ApiResponse<bool>.SuccessResponse(true, "User promoted to Group Admin successfully.");
    }
}
