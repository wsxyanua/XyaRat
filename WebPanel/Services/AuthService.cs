using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebPanel.Data;
using WebPanel.Models;

namespace WebPanel.Services;

public class AuthService
{
    private readonly WebPanelDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(WebPanelDbContext context, IConfiguration configuration, ILogger<AuthService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<LoginResponse> AuthenticateAsync(string username, string password)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

            if (user == null)
            {
                _logger.LogWarning("Failed login attempt for username: {Username}", username);
                return new LoginResponse
                {
                    Success = false,
                    Message = "Invalid username or password"
                };
            }

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                _logger.LogWarning("Invalid password for user: {Username}", username);
                return new LoginResponse
                {
                    Success = false,
                    Message = "Invalid username or password"
                };
            }

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Generate JWT token
            var token = GenerateJwtToken(user);

            _logger.LogInformation("User {Username} logged in successfully", username);

            return new LoginResponse
            {
                Success = true,
                Token = token,
                User = new UserInfo
                {
                    Id = user.Id,
                    Username = user.Username,
                    Role = user.Role,
                    LastLoginAt = user.LastLoginAt
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during authentication for user: {Username}", username);
            return new LoginResponse
            {
                Success = false,
                Message = "An error occurred during authentication"
            };
        }
    }

    private string GenerateJwtToken(User user)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
        var jwtIssuer = _configuration["Jwt:Issuer"] ?? "XyaRatWebPanel";
        var jwtAudience = _configuration["Jwt:Audience"] ?? "XyaRatClients";
        var expiryMinutes = int.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "480");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<User?> CreateUserAsync(string username, string password, string role = "User")
    {
        try
        {
            // Check if user exists
            if (await _context.Users.AnyAsync(u => u.Username == username))
            {
                _logger.LogWarning("Attempted to create duplicate user: {Username}", username);
                return null;
            }

            var user = new User
            {
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created new user: {Username} with role: {Role}", username, role);
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user: {Username}", username);
            return null;
        }
    }

    public async Task EnsureDefaultAdminAsync()
    {
        try
        {
            if (!await _context.Users.AnyAsync())
            {
                // Read from configuration instead of hardcoded
                var username = _configuration["DefaultAdmin:Username"];
                var password = _configuration["DefaultAdmin:Password"];
                var createIfNotExists = bool.Parse(_configuration["DefaultAdmin:CreateIfNotExists"] ?? "false");

                if (!createIfNotExists || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    _logger.LogError("No users exist and default admin creation is disabled or not configured!");
                    Console.WriteLine("==========================================================");
                    Console.WriteLine("⚠️  ERROR: No users in database!");
                    Console.WriteLine("Configure DefaultAdmin in appsettings.json or environment variables:");
                    Console.WriteLine("  DefaultAdmin__Username");
                    Console.WriteLine("  DefaultAdmin__Password");
                    Console.WriteLine("  DefaultAdmin__CreateIfNotExists=true");
                    Console.WriteLine("==========================================================");
                    return;
                }

                var defaultAdmin = await CreateUserAsync(username, password, "Admin");
                if (defaultAdmin != null)
                {
                    _logger.LogWarning("Created default admin user from configuration");
                    Console.WriteLine("==========================================================");
                    Console.WriteLine("DEFAULT ADMIN CREATED:");
                    Console.WriteLine($"Username: {username}");
                    Console.WriteLine("Password: [CONFIGURED]");
                    Console.WriteLine("⚠️  CHANGE THIS PASSWORD IMMEDIATELY!");
                    Console.WriteLine("==========================================================");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ensuring default admin");
        }
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
            var key = Encoding.UTF8.GetBytes(jwtKey);

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return true;
        }
        catch
        {
            return false;
        }
    }
}
