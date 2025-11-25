using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;

namespace Server.Security
{
    public class RateLimiter
    {
        private readonly ConcurrentDictionary<string, ClientRateInfo> _clientRates;
        private readonly Timer _cleanupTimer;
        
        // Configuration
        private readonly int _maxConnectionsPerMinute;
        private readonly int _maxCommandsPerMinute;
        private readonly int _maxBytesPerMinute;
        private readonly TimeSpan _cleanupInterval;
        
        public RateLimiter(int maxConnectionsPerMinute = 10, 
                          int maxCommandsPerMinute = 100, 
                          int maxBytesPerMinute = 10485760) // 10MB
        {
            _clientRates = new ConcurrentDictionary<string, ClientRateInfo>();
            _maxConnectionsPerMinute = maxConnectionsPerMinute;
            _maxCommandsPerMinute = maxCommandsPerMinute;
            _maxBytesPerMinute = maxBytesPerMinute;
            _cleanupInterval = TimeSpan.FromMinutes(5);
            
            // Cleanup timer to remove old entries
            _cleanupTimer = new Timer(CleanupOldEntries, null, _cleanupInterval, _cleanupInterval);
        }
        
        public bool AllowConnection(string ipAddress)
        {
            var rateInfo = GetOrCreateRateInfo(ipAddress);
            
            lock (rateInfo)
            {
                CleanExpiredEntries(rateInfo.ConnectionTimes);
                
                if (rateInfo.ConnectionTimes.Count >= _maxConnectionsPerMinute)
                {
                    return false;
                }
                
                rateInfo.ConnectionTimes.Add(DateTime.UtcNow);
                return true;
            }
        }
        
        public bool AllowCommand(string ipAddress)
        {
            var rateInfo = GetOrCreateRateInfo(ipAddress);
            
            lock (rateInfo)
            {
                CleanExpiredEntries(rateInfo.CommandTimes);
                
                if (rateInfo.CommandTimes.Count >= _maxCommandsPerMinute)
                {
                    return false;
                }
                
                rateInfo.CommandTimes.Add(DateTime.UtcNow);
                return true;
            }
        }
        
        public bool AllowDataTransfer(string ipAddress, int bytes)
        {
            var rateInfo = GetOrCreateRateInfo(ipAddress);
            
            lock (rateInfo)
            {
                CleanExpiredEntries(rateInfo.DataTransfers);
                
                int currentBytes = rateInfo.DataTransfers.Sum(x => x.Bytes);
                
                if (currentBytes + bytes > _maxBytesPerMinute)
                {
                    return false;
                }
                
                rateInfo.DataTransfers.Add(new DataTransferInfo
                {
                    Timestamp = DateTime.UtcNow,
                    Bytes = bytes
                });
                
                return true;
            }
        }
        
        public void ResetClient(string ipAddress)
        {
            _clientRates.TryRemove(ipAddress, out _);
        }
        
        public Dictionary<string, RateLimitStats> GetStatistics()
        {
            var stats = new Dictionary<string, RateLimitStats>();
            
            foreach (var kvp in _clientRates)
            {
                var rateInfo = kvp.Value;
                lock (rateInfo)
                {
                    CleanExpiredEntries(rateInfo.ConnectionTimes);
                    CleanExpiredEntries(rateInfo.CommandTimes);
                    CleanExpiredEntries(rateInfo.DataTransfers);
                    
                    stats[kvp.Key] = new RateLimitStats
                    {
                        ConnectionsLastMinute = rateInfo.ConnectionTimes.Count,
                        CommandsLastMinute = rateInfo.CommandTimes.Count,
                        BytesLastMinute = rateInfo.DataTransfers.Sum(x => x.Bytes),
                        LastActivity = rateInfo.LastActivity
                    };
                }
            }
            
            return stats;
        }
        
        private ClientRateInfo GetOrCreateRateInfo(string ipAddress)
        {
            return _clientRates.GetOrAdd(ipAddress, _ => new ClientRateInfo
            {
                ConnectionTimes = new List<DateTime>(),
                CommandTimes = new List<DateTime>(),
                DataTransfers = new List<DataTransferInfo>()
            });
        }
        
        private void CleanExpiredEntries(List<DateTime> times)
        {
            DateTime cutoff = DateTime.UtcNow.AddMinutes(-1);
            times.RemoveAll(t => t < cutoff);
        }
        
        private void CleanExpiredEntries(List<DataTransferInfo> transfers)
        {
            DateTime cutoff = DateTime.UtcNow.AddMinutes(-1);
            transfers.RemoveAll(t => t.Timestamp < cutoff);
        }
        
        private void CleanupOldEntries(object state)
        {
            DateTime cutoff = DateTime.UtcNow.AddMinutes(-10);
            
            var keysToRemove = _clientRates
                .Where(kvp => kvp.Value.LastActivity < cutoff)
                .Select(kvp => kvp.Key)
                .ToList();
            
            foreach (var key in keysToRemove)
            {
                _clientRates.TryRemove(key, out _);
            }
        }
        
        public void Dispose()
        {
            _cleanupTimer?.Dispose();
        }
        
        private class ClientRateInfo
        {
            public List<DateTime> ConnectionTimes { get; set; }
            public List<DateTime> CommandTimes { get; set; }
            public List<DataTransferInfo> DataTransfers { get; set; }
            public DateTime LastActivity => GetLastActivity();
            
            private DateTime GetLastActivity()
            {
                var times = new List<DateTime>();
                
                if (ConnectionTimes.Count > 0)
                    times.Add(ConnectionTimes.Max());
                    
                if (CommandTimes.Count > 0)
                    times.Add(CommandTimes.Max());
                    
                if (DataTransfers.Count > 0)
                    times.Add(DataTransfers.Max(x => x.Timestamp));
                
                return times.Count > 0 ? times.Max() : DateTime.MinValue;
            }
        }
        
        private class DataTransferInfo
        {
            public DateTime Timestamp { get; set; }
            public int Bytes { get; set; }
        }
    }
    
    public class RateLimitStats
    {
        public int ConnectionsLastMinute { get; set; }
        public int CommandsLastMinute { get; set; }
        public int BytesLastMinute { get; set; }
        public DateTime LastActivity { get; set; }
        
        public override string ToString()
        {
            return $"Connections: {ConnectionsLastMinute}/{100}, Commands: {CommandsLastMinute}/{100}, Data: {BytesLastMinute / 1024}KB";
        }
    }
}
