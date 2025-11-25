using System;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace Server.Helper
{
    /// <summary>
    /// Performance metrics collection và monitoring
    /// </summary>
    public class PerformanceMetrics
    {
        private static readonly object lockObj = new object();
        private static PerformanceMetrics instance;
        
        public static PerformanceMetrics Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockObj)
                    {
                        if (instance == null)
                        {
                            instance = new PerformanceMetrics();
                        }
                    }
                }
                return instance;
            }
        }
        
        // Metrics storage
        private Dictionary<string, List<long>> executionTimes = new Dictionary<string, List<long>>();
        private Dictionary<string, int> callCounts = new Dictionary<string, int>();
        private Dictionary<string, long> totalBytes = new Dictionary<string, long>();
        private object metricsLock = new object();
        
        // Memory usage tracking
        private Process currentProcess;
        
        private PerformanceMetrics()
        {
            currentProcess = Process.GetCurrentProcess();
        }
        
        /// <summary>
        /// Record execution time for an operation
        /// </summary>
        public void RecordExecutionTime(string operationName, long milliseconds)
        {
            lock (metricsLock)
            {
                if (!executionTimes.ContainsKey(operationName))
                {
                    executionTimes[operationName] = new List<long>();
                }
                executionTimes[operationName].Add(milliseconds);
                
                if (!callCounts.ContainsKey(operationName))
                {
                    callCounts[operationName] = 0;
                }
                callCounts[operationName]++;
            }
        }
        
        /// <summary>
        /// Record data transfer
        /// </summary>
        public void RecordDataTransfer(string transferType, long bytes)
        {
            lock (metricsLock)
            {
                if (!totalBytes.ContainsKey(transferType))
                {
                    totalBytes[transferType] = 0;
                }
                totalBytes[transferType] += bytes;
            }
        }
        
        /// <summary>
        /// Get average execution time
        /// </summary>
        public double GetAverageExecutionTime(string operationName)
        {
            lock (metricsLock)
            {
                if (!executionTimes.ContainsKey(operationName) || executionTimes[operationName].Count == 0)
                    return 0;
                
                return executionTimes[operationName].Average();
            }
        }
        
        /// <summary>
        /// Get call count
        /// </summary>
        public int GetCallCount(string operationName)
        {
            lock (metricsLock)
            {
                if (!callCounts.ContainsKey(operationName))
                    return 0;
                
                return callCounts[operationName];
            }
        }
        
        /// <summary>
        /// Get total bytes transferred
        /// </summary>
        public long GetTotalBytes(string transferType)
        {
            lock (metricsLock)
            {
                if (!totalBytes.ContainsKey(transferType))
                    return 0;
                
                return totalBytes[transferType];
            }
        }
        
        /// <summary>
        /// Get current memory usage in MB
        /// </summary>
        public long GetMemoryUsageMB()
        {
            currentProcess.Refresh();
            return currentProcess.WorkingSet64 / (1024 * 1024);
        }
        
        /// <summary>
        /// Get all metrics as formatted string
        /// </summary>
        public string GetMetricsSummary()
        {
            lock (metricsLock)
            {
                var summary = "=== Performance Metrics ===\n\n";
                
                summary += "Execution Times:\n";
                foreach (var kvp in executionTimes)
                {
                    var avg = kvp.Value.Average();
                    var min = kvp.Value.Min();
                    var max = kvp.Value.Max();
                    var count = callCounts[kvp.Key];
                    
                    summary += $"  {kvp.Key}:\n";
                    summary += $"    Calls: {count}\n";
                    summary += $"    Avg: {avg:F2}ms\n";
                    summary += $"    Min: {min}ms\n";
                    summary += $"    Max: {max}ms\n\n";
                }
                
                summary += "Data Transfer:\n";
                foreach (var kvp in totalBytes)
                {
                    var mb = kvp.Value / (1024.0 * 1024.0);
                    summary += $"  {kvp.Key}: {mb:F2} MB\n";
                }
                
                summary += $"\nMemory Usage: {GetMemoryUsageMB()} MB\n";
                
                return summary;
            }
        }
        
        /// <summary>
        /// Clear all metrics
        /// </summary>
        public void Clear()
        {
            lock (metricsLock)
            {
                executionTimes.Clear();
                callCounts.Clear();
                totalBytes.Clear();
            }
        }
        
        /// <summary>
        /// Measure execution time của một function
        /// Usage: await PerformanceMetrics.Instance.MeasureAsync("OperationName", async () => { ... });
        /// </summary>
        public async Task<T> MeasureAsync<T>(string operationName, Func<Task<T>> operation)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                return await operation();
            }
            finally
            {
                sw.Stop();
                RecordExecutionTime(operationName, sw.ElapsedMilliseconds);
            }
        }
        
        /// <summary>
        /// Measure execution time (sync version)
        /// </summary>
        public T Measure<T>(string operationName, Func<T> operation)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                return operation();
            }
            finally
            {
                sw.Stop();
                RecordExecutionTime(operationName, sw.ElapsedMilliseconds);
            }
        }
    }
}
