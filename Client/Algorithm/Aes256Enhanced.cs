using System;
using System.Security.Cryptography;
using System.Text;

namespace Client.Algorithm
{
    public class Aes256Enhanced
    {
        private readonly byte[] key;
        private readonly int iterations = 100000;

        public Aes256Enhanced(string password)
        {
            key = Encoding.UTF8.GetBytes(password);
        }

        public string Encrypt(string plainText)
        {
            try
            {
                byte[] salt = GenerateRandomBytes(32);
                byte[] iv = GenerateRandomBytes(16);
                
                using (Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(key, salt, iterations))
                {
                    byte[] derivedKey = pbkdf2.GetBytes(32);
                    
                    using (Aes aes = Aes.Create())
                    {
                        aes.Key = derivedKey;
                        aes.IV = iv;
                        aes.Mode = CipherMode.CBC;
                        aes.Padding = PaddingMode.PKCS7;
                        
                        using (ICryptoTransform encryptor = aes.CreateEncryptor())
                        {
                            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                            byte[] cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
                            
                            byte[] hmac = ComputeHmac(derivedKey, iv, cipherBytes);
                            
                            byte[] result = new byte[salt.Length + iv.Length + hmac.Length + cipherBytes.Length];
                            Buffer.BlockCopy(salt, 0, result, 0, salt.Length);
                            Buffer.BlockCopy(iv, 0, result, salt.Length, iv.Length);
                            Buffer.BlockCopy(hmac, 0, result, salt.Length + iv.Length, hmac.Length);
                            Buffer.BlockCopy(cipherBytes, 0, result, salt.Length + iv.Length + hmac.Length, cipherBytes.Length);
                            
                            return Convert.ToBase64String(result);
                        }
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        public string Decrypt(string cipherText)
        {
            try
            {
                byte[] data = Convert.FromBase64String(cipherText);
                
                byte[] salt = new byte[32];
                byte[] iv = new byte[16];
                byte[] hmac = new byte[32];
                byte[] cipherBytes = new byte[data.Length - 80];
                
                Buffer.BlockCopy(data, 0, salt, 0, 32);
                Buffer.BlockCopy(data, 32, iv, 0, 16);
                Buffer.BlockCopy(data, 48, hmac, 0, 32);
                Buffer.BlockCopy(data, 80, cipherBytes, 0, cipherBytes.Length);
                
                using (Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(key, salt, iterations))
                {
                    byte[] derivedKey = pbkdf2.GetBytes(32);
                    
                    byte[] computedHmac = ComputeHmac(derivedKey, iv, cipherBytes);
                    if (!CompareBytes(hmac, computedHmac))
                        return null;
                    
                    using (Aes aes = Aes.Create())
                    {
                        aes.Key = derivedKey;
                        aes.IV = iv;
                        aes.Mode = CipherMode.CBC;
                        aes.Padding = PaddingMode.PKCS7;
                        
                        using (ICryptoTransform decryptor = aes.CreateDecryptor())
                        {
                            byte[] plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
                            return Encoding.UTF8.GetString(plainBytes);
                        }
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        private byte[] GenerateRandomBytes(int length)
        {
            byte[] bytes = new byte[length];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(bytes);
            }
            return bytes;
        }

        private byte[] ComputeHmac(byte[] key, byte[] iv, byte[] data)
        {
            using (HMACSHA256 hmac = new HMACSHA256(key))
            {
                byte[] combined = new byte[iv.Length + data.Length];
                Buffer.BlockCopy(iv, 0, combined, 0, iv.Length);
                Buffer.BlockCopy(data, 0, combined, iv.Length, data.Length);
                return hmac.ComputeHash(combined);
            }
        }

        private bool CompareBytes(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;
            
            int result = 0;
            for (int i = 0; i < a.Length; i++)
            {
                result |= a[i] ^ b[i];
            }
            return result == 0;
        }
    }
}
