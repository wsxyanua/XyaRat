using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebPanel.Models;
using WebPanel.Services;

namespace WebPanel.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(AuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new LoginResponse
            {
                Success = false,
                Message = "Username and password are required"
            });
        }

        var result = await _authService.AuthenticateAsync(request.Username, request.Password);

        if (!result.Success)
        {
            return Unauthorized(result);
        }

        return Ok(result);
    }

    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        // JWT tokens are stateless, client should discard the token
        _logger.LogInformation("User {Username} logged out", User.Identity?.Name);
        return Ok(new { Success = true, Message = "Logged out successfully" });
    }

    [HttpGet("validate")]
    [Authorize]
    public IActionResult ValidateToken()
    {
        return Ok(new
        {
            Success = true,
            Username = User.Identity?.Name,
            Role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
        });
    }
}
