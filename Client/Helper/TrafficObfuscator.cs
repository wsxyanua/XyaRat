using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Client.Helper
{
    public static class TrafficObfuscator
    {
        private static readonly Random random = new Random();
        private static byte[] _obfuscationKey = null;
        
        /// <summary>
        /// Gets the obfuscation key (lazy initialization with secure derivation)
        /// </summary>
        private static byte[] ObfuscationKey
        {
            get
            {
                if (_obfuscationKey == null)
                {
                    _obfuscationKey = SecureKeyDerivation.GetObfuscationKey();
                }
                return _obfuscationKey;
            }
        }
        
        public static byte[] ObfuscateData(byte[] data)
        {
            try
            {
                int paddingSize = random.Next(16, 128);
                byte[] padding = GenerateRandomBytes(paddingSize);
                
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

        private static void XorObfuscate(byte[] data)
        {
            byte[] key = ObfuscationKey;
            for (int i = 0; i < data.Length; i++)
            {
                data[i] ^= key[i % key.Length];
            }
        }

        public static void AddRandomDelay()
        {
            try
            {
                int delay = random.Next(50, 500);
                System.Threading.Thread.Sleep(delay);
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleNonCritical(() => { }, ex, "AddRandomDelay failed");
            }
        }

        public static byte[] AddNoise(byte[] data, double noiseRatio = 0.1)
        {
            try
            {
                int noiseSize = (int)(data.Length * noiseRatio);
                byte[] noise = GenerateRandomBytes(noiseSize);
                
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

        public static byte[] GenerateRandomBytes(int length)
        {
            byte[] bytes = new byte[length];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(bytes);
            }
            return bytes;
        }

        public static byte[] ChunkData(byte[] data, int maxChunkSize = 1024)
        {
            try
            {
                if (data.Length <= maxChunkSize)
                    return data;
                
                int chunkSize = random.Next(maxChunkSize / 2, maxChunkSize);
                byte[] chunk = new byte[chunkSize];
                Buffer.BlockCopy(data, 0, chunk, 0, Math.Min(chunkSize, data.Length));
                
                return chunk;
            }
            catch
            {
                return data;
            }
        }

        public static string GenerateRandomSessionId()
        {
            return Guid.NewGuid().ToString("N");
        }

        public static long GetRandomTimestamp()
        {
            long currentTicks = DateTime.UtcNow.Ticks;
            long jitter = random.Next(-10000000, 10000000);
            return currentTicks + jitter;
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
    }
}
