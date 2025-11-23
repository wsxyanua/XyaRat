using System;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace Client.Connection
{
    public class TcpTransport : ITransport
    {
        private Socket socket;
        private SslStream sslStream;
        private X509Certificate2 serverCertificate;
        private bool isConnected;

        public bool IsConnected => isConnected && socket != null && socket.Connected;
        public int Available => socket?.Available ?? 0;

        public bool Connect(string host, int port)
        {
            try
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    ReceiveBufferSize = 50 * 1024,
                    SendBufferSize = 50 * 1024,
                };

                socket.Connect(host, port);

                if (socket.Connected)
                {
                    sslStream = new SslStream(new NetworkStream(socket, true), false, ValidateServerCertificate);
                    sslStream.AuthenticateAsClient(host, null, SslProtocols.Tls12 | SslProtocols.Tls13, false);
                    isConnected = true;
                    return true;
                }
            }
            catch
            {
                isConnected = false;
            }
            return false;
        }

        public void Send(byte[] data)
        {
            try
            {
                if (!IsConnected) return;

                byte[] sizeBuffer = BitConverter.GetBytes(data.Length);
                sslStream.Write(sizeBuffer, 0, sizeBuffer.Length);
                sslStream.Write(data, 0, data.Length);
                sslStream.Flush();
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
                if (!IsConnected) return null;

                byte[] buffer = new byte[size];
                int offset = 0;
                int remaining = size;

                while (remaining > 0)
                {
                    int bytesRead = sslStream.Read(buffer, offset, remaining);
                    if (bytesRead <= 0)
                    {
                        isConnected = false;
                        return null;
                    }
                    offset += bytesRead;
                    remaining -= bytesRead;
                }

                return buffer;
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
                sslStream?.Close();
                socket?.Close();
                isConnected = false;
            }
            catch { }
        }

        public Socket GetSocket()
        {
            return socket;
        }

        public X509Certificate2 GetCertificate()
        {
            return serverCertificate;
        }

        private bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            try
            {
                serverCertificate = new X509Certificate2(certificate);
                return Settings.Server_Certificate.Equals(certificate);
            }
            catch
            {
                return false;
            }
        }
    }
}
