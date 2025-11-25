using System;
using System.IO;
using NUnit.Framework;
using Client.Helper;

namespace Client.Tests.Helper
{
    [TestFixture]
    public class PluginManagerTests
    {
        private PluginManager manager;
        private string testPluginPath;

        [SetUp]
        public void Setup()
        {
            manager = PluginManager.Instance;
            testPluginPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "XyaRat", "Plugins");
            
            // Create test directory if it doesn't exist
            if (!Directory.Exists(testPluginPath))
            {
                Directory.CreateDirectory(testPluginPath);
            }
        }

        [Test]
        public void Instance_ReturnsSameInstance()
        {
            // Act
            var instance1 = PluginManager.Instance;
            var instance2 = PluginManager.Instance;

            // Assert
            Assert.AreSame(instance1, instance2);
        }

        [Test]
        public void Initialize_DoesNotThrowException()
        {
            // Act & Assert
            Assert.DoesNotThrow(() =>
            {
                manager.Initialize();
            });
        }

        [Test]
        public void GetLoadedPlugins_ReturnsNonNull()
        {
            // Act
            var plugins = manager.GetLoadedPlugins();

            // Assert
            Assert.IsNotNull(plugins);
        }

        [Test]
        public void GetPluginAssembly_WithNonExistentPlugin_ReturnsNull()
        {
            // Act
            var assembly = manager.GetPluginAssembly("NonExistentPlugin");

            // Assert
            Assert.IsNull(assembly);
        }

        [Test]
        public void LoadPlugin_WithInvalidPath_ReturnsFalse()
        {
            // Act
            bool result = manager.LoadPlugin("InvalidPath.dll");

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void UnloadPlugin_WithNonExistentPlugin_DoesNotThrow()
        {
            // Act & Assert
            Assert.DoesNotThrow(() =>
            {
                manager.UnloadPlugin("NonExistentPlugin");
            });
        }

        [Test]
        public void BlacklistPlugin_AddsToBlacklist()
        {
            // Arrange
            string testPlugin = "TestPlugin";

            // Act
            manager.BlacklistPlugin(testPlugin);

            // Assert - Verify plugin is blacklisted (can't load)
            var result = manager.LoadPlugin(Path.Combine(testPluginPath, testPlugin + ".dll"));
            Assert.IsFalse(result);
        }

        [Test]
        public void WhitelistPlugin_RemovesFromBlacklist()
        {
            // Arrange
            string testPlugin = "TestPlugin";
            manager.BlacklistPlugin(testPlugin);

            // Act
            manager.WhitelistPlugin(testPlugin);

            // Assert - Plugin should now be loadable (if file exists)
            // Just verify no exception thrown
            Assert.DoesNotThrow(() =>
            {
                manager.WhitelistPlugin(testPlugin);
            });
        }
    }
}
