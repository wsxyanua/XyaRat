using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace Server.Security
{
    public class IpWhitelist
    {
        private readonly ConcurrentDictionary<string, bool> _whitelist;
        private readonly ConcurrentDictionary<string, int> _blacklist;
        private readonly string _whitelistFilePath;
        private readonly int _maxFailedAttempts;
        private readonly TimeSpan _blacklistDuration;
        
        public bool IsEnabled { get; set; }
        public bool IsBlacklistEnabled { get; set; }
        
        public IpWhitelist(string whitelistFilePath = "whitelist.txt", 
                          int maxFailedAttempts = 5,
                          TimeSpan? blacklistDuration = null)
        {
            _whitelist = new ConcurrentDictionary<string, bool>();
            _blacklist = new ConcurrentDictionary<string, int>();
            _whitelistFilePath = whitelistFilePath;
            _maxFailedAttempts = maxFailedAttempts;
            _blacklistDuration = blacklistDuration ?? TimeSpan.FromHours(1);
            
            IsEnabled = false;
            IsBlacklistEnabled = true;
            
            LoadWhitelist();
        }
        
        public bool IsAllowed(string ipAddress)
        {
            // Check blacklist first
            if (IsBlacklistEnabled && IsBlacklisted(ipAddress))
            {
                return false;
            }
            
            // If whitelist is disabled, allow all non-blacklisted IPs
            if (!IsEnabled)
            {
                return true;
            }
            
            // Check whitelist
            return _whitelist.ContainsKey(ipAddress);
        }
        
        public bool IsBlacklisted(string ipAddress)
        {
            return _blacklist.ContainsKey(ipAddress);
        }
        
        public void AddToWhitelist(string ipAddress)
        {
            _whitelist.TryAdd(ipAddress, true);
            SaveWhitelist();
        }
        
        public void RemoveFromWhitelist(string ipAddress)
        {
            _whitelist.TryRemove(ipAddress, out _);
            SaveWhitelist();
        }
        
        public void AddToBlacklist(string ipAddress, int attempts = 0)
        {
            _blacklist.TryAdd(ipAddress, attempts);
        }
        
        public void RemoveFromBlacklist(string ipAddress)
        {
            _blacklist.TryRemove(ipAddress, out _);
        }
        
        public void RecordFailedAttempt(string ipAddress)
        {
            if (!IsBlacklistEnabled)
                return;
            
            int attempts = _blacklist.AddOrUpdate(ipAddress, 1, (key, oldValue) => oldValue + 1);
            
            if (attempts >= _maxFailedAttempts)
            {
                // Blacklist permanently (or until manual removal)
                AddToBlacklist(ipAddress, attempts);
            }
        }
        
        public void ResetFailedAttempts(string ipAddress)
        {
            _blacklist.TryRemove(ipAddress, out _);
        }
        
        public List<string> GetWhitelistedIPs()
        {
            return _whitelist.Keys.ToList();
        }
        
        public Dictionary<string, int> GetBlacklistedIPs()
        {
            return _blacklist.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
        
        private void LoadWhitelist()
        {
            try
            {
                if (!File.Exists(_whitelistFilePath))
                {
                    // Create default whitelist with localhost
                    File.WriteAllLines(_whitelistFilePath, new[] { "127.0.0.1", "::1" });
                }
                
                string[] lines = File.ReadAllLines(_whitelistFilePath);
                
                foreach (string line in lines)
                {
                    string ip = line.Trim();
                    if (!string.IsNullOrEmpty(ip) && !ip.StartsWith("#"))
                    {
                        _whitelist.TryAdd(ip, true);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[!] Failed to load whitelist: {ex.Message}");
            }
        }
        
        private void SaveWhitelist()
        {
            try
            {
                File.WriteAllLines(_whitelistFilePath, _whitelist.Keys);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[!] Failed to save whitelist: {ex.Message}");
            }
        }
        
        public void ClearBlacklist()
        {
            _blacklist.Clear();
        }
        
        public string GetStatistics()
        {
            return $"Whitelist: {_whitelist.Count} IPs, Blacklist: {_blacklist.Count} IPs, " +
                   $"Whitelist Enabled: {IsEnabled}, Blacklist Enabled: {IsBlacklistEnabled}";
        }
    }
}
