using System;
using System.IO;
using NUnit.Framework;
using Server.Security;

namespace Server.Tests.Security
{
    [TestFixture]
    public class IpWhitelistTests
    {
        private IpWhitelist whitelist;
        private const string TestIp = "192.168.1.100";
        private const string ConfigPath = "test_ipconfig.txt";

        [SetUp]
        public void Setup()
        {
            whitelist = new IpWhitelist();
            
            // Clean up config file if exists
            if (File.Exists(ConfigPath))
            {
                File.Delete(ConfigPath);
            }
        }

        [TearDown]
        public void Cleanup()
        {
            if (File.Exists(ConfigPath))
            {
                File.Delete(ConfigPath);
            }
        }

        [Test]
        public void IsAllowed_NewIp_ReturnsTrue()
        {
            // Act
            bool allowed = whitelist.IsAllowed(TestIp);

            // Assert
            Assert.IsTrue(allowed); // By default, all IPs allowed if no whitelist
        }

        [Test]
        public void AddToBlacklist_BlocksIp()
        {
            // Act
            whitelist.AddToBlacklist(TestIp);
            bool allowed = whitelist.IsAllowed(TestIp);

            // Assert
            Assert.IsFalse(allowed);
        }

        [Test]
        public void RemoveFromBlacklist_AllowsIp()
        {
            // Arrange
            whitelist.AddToBlacklist(TestIp);

            // Act
            whitelist.RemoveFromBlacklist(TestIp);
            bool allowed = whitelist.IsAllowed(TestIp);

            // Assert
            Assert.IsTrue(allowed);
        }

        [Test]
        public void AddToWhitelist_AllowsIp()
        {
            // Act
            whitelist.AddToWhitelist(TestIp);
            bool allowed = whitelist.IsAllowed(TestIp);

            // Assert
            Assert.IsTrue(allowed);
        }

        [Test]
        public void RecordFailedAttempt_IncreasesCount()
        {
            // Act
            int count1 = whitelist.GetFailedAttempts(TestIp);
            whitelist.RecordFailedAttempt(TestIp);
            int count2 = whitelist.GetFailedAttempts(TestIp);

            // Assert
            Assert.AreEqual(0, count1);
            Assert.AreEqual(1, count2);
        }

        [Test]
        public void RecordFailedAttempt_ExceedThreshold_AutoBlacklists()
        {
            // Arrange
            int threshold = 5;

            // Act
            for (int i = 0; i < threshold + 1; i++)
            {
                whitelist.RecordFailedAttempt(TestIp, threshold);
            }
            bool allowed = whitelist.IsAllowed(TestIp);

            // Assert
            Assert.IsFalse(allowed);
        }

        [Test]
        public void IsBlacklisted_ChecksCorrectly()
        {
            // Arrange
            whitelist.AddToBlacklist(TestIp);

            // Act
            bool blacklisted = whitelist.IsBlacklisted(TestIp);

            // Assert
            Assert.IsTrue(blacklisted);
        }

        [Test]
        public void IsWhitelisted_ChecksCorrectly()
        {
            // Arrange
            whitelist.AddToWhitelist(TestIp);

            // Act
            bool whitelisted = whitelist.IsWhitelisted(TestIp);

            // Assert
            Assert.IsTrue(whitelisted);
        }

        [Test]
        public void LoadConfiguration_WithNonExistentFile_DoesNotThrow()
        {
            // Act & Assert
            Assert.DoesNotThrow(() =>
            {
                whitelist.LoadConfiguration("nonexistent.txt");
            });
        }

        [Test]
        public void SaveConfiguration_CreatesFile()
        {
            // Arrange
            whitelist.AddToWhitelist(TestIp);

            // Act
            whitelist.SaveConfiguration(ConfigPath);

            // Assert
            Assert.IsTrue(File.Exists(ConfigPath));
        }
    }
}
