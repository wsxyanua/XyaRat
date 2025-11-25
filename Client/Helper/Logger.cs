using System;
using System.Diagnostics;
using System.IO;

namespace Client.Helper
{
    public static class Logger
    {
        private static readonly object lockObj = new object();
        private static readonly string logPath = Path.Combine(
            Path.GetTempPath(), 
            "XyaClient.log"
        );

        public static void Error(string message, Exception ex = null)
        {
            try
            {
                lock (lockObj)
                {
                    string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [ERROR] {message}";
                    if (ex != null)
                        logMessage += $"\nException: {ex.GetType().Name}\nMessage: {ex.Message}\nStackTrace: {ex.StackTrace}";
                    
                    Debug.WriteLine(logMessage);
                    
#if DEBUG
                    File.AppendAllText(logPath, logMessage + "\n");
#endif
                }
            }
            catch
            {
                // Silent fail for logger itself
            }
        }

        public static void Info(string message)
        {
            try
            {
                lock (lockObj)
                {
                    string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [INFO] {message}";
                    Debug.WriteLine(logMessage);
                    
#if DEBUG
                    File.AppendAllText(logPath, logMessage + "\n");
#endif
                }
            }
            catch
            {
                // Silent fail for logger itself
            }
        }

        public static void Warning(string message)
        {
            try
            {
                lock (lockObj)
                {
                    string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [WARN] {message}";
                    Debug.WriteLine(logMessage);
                    
#if DEBUG
                    File.AppendAllText(logPath, logMessage + "\n");
#endif
                }
            }
            catch
            {
                // Silent fail for logger itself
            }
        }
    }
}
