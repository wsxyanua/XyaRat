using System;
using System.Collections.Generic;
using MessagePackLib.MessagePack;

namespace Client.Helper
{
    /// <summary>
    /// Plugin communication system - Handles message routing between plugins and server
    /// Implements priority queuing and rate limiting per plugin
    /// </summary>
    public class PluginCommunication
    {
        private static PluginCommunication _instance;
        private Dictionary<string, Queue<MsgPack>> _pluginQueues;
        private Dictionary<string, DateTime> _lastSendTimes;
        private Dictionary<string, int> _sendCounts;
        private readonly int _rateLimitInterval = 1000; // 1 second
        private readonly int _maxMessagesPerInterval = 10;
        
        public static PluginCommunication Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new PluginCommunication();
                return _instance;
            }
        }
        
        private PluginCommunication()
        {
            _pluginQueues = new Dictionary<string, Queue<MsgPack>>();
            _lastSendTimes = new Dictionary<string, DateTime>();
            _sendCounts = new Dictionary<string, int>();
        }
        
        /// <summary>
        /// Registers a plugin for communication
        /// </summary>
        public void RegisterPlugin(string pluginName)
        {
            if (!_pluginQueues.ContainsKey(pluginName))
            {
                _pluginQueues[pluginName] = new Queue<MsgPack>();
                _lastSendTimes[pluginName] = DateTime.MinValue;
                _sendCounts[pluginName] = 0;
                
                Logger.Log($"[PluginComm] Registered plugin: {pluginName}", Logger.LogLevel.Info);
            }
        }
        
        /// <summary>
        /// Enqueues a message from plugin with priority
        /// </summary>
        public bool EnqueueMessage(string pluginName, MsgPack message, int priority = 0)
        {
            try
            {
                // Check rate limit
                if (!CheckRateLimit(pluginName))
                {
                    Logger.Log($"[PluginComm] Rate limit exceeded for {pluginName}", Logger.LogLevel.Warning);
                    return false;
                }
                
                // Register plugin if not exists
                if (!_pluginQueues.ContainsKey(pluginName))
                {
                    RegisterPlugin(pluginName);
                }
                
                // Add plugin identifier to message
                message.ForcePathObject("PluginName").AsString = pluginName;
                message.ForcePathObject("Priority").AsInteger = priority;
                message.ForcePathObject("Timestamp").AsString = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                
                _pluginQueues[pluginName].Enqueue(message);
                
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log($"[PluginComm] Enqueue error: {ex.Message}", Logger.LogLevel.Error);
                return false;
            }
        }
        
        /// <summary>
        /// Dequeues next message for sending
        /// </summary>
        public MsgPack DequeueMessage(string pluginName)
        {
            try
            {
                if (_pluginQueues.ContainsKey(pluginName) && _pluginQueues[pluginName].Count > 0)
                {
                    return _pluginQueues[pluginName].Dequeue();
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"[PluginComm] Dequeue error: {ex.Message}", Logger.LogLevel.Error);
            }
            
            return null;
        }
        
        /// <summary>
        /// Gets next high-priority message from any plugin
        /// </summary>
        public MsgPack GetNextPriorityMessage()
        {
            try
            {
                MsgPack highestPriorityMsg = null;
                string selectedPlugin = null;
                int highestPriority = int.MinValue;
                
                foreach (var kvp in _pluginQueues)
                {
                    if (kvp.Value.Count == 0)
                        continue;
                    
                    MsgPack msg = kvp.Value.Peek();
                    int priority = msg.ForcePathObject("Priority").AsInteger;
                    
                    if (priority > highestPriority)
                    {
                        highestPriority = priority;
                        highestPriorityMsg = msg;
                        selectedPlugin = kvp.Key;
                    }
                }
                
                if (highestPriorityMsg != null && selectedPlugin != null)
                {
                    _pluginQueues[selectedPlugin].Dequeue();
                    return highestPriorityMsg;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"[PluginComm] Priority queue error: {ex.Message}", Logger.LogLevel.Error);
            }
            
            return null;
        }
        
        /// <summary>
        /// Checks if plugin exceeds rate limit
        /// </summary>
        private bool CheckRateLimit(string pluginName)
        {
            try
            {
                DateTime now = DateTime.Now;
                
                if (!_lastSendTimes.ContainsKey(pluginName))
                {
                    _lastSendTimes[pluginName] = now;
                    _sendCounts[pluginName] = 1;
                    return true;
                }
                
                TimeSpan elapsed = now - _lastSendTimes[pluginName];
                
                if (elapsed.TotalMilliseconds >= _rateLimitInterval)
                {
                    // Reset counter
                    _lastSendTimes[pluginName] = now;
                    _sendCounts[pluginName] = 1;
                    return true;
                }
                
                // Check if within limit
                if (_sendCounts[pluginName] < _maxMessagesPerInterval)
                {
                    _sendCounts[pluginName]++;
                    return true;
                }
                
                return false;
            }
            catch
            {
                return true; // Allow on error
            }
        }
        
        /// <summary>
        /// Gets pending message count for plugin
        /// </summary>
        public int GetPendingCount(string pluginName)
        {
            if (_pluginQueues.ContainsKey(pluginName))
            {
                return _pluginQueues[pluginName].Count;
            }
            return 0;
        }
        
        /// <summary>
        /// Clears all pending messages for plugin
        /// </summary>
        public void ClearQueue(string pluginName)
        {
            if (_pluginQueues.ContainsKey(pluginName))
            {
                _pluginQueues[pluginName].Clear();
                Logger.Log($"[PluginComm] Cleared queue for {pluginName}", Logger.LogLevel.Info);
            }
        }
        
        /// <summary>
        /// Gets metrics for all plugins
        /// </summary>
        public Dictionary<string, int> GetMetrics()
        {
            Dictionary<string, int> metrics = new Dictionary<string, int>();
            
            foreach (var kvp in _pluginQueues)
            {
                metrics[kvp.Key] = kvp.Value.Count;
            }
            
            return metrics;
        }
        
        /// <summary>
        /// Unregisters a plugin
        /// </summary>
        public void UnregisterPlugin(string pluginName)
        {
            if (_pluginQueues.ContainsKey(pluginName))
            {
                _pluginQueues[pluginName].Clear();
                _pluginQueues.Remove(pluginName);
                _lastSendTimes.Remove(pluginName);
                _sendCounts.Remove(pluginName);
                
                Logger.Log($"[PluginComm] Unregistered plugin: {pluginName}", Logger.LogLevel.Info);
            }
        }
    }
}
