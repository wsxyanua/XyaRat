using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System;
using System.Windows.Forms;
using System.Drawing;
using Server.Handle_Packet;
using System.Diagnostics;
using Server.Security;
using Server.Helper;

namespace Server.Connection
{
    class Listener
    {
        private Socket Server { get; set; }
        private SecurityManager SecurityManager { get; set; }
        private X509Certificate2 ServerCertificate { get; set; }
        private bool UseTls { get; set; } = true; // Enable TLS by default

        public void Connect(object port)
        {
            try
            {
                SecurityManager = SecurityManager.Instance;
                
                // Load server certificate for TLS
                if (UseTls)
                {
                    ServerCertificate = CertificateManager.GetServerCertificate();
                    if (ServerCertificate != null)
                    {
                        CertificateManager.LogCertificateInfo(ServerCertificate);
                        new HandleLogs().Addmsg("✓ TLS/SSL enabled with certificate", Color.Green);
                    }
                    else
                    {
                        new HandleLogs().Addmsg("✗ Failed to load certificate, TLS disabled", Color.Orange);
                        UseTls = false;
                    }
                }
                
                // Safe port conversion
                if (!int.TryParse(port?.ToString(), out int portNumber))
                {
                    MessageBox.Show("Invalid port number");
                    Environment.Exit(0);
                    return;
                }
                
                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, portNumber);
                Server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    SendBufferSize = 50 * 1024,
                    ReceiveBufferSize = 50 * 1024,
                };
                Server.Bind(ipEndPoint);
                Server.Listen(500);
                new HandleLogs().Addmsg($"Listenning to: {port}", Color.Green);
                new HandleLogs().Addmsg($"TLS/SSL: {(UseTls ? "Enabled" : "Disabled")}", Color.Blue);
                new HandleLogs().Addmsg($"Security: Rate Limiting={SecurityManager.IsRateLimitingEnabled}, Whitelist={SecurityManager.IsIpWhitelistEnabled}", Color.Blue);
                Server.BeginAccept(EndAccept, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Environment.Exit(0);
            }
        }

        private void EndAccept(IAsyncResult ar)
        {
            try
            {
                Socket clientSocket = Server.EndAccept(ar);
                string clientIp = clientSocket.RemoteEndPoint.ToString().Split(':')[0];
                
                // Validate connection with SecurityManager
                if (!SecurityManager.ValidateConnection(clientIp))
                {
                    new HandleLogs().Addmsg($"Rejected connection from {clientIp} (Rate limit or blacklist)", Color.Red);
                    clientSocket.Close();
                    return;
                }
                
                new Clients(clientSocket);
            }
            catch (Exception ex)
            {
                Logger.Error("Listener EndAccept error", ex);
            }
            finally
            {
                Server.BeginAccept(EndAccept, null);
            }
        }
    }
}