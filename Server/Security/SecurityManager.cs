using System;
using System.Collections.Generic;

namespace Server.Security
{
    public class SecurityManager
    {
        private static SecurityManager _instance;
        private static readonly object _lock = new object();
        
        public RateLimiter RateLimiter { get; private set; }
        public IpWhitelist IpWhitelist { get; private set; }
        public ConnectionThrottle ConnectionThrottle { get; private set; }
        
        public bool IsRateLimitingEnabled { get; set; }
        public bool IsIpWhitelistEnabled { get; set; }
        public bool IsConnectionThrottleEnabled { get; set; }
        
        private SecurityManager()
        {
            RateLimiter = new RateLimiter(
                maxConnectionsPerMinute: 10,
                maxCommandsPerMinute: 100,
                maxBytesPerMinute: 10485760 // 10MB
            );
            
            IpWhitelist = new IpWhitelist(
                whitelistFilePath: "whitelist.txt",
                maxFailedAttempts: 5,
                blacklistDuration: TimeSpan.FromHours(1)
            );
            
            ConnectionThrottle = new ConnectionThrottle(
                maxConcurrentOperations: 5,
                operationTimeout: TimeSpan.FromSeconds(30)
            );
            
            // Default: Rate limiting enabled, whitelist disabled
            IsRateLimitingEnabled = true;
            IsIpWhitelistEnabled = false;
            IsConnectionThrottleEnabled = true;
        }
        
        public static SecurityManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new SecurityManager();
                        }
                    }
                }
                return _instance;
            }
        }
        
        public bool ValidateConnection(string ipAddress)
        {
            // Check IP whitelist/blacklist
            if (IsIpWhitelistEnabled && !IpWhitelist.IsAllowed(ipAddress))
            {
                return false;
            }
            
            // Check rate limiting
            if (IsRateLimitingEnabled && !RateLimiter.AllowConnection(ipAddress))
            {
                return false;
            }
            
            return true;
        }
        
        public bool ValidateCommand(string ipAddress)
        {
            if (IsRateLimitingEnabled && !RateLimiter.AllowCommand(ipAddress))
            {
                return false;
            }
            
            return true;
        }
        
        public bool ValidateDataTransfer(string ipAddress, int bytes)
        {
            if (IsRateLimitingEnabled && !RateLimiter.AllowDataTransfer(ipAddress, bytes))
            {
                return false;
            }
            
            return true;
        }
        
        public void RecordFailedAttempt(string ipAddress)
        {
            if (IsIpWhitelistEnabled)
            {
                IpWhitelist.RecordFailedAttempt(ipAddress);
            }
        }
        
        public void ResetClient(string ipAddress)
        {
            RateLimiter.ResetClient(ipAddress);
            ConnectionThrottle.ResetClient(ipAddress);
        }
        
        public string GetStatistics()
        {
            var stats = new List<string>();
            
            stats.Add("=== Security Statistics ===");
            stats.Add($"Rate Limiting: {(IsRateLimitingEnabled ? "ENABLED" : "DISABLED")}");
            stats.Add($"IP Whitelist: {(IsIpWhitelistEnabled ? "ENABLED" : "DISABLED")}");
            stats.Add($"Connection Throttle: {(IsConnectionThrottleEnabled ? "ENABLED" : "DISABLED")}");
            stats.Add("");
            
            if (IsRateLimitingEnabled)
            {
                stats.Add("Rate Limit Stats:");
                var rateLimitStats = RateLimiter.GetStatistics();
                foreach (var kvp in rateLimitStats)
                {
                    stats.Add($"  {kvp.Key}: {kvp.Value}");
                }
                stats.Add("");
            }
            
            if (IsIpWhitelistEnabled)
            {
                stats.Add(IpWhitelist.GetStatistics());
                stats.Add("");
            }
            
            return string.Join("\n", stats);
        }
        
        public void Dispose()
        {
            RateLimiter?.Dispose();
            ConnectionThrottle?.Dispose();
        }
    }
}
