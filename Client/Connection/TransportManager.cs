using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Client.Connection
{
    public class TransportManager
    {
        private List<ITransport> transports;
        private ITransport currentTransport;
        private int currentIndex;
        private int reconnectAttempts;
        private readonly int maxReconnectAttempts = 10;

        public TransportManager()
        {
            transports = new List<ITransport>();
            currentIndex = 0;
            reconnectAttempts = 0;
        }

        public void AddTransport(ITransport transport)
        {
            transports.Add(transport);
        }

        public bool Connect(string host, int port)
        {
            if (transports.Count == 0)
                return false;

            for (int i = 0; i < transports.Count; i++)
            {
                currentIndex = (currentIndex + i) % transports.Count;
                currentTransport = transports[currentIndex];

                try
                {
                    if (currentTransport.Connect(host, port))
                    {
                        reconnectAttempts = 0;
                        return true;
                    }
                }
                catch
                {
                    continue;
                }
            }

            return false;
        }

        public bool Reconnect(string host, int port)
        {
            reconnectAttempts++;
            
            if (reconnectAttempts > maxReconnectAttempts)
            {
                reconnectAttempts = 0;
                currentIndex = (currentIndex + 1) % transports.Count;
            }

            int delay = CalculateBackoffDelay(reconnectAttempts);
            Thread.Sleep(delay);

            return Connect(host, port);
        }

        public void Send(byte[] data)
        {
            if (currentTransport != null && currentTransport.IsConnected)
            {
                currentTransport.Send(data);
            }
        }

        public byte[] Receive(int size)
        {
            if (currentTransport != null && currentTransport.IsConnected)
            {
                return currentTransport.Receive(size);
            }
            return null;
        }

        public void Disconnect()
        {
            if (currentTransport != null)
            {
                currentTransport.Disconnect();
            }
        }

        public bool IsConnected
        {
            get { return currentTransport != null && currentTransport.IsConnected; }
        }

        public ITransport GetCurrentTransport()
        {
            return currentTransport;
        }

        public string GetCurrentTransportType()
        {
            if (currentTransport == null)
                return "None";
            
            return currentTransport.GetType().Name.Replace("Transport", "");
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
    }
}
