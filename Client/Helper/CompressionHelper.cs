using System;
using System.IO;
using System.IO.Compression;

namespace Client.Helper
{
    /// <summary>
    /// Client-side compression helper
    /// Mirror của server CompressionHelper
    /// </summary>
    public static class CompressionHelper
    {
        public const int COMPRESSION_THRESHOLD = 1024;
        
        public static byte[] CompressGZip(byte[] data)
        {
            if (data == null || data.Length == 0)
                return data;
            
            using (var output = new MemoryStream())
            {
                using (var gzip = new GZipStream(output, CompressionLevel.Optimal))
                {
                    gzip.Write(data, 0, data.Length);
                }
                return output.ToArray();
            }
        }
        
        public static byte[] DecompressGZip(byte[] compressedData)
        {
            if (compressedData == null || compressedData.Length == 0)
                return compressedData;
            
            using (var input = new MemoryStream(compressedData))
            using (var gzip = new GZipStream(input, CompressionMode.Decompress))
            using (var output = new MemoryStream())
            {
                gzip.CopyTo(output);
                return output.ToArray();
            }
        }
        
        public static (byte[] data, bool wasCompressed) CompressIfBeneficial(byte[] data, bool forceCompress = false)
        {
            if (data == null || data.Length == 0)
                return (data, false);
            
            if (!forceCompress && data.Length < COMPRESSION_THRESHOLD)
                return (data, false);
            
            byte[] compressed = CompressGZip(data);
            
            if (compressed.Length < data.Length)
            {
                return (compressed, true);
            }
            else
            {
                return (data, false);
            }
        }
        
        public static bool ShouldCompress(int dataSize, bool isAlreadyCompressed = false)
        {
            if (isAlreadyCompressed)
                return false;
            
            return dataSize >= COMPRESSION_THRESHOLD;
        }
    }
}
