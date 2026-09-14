using Vargshala.Contracts.Common;
using Vargshala.Contracts.Messages;

namespace Vargshala.Web.Services;

public interface IMessageService
{
    Task<ApiResponse<PagedResponse<ChatConversationDto>>> GetConversationsAsync(
        GetConversationsPagedRequest? request = null,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<ChatConversationDto>> CreateConversationAsync(
        CreateConversationRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<PagedResponse<ChatMessageDto>>> GetMessagesAsync(
        Guid conversationId,
        PagedRequest? request = null,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<ChatMessageDto>> SendMessageAsync(
        SendMessageRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<bool>> MarkAsReadAsync(
        Guid conversationId,
        Guid latestMessageId,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<bool>> PromoteAdminAsync(
        Guid conversationId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<bool>> RemoveParticipantAsync(
        Guid conversationId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<bool>> AddParticipantAsync(
        Guid conversationId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<bool>> ChangeGroupPhotoAsync(
        Guid conversationId,
        string photoUrl,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<PagedResponse<EligibleUserDto>>> GetEligibleRecipientsAsync(
        GetEligibleRecipientsRequest? request = null,
        CancellationToken cancellationToken = default);
}
