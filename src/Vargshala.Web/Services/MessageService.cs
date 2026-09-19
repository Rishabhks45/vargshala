using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Messages;

namespace Vargshala.Web.Services;

public class MessageService : IMessageService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MessageService> _logger;

    public MessageService(
        IHttpClientFactory httpClientFactory,
        ILogger<MessageService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("VargshalaApi");
        _logger = logger;
    }

    public async Task<ApiResponse<PagedResponse<ChatConversationDto>>> GetConversationsAsync(
        GetConversationsPagedRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var req = request ?? new GetConversationsPagedRequest();
            var queryParams = $"?pageNumber={req.PageNumber}&pageSize={req.PageSize}&sortDirection={req.SortDirection}";
            if (!string.IsNullOrWhiteSpace(req.Search))
                queryParams += $"&search={Uri.EscapeDataString(req.Search)}";
            if (!string.IsNullOrWhiteSpace(req.SortBy))
                queryParams += $"&sortBy={Uri.EscapeDataString(req.SortBy)}";
            if (req.Type.HasValue)
                queryParams += $"&type={req.Type.Value}";
            if (req.BranchId.HasValue)
                queryParams += $"&branchId={req.BranchId.Value}";
            if (req.BatchId.HasValue)
                queryParams += $"&batchId={req.BatchId.Value}";
            if (req.IsAnnouncement.HasValue)
                queryParams += $"&isAnnouncement={req.IsAnnouncement.Value.ToString().ToLowerInvariant()}";

            var response = await _httpClient.GetAsync($"api/v1/messages/conversations{queryParams}", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResponse<ChatConversationDto>>>(cancellationToken: cancellationToken);
                return errorResponse ?? ApiResponse<PagedResponse<ChatConversationDto>>.FailureResponse("Failed to fetch conversations.");
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResponse<ChatConversationDto>>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<PagedResponse<ChatConversationDto>>.FailureResponse("Empty response from server.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching conversations");
            return ApiResponse<PagedResponse<ChatConversationDto>>.FailureResponse($"Network or server error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<ChatConversationDto>> CreateConversationAsync(
        CreateConversationRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/messages/conversations", request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = await response.Content.ReadFromJsonAsync<ApiResponse<ChatConversationDto>>(cancellationToken: cancellationToken);
                return errorResponse ?? ApiResponse<ChatConversationDto>.FailureResponse("Failed to create conversation.");
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<ChatConversationDto>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<ChatConversationDto>.FailureResponse("Empty response from server.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating conversation");
            return ApiResponse<ChatConversationDto>.FailureResponse($"Network or server error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<PagedResponse<ChatMessageDto>>> GetMessagesAsync(
        Guid conversationId,
        PagedRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var req = request ?? new PagedRequest { PageNumber = 1, PageSize = 50 };
            var queryParams = $"?pageNumber={req.PageNumber}&pageSize={req.PageSize}&sortDirection={req.SortDirection}";
            if (!string.IsNullOrWhiteSpace(req.Search))
                queryParams += $"&search={Uri.EscapeDataString(req.Search)}";
            if (!string.IsNullOrWhiteSpace(req.SortBy))
                queryParams += $"&sortBy={Uri.EscapeDataString(req.SortBy)}";

            var response = await _httpClient.GetAsync($"api/v1/messages/conversations/{conversationId}/messages{queryParams}", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResponse<ChatMessageDto>>>(cancellationToken: cancellationToken);
                return errorResponse ?? ApiResponse<PagedResponse<ChatMessageDto>>.FailureResponse("Failed to fetch messages.");
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResponse<ChatMessageDto>>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<PagedResponse<ChatMessageDto>>.FailureResponse("Empty response from server.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching messages for conversation {ConversationId}", conversationId);
            return ApiResponse<PagedResponse<ChatMessageDto>>.FailureResponse($"Network or server error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<ChatMessageDto>> SendMessageAsync(
        SendMessageRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/messages/send", request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = await response.Content.ReadFromJsonAsync<ApiResponse<ChatMessageDto>>(cancellationToken: cancellationToken);
                return errorResponse ?? ApiResponse<ChatMessageDto>.FailureResponse("Failed to send message.");
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<ChatMessageDto>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<ChatMessageDto>.FailureResponse("Empty response from server.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message to conversation {ConversationId}", request.ConversationId);
            return ApiResponse<ChatMessageDto>.FailureResponse($"Network or server error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> MarkAsReadAsync(
        Guid conversationId,
        Guid latestMessageId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsync($"api/v1/messages/conversations/{conversationId}/read/{latestMessageId}", null, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(cancellationToken: cancellationToken);
                return errorResponse ?? ApiResponse<bool>.FailureResponse("Failed to mark conversation as read.");
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<bool>.FailureResponse("Empty response from server.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking conversation {ConversationId} as read", conversationId);
            return ApiResponse<bool>.FailureResponse($"Network or server error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> PromoteAdminAsync(
        Guid conversationId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsync($"api/v1/messages/conversations/{conversationId}/admins/{userId}", null, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<bool>.FailureResponse("Failed to promote user to Group Admin.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error promoting admin in conversation {ConversationId}", conversationId);
            return ApiResponse<bool>.FailureResponse($"Network error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> RemoveParticipantAsync(
        Guid conversationId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/v1/messages/conversations/{conversationId}/participants/{userId}", cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<bool>.FailureResponse("Failed to remove participant.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing participant from conversation {ConversationId}", conversationId);
            return ApiResponse<bool>.FailureResponse($"Network error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> AddParticipantAsync(
        Guid conversationId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsync($"api/v1/messages/conversations/{conversationId}/participants/{userId}", null, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<bool>.FailureResponse("Failed to add participant.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding participant to conversation {ConversationId}", conversationId);
            return ApiResponse<bool>.FailureResponse($"Network error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> ChangeGroupPhotoAsync(
        Guid conversationId,
        string photoUrl,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new ChangeGroupPhotoRequest { ConversationId = conversationId, GroupPhotoUrl = photoUrl };
            var response = await _httpClient.PutAsJsonAsync($"api/v1/messages/conversations/{conversationId}/photo", request, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<bool>.FailureResponse("Failed to update group photo.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing photo for conversation {ConversationId}", conversationId);
            return ApiResponse<bool>.FailureResponse($"Network error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<PagedResponse<EligibleUserDto>>> GetEligibleRecipientsAsync(
        GetEligibleRecipientsRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var req = request ?? new GetEligibleRecipientsRequest();
            var queryParams = $"?pageNumber={req.PageNumber}&pageSize={req.PageSize}";
            if (!string.IsNullOrWhiteSpace(req.SearchTerm))
            {
                queryParams += $"&searchTerm={Uri.EscapeDataString(req.SearchTerm)}";
            }

            var response = await _httpClient.GetAsync($"api/v1/messages/recipients/eligible{queryParams}", cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResponse<EligibleUserDto>>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<PagedResponse<EligibleUserDto>>.FailureResponse("Empty response from server.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching eligible recipients");
            return ApiResponse<PagedResponse<EligibleUserDto>>.FailureResponse($"Network error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<ToggleReactionResultDto>> ToggleReactionAsync(
        Guid messageId,
        string emoji,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new ToggleMessageReactionRequest { Emoji = emoji };
            var response = await _httpClient.PostAsJsonAsync($"api/v1/messages/{messageId}/reactions", request, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<ToggleReactionResultDto>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<ToggleReactionResultDto>.FailureResponse("Failed to toggle reaction.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling reaction on message {MessageId}", messageId);
            return ApiResponse<ToggleReactionResultDto>.FailureResponse($"Network error: {ex.Message}");
        }
    }
}

