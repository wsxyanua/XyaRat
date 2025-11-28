using System;
using System.Diagnostics;

namespace Client.Helper
{
    /// <summary>
    /// Centralized error handling to replace empty catch blocks
    /// Provides consistent logging and graceful degradation
    /// </summary>
    public static class ErrorHandler
    {
        /// <summary>
        /// Handles exception with logging (for non-critical operations)
        /// Returns default value on error
        /// </summary>
        public static T HandleNonCritical<T>(Func<T> operation, T defaultValue = default, string context = null)
        {
            try
            {
                return operation();
            }
            catch (Exception ex)
            {
                LogError(ex, context, isCritical: false);
                return defaultValue;
            }
        }

        /// <summary>
        /// Handles exception with logging (for critical operations)
        /// May throw if error is unrecoverable
        /// </summary>
        public static T HandleCritical<T>(Func<T> operation, string context = null)
        {
            try
            {
                return operation();
            }
            catch (Exception ex)
            {
                LogError(ex, context, isCritical: true);
                
                // For critical errors, decide if we should rethrow
                if (IsUnrecoverableError(ex))
                    throw;
                
                return default;
            }
        }

        /// <summary>
        /// Handles void operation with error logging
        /// </summary>
        public static void HandleVoid(Action operation, string context = null, bool isCritical = false)
        {
            try
            {
                operation();
            }
            catch (Exception ex)
            {
                LogError(ex, context, isCritical);
                
                if (isCritical && IsUnrecoverableError(ex))
                    throw;
            }
        }

        /// <summary>
        /// Logs error with appropriate severity
        /// </summary>
        private static void LogError(Exception ex, string context, bool isCritical)
        {
            string message = string.IsNullOrEmpty(context) 
                ? $"Error: {ex.Message}" 
                : $"Error in {context}: {ex.Message}";

            if (isCritical)
            {
                Logger.Error(message, ex);
            }
            else
            {
                Logger.Warning(message);
            }

#if DEBUG
            // In debug mode, also write to Debug output
            Debug.WriteLine($"[ErrorHandler] {message}");
            if (ex != null)
            {
                Debug.WriteLine($"[ErrorHandler] Stack: {ex.StackTrace}");
            }
#endif
        }

        /// <summary>
        /// Determines if error is unrecoverable
        /// </summary>
        private static bool IsUnrecoverableError(Exception ex)
        {
            return ex is OutOfMemoryException
                || ex is StackOverflowException
                || ex is AccessViolationException
                || ex is AppDomainUnloadedException;
        }

        /// <summary>
        /// Tries operation multiple times with exponential backoff
        /// </summary>
        public static T Retry<T>(Func<T> operation, int maxAttempts = 3, int delayMs = 1000, string context = null)
        {
            int attempt = 0;
            Exception lastException = null;

            while (attempt < maxAttempts)
            {
                try
                {
                    return operation();
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    attempt++;

                    if (attempt >= maxAttempts)
                        break;

                    Logger.Warning($"Retry {attempt}/{maxAttempts} for {context}: {ex.Message}");
                    
                    // Exponential backoff
                    System.Threading.Thread.Sleep(delayMs * attempt);
                }
            }

            // All retries failed
            LogError(lastException, context, isCritical: true);
            return default;
        }

        /// <summary>
        /// Safe dispose with error handling
        /// </summary>
        public static void SafeDispose(IDisposable obj, string context = null)
        {
            if (obj == null) return;

            HandleVoid(() => obj.Dispose(), context ?? "Dispose", isCritical: false);
        }

        /// <summary>
        /// Safe close with error handling
        /// </summary>
        public static void SafeClose<T>(T obj, Action<T> closeAction, string context = null) where T : class
        {
            if (obj == null || closeAction == null) return;

            HandleVoid(() => closeAction(obj), context ?? "Close", isCritical: false);
        }
    }
}
