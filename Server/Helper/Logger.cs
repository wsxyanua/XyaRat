using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Server.Helper
{
    public static class Logger
    {
        public enum LogLevel { Info, Warning, Error, Debug }

        private static readonly object lockObj = new object();
        private static readonly string logPath = Path.Combine(
            Application.StartupPath, 
            "Logs", 
            $"XyaServer_{DateTime.Now:yyyy-MM-dd}.log"
        );

        static Logger()
        {
            try
            {
                string logDir = Path.Combine(Application.StartupPath, "Logs");
                if (!Directory.Exists(logDir))
                    Directory.CreateDirectory(logDir);
            }
            catch { }
        }

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
                    File.AppendAllText(logPath, logMessage + "\n");
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
                    File.AppendAllText(logPath, logMessage + "\n");
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
                    File.AppendAllText(logPath, logMessage + "\n");
                }
            }
            catch
            {
                // Silent fail for logger itself
            }
        }

        public static void Log(string message, LogLevel level = LogLevel.Info)
        {
            switch (level)
            {
                case LogLevel.Info: Info(message); break;
                case LogLevel.Warning: Warning(message); break;
                case LogLevel.Error: Error(message); break;
                case LogLevel.Debug: Info($"[DEBUG] {message}"); break;
            }
        }
    }
}
