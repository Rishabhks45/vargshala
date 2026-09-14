using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.Messages.Common;
using Vargshala.Application.Features.Messages.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Messages;
using Vargshala.Contracts.Messages.Enums;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Features.Messages.Commands.CreateConversation;

public class CreateConversationCommandHandler 
    : IRequestHandler<CreateConversationCommand, ApiResponse<ChatConversationDto>>
{
    private readonly IMessageRepository _messageRepository;
    private readonly ICurrentUser _currentUser;

    public CreateConversationCommandHandler(
        IMessageRepository messageRepository,
        ICurrentUser currentUser)
    {
        _messageRepository = messageRepository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<ChatConversationDto>> Handle(
        CreateConversationCommand command, 
        CancellationToken cancellationToken)
    {
        if (_currentUser.OrganizationId is null)
        {
            return ApiResponse<ChatConversationDto>.FailureResponse("You must belong to an organization to create conversations.");
        }

        if (_currentUser.UserId == Guid.Empty)
        {
            return ApiResponse<ChatConversationDto>.FailureResponse("User identity could not be verified.");
        }

        var orgId = _currentUser.OrganizationId.Value;
        var currentUserId = _currentUser.UserId;
        var req = command.Request;

        // If Direct Chat: check if direct conversation already exists between these two users
        if (req.Type == ConversationType.Direct)
        {
            var targetUserId = req.TargetUserId!.Value;
            if (targetUserId == currentUserId)
            {
                return ApiResponse<ChatConversationDto>.FailureResponse("You cannot start a direct conversation with yourself.");
            }

            var existing = await _messageRepository.FindDirectConversationAsync(orgId, currentUserId, targetUserId, cancellationToken);
            if (existing != null)
            {
                var existingDto = existing.ToDto(currentUserId);
                return ApiResponse<ChatConversationDto>.SuccessResponse(existingDto, "Existing conversation retrieved.");
            }

            var u1 = currentUserId.CompareTo(targetUserId) < 0 ? currentUserId : targetUserId;
            var u2 = currentUserId.CompareTo(targetUserId) < 0 ? targetUserId : currentUserId;

            var directConversation = new Conversation
            {
                Id = Guid.NewGuid(),
                OrganizationId = orgId,
                CreatedBy = currentUserId,
                Type = ConversationType.Direct,
                DirectUser1Id = u1,
                DirectUser2Id = u2,
                IsAnnouncement = false,
                AllowReplies = true,
                WhoCanReply = WhoCanReply.Everyone,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // Add both participants
            directConversation.Participants.Add(new ConversationParticipant
            {
                Id = Guid.NewGuid(),
                OrganizationId = orgId,
                ConversationId = directConversation.Id,
                UserId = currentUserId,
                Role = ConversationParticipantRole.Member,
                IsAdmin = false,
                JoinedAt = DateTime.UtcNow,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            directConversation.Participants.Add(new ConversationParticipant
            {
                Id = Guid.NewGuid(),
                OrganizationId = orgId,
                ConversationId = directConversation.Id,
                UserId = targetUserId,
                Role = ConversationParticipantRole.Member,
                IsAdmin = false,
                JoinedAt = DateTime.UtcNow,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            await _messageRepository.AddConversationAsync(directConversation, cancellationToken);
            await _messageRepository.SaveChangesAsync(cancellationToken);

            var createdDirect = await _messageRepository.GetConversationByIdAsync(directConversation.Id, cancellationToken);
            var directDto = (createdDirect ?? directConversation).ToDto(currentUserId);

            return ApiResponse<ChatConversationDto>.SuccessResponse(directDto, "Conversation started successfully.");
        }

        // Group / Announcement / Batch / Branch Conversation
        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            OrganizationId = orgId,
            BranchId = req.BranchId,
            BatchId = req.BatchId,
            CreatedBy = currentUserId,
            Type = req.Type,
            Name = req.Name,
            Description = req.Description,
            GroupPhotoUrl = req.GroupPhotoUrl,
            IsAnnouncement = req.IsAnnouncement,
            AllowReplies = req.AllowReplies,
            WhoCanReply = req.WhoCanReply,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Add creator as Admin
        conversation.Participants.Add(new ConversationParticipant
        {
            Id = Guid.NewGuid(),
            OrganizationId = orgId,
            ConversationId = conversation.Id,
            UserId = currentUserId,
            Role = ConversationParticipantRole.Admin,
            IsAdmin = true,
            JoinedAt = DateTime.UtcNow,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });

        conversation.Admins.Add(new ConversationAdmin
        {
            Id = Guid.NewGuid(),
            OrganizationId = orgId,
            ConversationId = conversation.Id,
            UserId = currentUserId,
            AssignedBy = currentUserId,
            AssignedAt = DateTime.UtcNow,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });

        // Add additional participants
        if (req.ParticipantUserIds != null && req.ParticipantUserIds.Any())
        {
            var distinctIds = req.ParticipantUserIds.Where(id => id != currentUserId).Distinct();
            foreach (var memberId in distinctIds)
            {
                conversation.Participants.Add(new ConversationParticipant
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = orgId,
                    ConversationId = conversation.Id,
                    UserId = memberId,
                    Role = ConversationParticipantRole.Member,
                    IsAdmin = false,
                    JoinedAt = DateTime.UtcNow,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        await _messageRepository.AddConversationAsync(conversation, cancellationToken);
        await _messageRepository.SaveChangesAsync(cancellationToken);

        var freshConversation = await _messageRepository.GetConversationWithParticipantsAsync(conversation.Id, cancellationToken);
        var dto = (freshConversation ?? conversation).ToDto(currentUserId);

        return ApiResponse<ChatConversationDto>.SuccessResponse(dto, "Conversation created successfully.");
    }
}
