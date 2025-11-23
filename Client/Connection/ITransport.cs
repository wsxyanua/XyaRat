using System;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace Client.Connection
{
    public interface ITransport
    {
        bool Connect(string host, int port);
        void Disconnect();
        bool IsConnected { get; }
        void Send(byte[] data);
        byte[] Receive(int size);
        int Available { get; }
        Socket GetSocket();
        X509Certificate2 GetCertificate();
    }
}
