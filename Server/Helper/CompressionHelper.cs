using System;
using System.IO;
using System.IO.Compression;

namespace Server.Helper
{
    /// <summary>
    /// Compression utilities cho data transfer
    /// Sử dụng GZip và Brotli compression
    /// </summary>
    public static class CompressionHelper
    {
        // Threshold: Only compress data larger than 1KB
        public const int COMPRESSION_THRESHOLD = 1024;
        
        /// <summary>
        /// Compress data using GZip
        /// </summary>
        /// <param name="data">Data to compress</param>
        /// <returns>Compressed data</returns>
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
        
        /// <summary>
        /// Decompress GZip data
        /// </summary>
        /// <param name="compressedData">Compressed data</param>
        /// <returns>Decompressed data</returns>
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
        
        /// <summary>
        /// Compress data with automatic threshold check
        /// Returns original data if compression doesn't save space
        /// </summary>
        /// <param name="data">Data to compress</param>
        /// <param name="forceCompress">Force compression regardless of threshold</param>
        /// <returns>Tuple: (compressed data, was compressed)</returns>
        public static (byte[] data, bool wasCompressed) CompressIfBeneficial(byte[] data, bool forceCompress = false)
        {
            if (data == null || data.Length == 0)
                return (data, false);
            
            // Skip compression for small data unless forced
            if (!forceCompress && data.Length < COMPRESSION_THRESHOLD)
                return (data, false);
            
            byte[] compressed = CompressGZip(data);
            
            // Only use compressed if it's actually smaller
            if (compressed.Length < data.Length)
            {
                return (compressed, true);
            }
            else
            {
                return (data, false);
            }
        }
        
        /// <summary>
        /// Calculate compression ratio
        /// </summary>
        /// <param name="originalSize">Original data size</param>
        /// <param name="compressedSize">Compressed data size</param>
        /// <returns>Compression ratio (0.0 - 1.0)</returns>
        public static double GetCompressionRatio(int originalSize, int compressedSize)
        {
            if (originalSize == 0)
                return 0;
            
            return 1.0 - ((double)compressedSize / originalSize);
        }
        
        /// <summary>
        /// Check if data should be compressed based on size and type
        /// </summary>
        /// <param name="dataSize">Size of data</param>
        /// <param name="isAlreadyCompressed">Is data already compressed (e.g., images, videos)</param>
        /// <returns>True if should compress</returns>
        public static bool ShouldCompress(int dataSize, bool isAlreadyCompressed = false)
        {
            if (isAlreadyCompressed)
                return false;
            
            return dataSize >= COMPRESSION_THRESHOLD;
        }
    }
}
