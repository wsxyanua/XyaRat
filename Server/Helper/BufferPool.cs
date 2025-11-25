using System;
using System.Buffers;
using System.Collections.Generic;

namespace Server.Helper
{
    /// <summary>
    /// Buffer pooling để giảm memory allocations
    /// Sử dụng ArrayPool<byte>.Shared cho performance tốt hơn
    /// </summary>
    public static class BufferPool
    {
        private static readonly ArrayPool<byte> Pool = ArrayPool<byte>.Shared;
        
        /// <summary>
        /// Rent một buffer từ pool
        /// </summary>
        /// <param name="minimumLength">Minimum size cần thiết</param>
        /// <returns>Buffer với size >= minimumLength</returns>
        public static byte[] Rent(int minimumLength)
        {
            return Pool.Rent(minimumLength);
        }
        
        /// <summary>
        /// Return buffer về pool để reuse
        /// CRITICAL: Phải gọi sau khi dùng xong buffer
        /// </summary>
        /// <param name="buffer">Buffer cần return</param>
        /// <param name="clearArray">Clear array trước khi return (security)</param>
        public static void Return(byte[] buffer, bool clearArray = true)
        {
            if (buffer != null)
            {
                Pool.Return(buffer, clearArray);
            }
        }
        
        /// <summary>
        /// Rent buffer và tự động return khi dispose
        /// Usage: using (var rental = BufferPool.RentAuto(1024)) { ... }
        /// </summary>
        public static BufferRental RentAuto(int minimumLength)
        {
            return new BufferRental(minimumLength);
        }
    }
    
    /// <summary>
    /// Auto-disposable buffer rental
    /// Tự động return buffer khi dispose
    /// </summary>
    public class BufferRental : IDisposable
    {
        public byte[] Buffer { get; private set; }
        private bool disposed = false;
        
        public BufferRental(int minimumLength)
        {
            Buffer = BufferPool.Rent(minimumLength);
        }
        
        public void Dispose()
        {
            if (!disposed)
            {
                BufferPool.Return(Buffer);
                Buffer = null;
                disposed = true;
            }
        }
    }
    
    /// <summary>
    /// Object pooling cho frequently allocated objects
    /// Generic pool cho bất kỳ object type nào
    /// </summary>
    public class ObjectPool<T> where T : class, new()
    {
        private readonly Stack<T> pool = new Stack<T>();
        private readonly object lockObj = new object();
        private readonly int maxSize;
        
        public ObjectPool(int maxSize = 100)
        {
            this.maxSize = maxSize;
        }
        
        /// <summary>
        /// Get object từ pool hoặc create new
        /// </summary>
        public T Rent()
        {
            lock (lockObj)
            {
                if (pool.Count > 0)
                {
                    return pool.Pop();
                }
            }
            return new T();
        }
        
        /// <summary>
        /// Return object về pool
        /// </summary>
        public void Return(T item)
        {
            if (item == null) return;
            
            lock (lockObj)
            {
                if (pool.Count < maxSize)
                {
                    pool.Push(item);
                }
            }
        }
        
        /// <summary>
        /// Clear pool
        /// </summary>
        public void Clear()
        {
            lock (lockObj)
            {
                pool.Clear();
            }
        }
        
        /// <summary>
        /// Get current pool size
        /// </summary>
        public int Count
        {
            get
            {
                lock (lockObj)
                {
                    return pool.Count;
                }
            }
        }
    }
}
