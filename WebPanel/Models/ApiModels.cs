namespace WebPanel.Models;

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public string? Message { get; set; }
    public UserInfo? User { get; set; }
}

public class UserInfo
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime? LastLoginAt { get; set; }
}

public class ClientInfo
{
    public string ClientId { get; set; } = string.Empty;
    public string Hwid { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string OS { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public int Ping { get; set; }
    public bool IsConnected { get; set; }
    public DateTime ConnectedAt { get; set; }
    public string? ActiveWindow { get; set; }
    public string? AntiVirus { get; set; }
    public string? Version { get; set; }
}

public class CommandRequest
{
    public string ClientId { get; set; } = string.Empty;
    public string Command { get; set; } = string.Empty;
    public Dictionary<string, object>? Parameters { get; set; }
}

public class CommandResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public object? Data { get; set; }
}

public class FileManagerRequest
{
    public string ClientId { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
}

public class FileItem
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // File, Directory
    public long Size { get; set; }
    public DateTime Modified { get; set; }
}

public class DashboardStats
{
    public int TotalClients { get; set; }
    public int OnlineClients { get; set; }
    public int OfflineClients { get; set; }
    public int CommandsToday { get; set; }
    public int FileTransfersToday { get; set; }
    public Dictionary<string, int> ClientsByCountry { get; set; } = new();
    public Dictionary<string, int> ClientsByOS { get; set; } = new();
}
