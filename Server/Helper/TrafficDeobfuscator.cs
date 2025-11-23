using System;
using System.Text;

namespace Server.Helper
{
    public static class TrafficDeobfuscator
    {
        public static byte[] DeobfuscateData(byte[] obfuscatedData)
        {
            try
            {
                XorObfuscate(obfuscatedData);
                
                int dataLength = BitConverter.ToInt32(obfuscatedData, 0);
                
                if (dataLength <= 0 || dataLength > obfuscatedData.Length - 4)
                    return null;
                
                byte[] data = new byte[dataLength];
                Buffer.BlockCopy(obfuscatedData, 4, data, 0, dataLength);
                
                return data;
            }
            catch
            {
                return null;
            }
        }

        public static byte[] ObfuscateData(byte[] data)
        {
            try
            {
                Random random = new Random();
                int paddingSize = random.Next(16, 128);
                byte[] padding = new byte[paddingSize];
                new System.Security.Cryptography.RNGCryptoServiceProvider().GetBytes(padding);
                
                byte[] obfuscated = new byte[data.Length + padding.Length + 4];
                
                byte[] dataLengthBytes = BitConverter.GetBytes(data.Length);
                Buffer.BlockCopy(dataLengthBytes, 0, obfuscated, 0, 4);
                Buffer.BlockCopy(data, 0, obfuscated, 4, data.Length);
                Buffer.BlockCopy(padding, 0, obfuscated, 4 + data.Length, padding.Length);
                
                XorObfuscate(obfuscated);
                
                return obfuscated;
            }
            catch
            {
                return data;
            }
        }

        private static void XorObfuscate(byte[] data)
        {
            byte[] key = Encoding.UTF8.GetBytes("XyaObfuscationKey2025");
            for (int i = 0; i < data.Length; i++)
            {
                data[i] ^= key[i % key.Length];
            }
        }

        public static byte[] RemoveNoise(byte[] noisyData)
        {
            try
            {
                int noiseSize = BitConverter.ToInt32(noisyData, 0);
                
                if (noiseSize < 0 || noiseSize > noisyData.Length - 4)
                    return null;
                
                int dataLength = noisyData.Length - 4 - noiseSize;
                byte[] data = new byte[dataLength];
                
                Buffer.BlockCopy(noisyData, 4, data, 0, dataLength);
                
                return data;
            }
            catch
            {
                return null;
            }
        }

        public static byte[] AddNoise(byte[] data, double noiseRatio = 0.1)
        {
            try
            {
                Random random = new Random();
                int noiseSize = (int)(data.Length * noiseRatio);
                byte[] noise = new byte[noiseSize];
                new System.Security.Cryptography.RNGCryptoServiceProvider().GetBytes(noise);
                
                byte[] result = new byte[data.Length + noiseSize + 4];
                byte[] noiseSizeBytes = BitConverter.GetBytes(noiseSize);
                
                Buffer.BlockCopy(noiseSizeBytes, 0, result, 0, 4);
                Buffer.BlockCopy(data, 0, result, 4, data.Length);
                Buffer.BlockCopy(noise, 0, result, 4 + data.Length, noiseSize);
                
                return result;
            }
            catch
            {
                return data;
            }
        }

        public static byte[] RemoveMultiLayerObfuscation(byte[] obfuscatedData)
        {
            try
            {
                byte[] result = obfuscatedData;
                
                result = DeobfuscateData(result);
                if (result == null) return null;
                
                result = RemoveNoise(result);
                
                return result;
            }
            catch
            {
                return null;
            }
        }

        public static byte[] ApplyMultiLayerObfuscation(byte[] data)
        {
            try
            {
                byte[] result = data;
                
                result = AddNoise(result, 0.15);
                
                result = ObfuscateData(result);
                
                return result;
            }
            catch
            {
                return data;
            }
        }
    }
}
