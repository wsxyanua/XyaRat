using System.ComponentModel.DataAnnotations.Schema;

namespace WebPanel.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "User"; // Admin, User, Viewer
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
}

public class ClientSession
{
    public int Id { get; set; }
    public string ClientId { get; set; } = string.Empty;
    public string Hwid { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string OS { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public int Ping { get; set; }
    public bool IsConnected { get; set; }
    public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DisconnectedAt { get; set; }
    public string? ActiveWindow { get; set; }
    public string? AntiVirus { get; set; }
    public string? Version { get; set; }
    
    [NotMapped]
    public Dictionary<string, string> SystemInfo { get; set; } = new();
}

public class CommandHistory
{
    public int Id { get; set; }
    public string ClientId { get; set; } = string.Empty;
    public string Command { get; set; } = string.Empty;
    public string? Response { get; set; }
    public bool Success { get; set; }
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
    public string ExecutedBy { get; set; } = string.Empty;
    public int ExecutionTimeMs { get; set; }
}

public class FileTransfer
{
    public int Id { get; set; }
    public string ClientId { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public long TransferredBytes { get; set; }
    public string Direction { get; set; } = string.Empty; // Upload, Download
    public string Status { get; set; } = "Pending"; // Pending, InProgress, Completed, Failed
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
}
