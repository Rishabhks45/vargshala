using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Vargshala.API.Hubs;

[Authorize]
public class ChatHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier 
                     ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? Context.User?.FindFirst("sub")?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier 
                     ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? Context.User?.FindFirst("sub")?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinConversation(string conversationId)
    {
        if (!string.IsNullOrWhiteSpace(conversationId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"conv_{conversationId}");
        }
    }

    public async Task LeaveConversation(string conversationId)
    {
        if (!string.IsNullOrWhiteSpace(conversationId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conv_{conversationId}");
        }
    }

    public async Task SendTyping(string conversationId, string userName, bool isTyping)
    {
        if (!string.IsNullOrWhiteSpace(conversationId))
        {
            await Clients.OthersInGroup($"conv_{conversationId}").SendAsync("UserTyping", conversationId, userName, isTyping);
        }
    }
}
