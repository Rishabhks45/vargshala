using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Vargshala.Contracts.Messages;
using Vargshala.Web.Auth;

namespace Vargshala.Web.Services;

public class ChatHubService : IChatHubService
{
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ChatHubService> _logger;

    private HubConnection? _hubConnection;
    private bool _isDisposed;

    public event Func<ChatMessageDto, Task>? OnMessageReceived;
    public event Action<MessagesReadNotification>? OnMessagesRead;
    public event Action<string, string, bool>? OnUserTyping;
    public event Action<string, string, string, int>? OnReactionUpdated;
    public event Action<MessageDeletedNotification>? OnMessageDeleted;
    public event Func<ChatMessageDto, Task>? OnMessageRestored;

    public HubConnectionState State => _hubConnection?.State ?? HubConnectionState.Disconnected;

    public ChatHubService(
        AuthenticationStateProvider authStateProvider,
        IConfiguration configuration,
        ILogger<ChatHubService> logger)
    {
        _authStateProvider = authStateProvider;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task ConnectAsync()
    {
        if (_hubConnection != null && _hubConnection.State == HubConnectionState.Connected)
        {
            return;
        }

        try
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;

            var apiBaseUrl = _configuration["ApiBaseUrl"] ?? "https://localhost:7288";
            var hubUrl = $"{apiBaseUrl.TrimEnd('/')}/hubs/chat";

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(hubUrl, options =>
                {
                    options.AccessTokenProvider = async () =>
                    {
                        var state = await _authStateProvider.GetAuthenticationStateAsync();
                        var token = state.User.FindFirst("access_token")?.Value;
                        if (string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(userId))
                        {
                            token = JwtTokenHandler.GetUserAccessToken(userId);
                        }
                        return token;
                    };
                })
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.On<ChatMessageDto>("ReceiveMessage", async (message) =>
            {
                if (OnMessageReceived != null)
                {
                    try
                    {
                        await OnMessageReceived.Invoke(message);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing ReceiveMessage in ChatHubService");
                    }
                }
            });

            _hubConnection.On<MessagesReadNotification>("MessagesRead", (notification) =>
            {
                try
                {
                    OnMessagesRead?.Invoke(notification);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing MessagesRead in ChatHubService");
                }
            });

            _hubConnection.On<string, string, bool>("UserTyping", (convId, userName, isTyping) =>
            {
                try
                {
                    OnUserTyping?.Invoke(convId, userName, isTyping);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing UserTyping in ChatHubService");
                }
            });

            _hubConnection.On<string, string, string, int>("MessageReactionUpdated", (convId, messageId, emoji, count) =>
            {
                try
                {
                    OnReactionUpdated?.Invoke(convId, messageId, emoji, count);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing MessageReactionUpdated in ChatHubService");
                }
            });

            _hubConnection.On<MessageDeletedNotification>("MessageDeleted", (notification) =>
            {
                try
                {
                    OnMessageDeleted?.Invoke(notification);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing MessageDeleted in ChatHubService");
                }
            });

            _hubConnection.On<ChatMessageDto>("MessageRestored", async (message) =>
            {
                if (OnMessageRestored != null)
                {
                    try
                    {
                        await OnMessageRestored.Invoke(message);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing MessageRestored in ChatHubService");
                    }
                }
            });

            await _hubConnection.StartAsync();
            _logger.LogInformation("ChatHub connected successfully to {HubUrl}", hubUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to ChatHub");
        }
    }

    public async Task JoinConversationAsync(Guid conversationId)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            try
            {
                await _hubConnection.SendAsync("JoinConversation", conversationId.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining conversation {ConversationId}", conversationId);
            }
        }
    }

    public async Task LeaveConversationAsync(Guid conversationId)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            try
            {
                await _hubConnection.SendAsync("LeaveConversation", conversationId.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error leaving conversation {ConversationId}", conversationId);
            }
        }
    }

    public async Task SendTypingAsync(Guid conversationId, string userName, bool isTyping)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            try
            {
                await _hubConnection.SendAsync("SendTyping", conversationId.ToString(), userName, isTyping);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending typing status for {ConversationId}", conversationId);
            }
        }
    }

    public async Task SendReactionAsync(Guid conversationId, Guid messageId, string emoji, int count)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            try
            {
                await _hubConnection.SendAsync("SendReaction", conversationId.ToString(), messageId.ToString(), emoji, count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending reaction for conversation {ConversationId}, message {MessageId}", conversationId, messageId);
            }
        }
    }

    public async Task DisconnectAsync()
    {
        if (_hubConnection != null)
        {
            try
            {
                await _hubConnection.StopAsync();
            }
            catch { }
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        if (_hubConnection != null)
        {
            try
            {
                await _hubConnection.DisposeAsync();
            }
            catch { }
        }
    }
}
