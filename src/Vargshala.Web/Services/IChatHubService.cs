using Microsoft.AspNetCore.SignalR.Client;
using Vargshala.Contracts.Messages;

namespace Vargshala.Web.Services;

public interface IChatHubService : IAsyncDisposable
{
    event Func<ChatMessageDto, Task>? OnMessageReceived;
    event Action<MessagesReadNotification>? OnMessagesRead;
    event Action<string, string, bool>? OnUserTyping;

    HubConnectionState State { get; }

    Task ConnectAsync();
    Task DisconnectAsync();
    Task JoinConversationAsync(Guid conversationId);
    Task LeaveConversationAsync(Guid conversationId);
    Task SendTypingAsync(Guid conversationId, string userName, bool isTyping);
}
