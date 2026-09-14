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
}
