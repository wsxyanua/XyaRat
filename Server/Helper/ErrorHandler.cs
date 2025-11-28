using System;
using System.Threading;

namespace Server.Helper
{
    /// <summary>
    /// Centralized error handling to replace empty catch blocks
    /// Provides consistent logging and recovery strategies
    /// </summary>
    public static class ErrorHandler
    {
        /// <summary>
        /// Handle non-critical errors that should be logged but not crash the application
        /// Returns default value on error
        /// </summary>
        public static T HandleNonCritical<T>(Func<T> action, Exception ex, string context = "")
        {
            try
            {
                Logger.Error($"Non-critical error in {context}: {ex.Message}");
                Logger.Error(ex);
                return default(T);
            }
            catch
            {
                // Last resort: Don't let logging errors crash the app
                return default(T);
            }
        }

        /// <summary>
        /// Handle non-critical void operations
        /// </summary>
        public static void HandleNonCritical(Action action, Exception ex, string context = "")
        {
            try
            {
                Logger.Error($"Non-critical error in {context}: {ex.Message}");
                Logger.Error(ex);
            }
            catch
            {
                // Last resort: Don't let logging errors crash the app
            }
        }

        /// <summary>
        /// Handle critical errors that may require throwing after logging
        /// </summary>
        public static T HandleCritical<T>(Func<T> action, Exception ex, string context = "", bool rethrow = false)
        {
            try
            {
                Logger.Error($"Critical error in {context}: {ex.Message}");
                Logger.Error(ex);
                
                if (rethrow && IsUnrecoverableError(ex))
                {
                    throw;
                }
                
                return default(T);
            }
            catch
            {
                return default(T);
            }
        }

        /// <summary>
        /// Handle critical void operations
        /// </summary>
        public static void HandleCritical(Action action, Exception ex, string context = "", bool rethrow = false)
        {
            try
            {
                Logger.Error($"Critical error in {context}: {ex.Message}");
                Logger.Error(ex);
                
                if (rethrow && IsUnrecoverableError(ex))
                {
                    throw;
                }
            }
            catch
            {
                // Last resort: Don't let logging errors crash the app
            }
        }

        /// <summary>
        /// Execute action with retry logic
        /// </summary>
        public static T Retry<T>(Func<T> action, int maxAttempts = 3, int delayMs = 1000)
        {
            Exception lastException = null;
            
            for (int i = 0; i < maxAttempts; i++)
            {
                try
                {
                    return action();
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    if (i < maxAttempts - 1)
                    {
                        Thread.Sleep(delayMs * (i + 1)); // Exponential backoff
                    }
                }
            }
            
            Logger.Error($"Retry failed after {maxAttempts} attempts: {lastException?.Message}");
            return default(T);
        }

        /// <summary>
        /// Safely dispose of IDisposable objects
        /// </summary>
        public static void SafeDispose(IDisposable disposable, string context = "")
        {
            if (disposable == null) return;
            
            try
            {
                disposable.Dispose();
            }
            catch (Exception ex)
            {
                HandleNonCritical(() => { }, ex, $"Dispose failed: {context}");
            }
        }

        /// <summary>
        /// Safely close objects with Close() method
        /// </summary>
        public static void SafeClose<T>(T obj, string context = "") where T : class
        {
            if (obj == null) return;
            
            try
            {
                var closeMethod = typeof(T).GetMethod("Close");
                if (closeMethod != null)
                {
                    closeMethod.Invoke(obj, null);
                }
            }
            catch (Exception ex)
            {
                HandleNonCritical(() => { }, ex, $"Close failed: {context}");
            }
        }

        /// <summary>
        /// Check if exception is unrecoverable and should terminate
        /// </summary>
        private static bool IsUnrecoverableError(Exception ex)
        {
            return ex is OutOfMemoryException
                || ex is StackOverflowException
                || ex is AccessViolationException
                || ex is AppDomainUnloadedException
                || ex is ThreadAbortException;
        }
    }
}
