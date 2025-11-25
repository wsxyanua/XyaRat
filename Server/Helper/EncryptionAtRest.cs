using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Server.Helper
{
    /// <summary>
    /// Encryption at Rest - Encrypts sensitive data before storing to disk
    /// Uses AES-256-GCM for authenticated encryption
    /// </summary>
    public static class EncryptionAtRest
    {
        // Master key derived from machine-specific data + hardcoded salt
        // In production, this should be stored in secure key storage (HSM, Azure Key Vault, etc.)
        private static readonly byte[] MasterKeySalt = new byte[]
        {
            0x4B, 0x65, 0x79, 0x53, 0x61, 0x6C, 0x74, 0x31,
            0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39,
            0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48
        };

        private const int KeySize = 256; // AES-256
        private const int NonceSize = 12; // 96 bits for GCM
        private const int TagSize = 16; // 128 bits authentication tag
        private const int Iterations = 100000; // PBKDF2 iterations

        /// <summary>
        /// Derives master encryption key from machine-specific entropy
        /// </summary>
        private static byte[] DeriveMasterKey()
        {
            try
            {
                // Use machine-specific data as entropy
                string machineId = Environment.MachineName + Environment.UserName + Environment.ProcessorCount;
                byte[] entropy = Encoding.UTF8.GetBytes(machineId);

                using (var deriveBytes = new Rfc2898DeriveBytes(entropy, MasterKeySalt, Iterations, HashAlgorithmName.SHA256))
                {
                    return deriveBytes.GetBytes(KeySize / 8); // 32 bytes for AES-256
                }
            }
            catch
            {
                // Fallback to hardcoded key if machine-specific derivation fails
                using (var deriveBytes = new Rfc2898DeriveBytes("XyaRatMasterKey2025", MasterKeySalt, Iterations, HashAlgorithmName.SHA256))
                {
                    return deriveBytes.GetBytes(KeySize / 8);
                }
            }
        }

        /// <summary>
        /// Encrypts data using AES-256-GCM
        /// </summary>
        /// <param name="plaintext">Data to encrypt</param>
        /// <returns>Encrypted data (nonce + ciphertext + tag)</returns>
        public static byte[] Encrypt(byte[] plaintext)
        {
            if (plaintext == null || plaintext.Length == 0)
                return null;

            try
            {
                byte[] masterKey = DeriveMasterKey();
                byte[] nonce = new byte[NonceSize];
                byte[] tag = new byte[TagSize];
                byte[] ciphertext = new byte[plaintext.Length];

                // Generate random nonce
                using (var rng = new RNGCryptoServiceProvider())
                {
                    rng.GetBytes(nonce);
                }

                // Encrypt using AES-GCM
                using (var aesGcm = new AesGcm(masterKey))
                {
                    aesGcm.Encrypt(nonce, plaintext, ciphertext, tag);
                }

                // Combine: nonce + ciphertext + tag
                byte[] result = new byte[NonceSize + ciphertext.Length + TagSize];
                Buffer.BlockCopy(nonce, 0, result, 0, NonceSize);
                Buffer.BlockCopy(ciphertext, 0, result, NonceSize, ciphertext.Length);
                Buffer.BlockCopy(tag, 0, result, NonceSize + ciphertext.Length, TagSize);

                return result;
            }
            catch (Exception ex)
            {
                Logger.Log($"[EncryptionAtRest] Encryption failed: {ex.Message}", Logger.LogLevel.Error);
                return null;
            }
        }

        /// <summary>
        /// Encrypts string data
        /// </summary>
        public static byte[] Encrypt(string plaintext)
        {
            if (string.IsNullOrEmpty(plaintext))
                return null;

            byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
            return Encrypt(plaintextBytes);
        }

        /// <summary>
        /// Decrypts data using AES-256-GCM
        /// </summary>
        /// <param name="encryptedData">Encrypted data (nonce + ciphertext + tag)</param>
        /// <returns>Decrypted data</returns>
        public static byte[] Decrypt(byte[] encryptedData)
        {
            if (encryptedData == null || encryptedData.Length < NonceSize + TagSize)
                return null;

            try
            {
                byte[] masterKey = DeriveMasterKey();

                // Extract components
                byte[] nonce = new byte[NonceSize];
                byte[] tag = new byte[TagSize];
                byte[] ciphertext = new byte[encryptedData.Length - NonceSize - TagSize];

                Buffer.BlockCopy(encryptedData, 0, nonce, 0, NonceSize);
                Buffer.BlockCopy(encryptedData, NonceSize, ciphertext, 0, ciphertext.Length);
                Buffer.BlockCopy(encryptedData, NonceSize + ciphertext.Length, tag, 0, TagSize);

                // Decrypt using AES-GCM
                byte[] plaintext = new byte[ciphertext.Length];
                using (var aesGcm = new AesGcm(masterKey))
                {
                    aesGcm.Decrypt(nonce, ciphertext, tag, plaintext);
                }

                return plaintext;
            }
            catch (CryptographicException)
            {
                Logger.Log("[EncryptionAtRest] Decryption failed: Authentication tag mismatch", Logger.LogLevel.Error);
                return null;
            }
            catch (Exception ex)
            {
                Logger.Log($"[EncryptionAtRest] Decryption failed: {ex.Message}", Logger.LogLevel.Error);
                return null;
            }
        }

        /// <summary>
        /// Decrypts to string
        /// </summary>
        public static string DecryptToString(byte[] encryptedData)
        {
            byte[] decrypted = Decrypt(encryptedData);
            if (decrypted == null)
                return null;

            return Encoding.UTF8.GetString(decrypted);
        }

        /// <summary>
        /// Encrypts a file in place
        /// </summary>
        /// <param name="filePath">Path to file to encrypt</param>
        /// <returns>True if successful</returns>
        public static bool EncryptFile(string filePath)
        {
            if (!File.Exists(filePath))
                return false;

            try
            {
                byte[] plaintext = File.ReadAllBytes(filePath);
                byte[] encrypted = Encrypt(plaintext);

                if (encrypted == null)
                    return false;

                File.WriteAllBytes(filePath, encrypted);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log($"[EncryptionAtRest] File encryption failed: {ex.Message}", Logger.LogLevel.Error);
                return false;
            }
        }

        /// <summary>
        /// Decrypts a file in place
        /// </summary>
        /// <param name="filePath">Path to file to decrypt</param>
        /// <returns>True if successful</returns>
        public static bool DecryptFile(string filePath)
        {
            if (!File.Exists(filePath))
                return false;

            try
            {
                byte[] encrypted = File.ReadAllBytes(filePath);
                byte[] decrypted = Decrypt(encrypted);

                if (decrypted == null)
                    return false;

                File.WriteAllBytes(filePath, decrypted);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log($"[EncryptionAtRest] File decryption failed: {ex.Message}", Logger.LogLevel.Error);
                return false;
            }
        }

        /// <summary>
        /// Encrypts data and saves to file
        /// </summary>
        public static bool EncryptToFile(string data, string filePath)
        {
            try
            {
                byte[] encrypted = Encrypt(data);
                if (encrypted == null)
                    return false;

                string directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllBytes(filePath, encrypted);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log($"[EncryptionAtRest] EncryptToFile failed: {ex.Message}", Logger.LogLevel.Error);
                return false;
            }
        }

        /// <summary>
        /// Reads and decrypts file
        /// </summary>
        public static string DecryptFromFile(string filePath)
        {
            if (!File.Exists(filePath))
                return null;

            try
            {
                byte[] encrypted = File.ReadAllBytes(filePath);
                return DecryptToString(encrypted);
            }
            catch (Exception ex)
            {
                Logger.Log($"[EncryptionAtRest] DecryptFromFile failed: {ex.Message}", Logger.LogLevel.Error);
                return null;
            }
        }

        /// <summary>
        /// Securely wipes a byte array from memory
        /// </summary>
        public static void SecureWipe(byte[] data)
        {
            if (data == null)
                return;

            for (int i = 0; i < data.Length; i++)
            {
                data[i] = 0;
            }
        }
    }
}
