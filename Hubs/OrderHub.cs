using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Security.Claims;

[Authorize]
public class OrderHub : Hub
{
    private static readonly ConcurrentDictionary<Guid, string> _userConnections = new();

    public override async Task OnConnectedAsync()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(userIdClaim, out var userId))
        {
            _userConnections[userId] = Context.ConnectionId;
        }

        await base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        var connectionId = Context.ConnectionId;
        var item = _userConnections.FirstOrDefault(kv => kv.Value == connectionId);
        if (!item.Equals(default(KeyValuePair<Guid, string>)))
        {
            _userConnections.TryRemove(item.Key, out _);
        }

        return base.OnDisconnectedAsync(exception);
    }

    public static async Task NotifyUserAsync(IHubContext<OrderHub> hubContext, Guid userId, string message)
    {
        if (_userConnections.TryGetValue(userId, out var connectionId))
        {
            await hubContext.Clients.Client(connectionId).SendAsync("ReceiveOrderNotification", message);
        }
    }
}
