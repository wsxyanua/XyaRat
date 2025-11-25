using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Server.Security
{
    public class ConnectionThrottle
    {
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _clientSemaphores;
        private readonly int _maxConcurrentOperations;
        private readonly TimeSpan _operationTimeout;
        
        public ConnectionThrottle(int maxConcurrentOperations = 5, TimeSpan? operationTimeout = null)
        {
            _clientSemaphores = new ConcurrentDictionary<string, SemaphoreSlim>();
            _maxConcurrentOperations = maxConcurrentOperations;
            _operationTimeout = operationTimeout ?? TimeSpan.FromSeconds(30);
        }
        
        public async Task<bool> ExecuteThrottledAsync(string clientId, Func<Task> operation)
        {
            var semaphore = GetOrCreateSemaphore(clientId);
            
            bool acquired = await semaphore.WaitAsync(_operationTimeout);
            
            if (!acquired)
            {
                return false; // Timeout - too many concurrent operations
            }
            
            try
            {
                await operation();
                return true;
            }
            finally
            {
                semaphore.Release();
            }
        }
        
        public bool ExecuteThrottled(string clientId, Action operation)
        {
            var semaphore = GetOrCreateSemaphore(clientId);
            
            bool acquired = semaphore.Wait(_operationTimeout);
            
            if (!acquired)
            {
                return false;
            }
            
            try
            {
                operation();
                return true;
            }
            finally
            {
                semaphore.Release();
            }
        }
        
        public int GetActiveOperations(string clientId)
        {
            if (_clientSemaphores.TryGetValue(clientId, out var semaphore))
            {
                return _maxConcurrentOperations - semaphore.CurrentCount;
            }
            
            return 0;
        }
        
        public void ResetClient(string clientId)
        {
            if (_clientSemaphores.TryRemove(clientId, out var semaphore))
            {
                semaphore.Dispose();
            }
        }
        
        private SemaphoreSlim GetOrCreateSemaphore(string clientId)
        {
            return _clientSemaphores.GetOrAdd(clientId, 
                _ => new SemaphoreSlim(_maxConcurrentOperations, _maxConcurrentOperations));
        }
        
        public void Dispose()
        {
            foreach (var semaphore in _clientSemaphores.Values)
            {
                semaphore.Dispose();
            }
            
            _clientSemaphores.Clear();
        }
    }
}
