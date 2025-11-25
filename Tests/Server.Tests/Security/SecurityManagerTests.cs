using System;
using NUnit.Framework;
using Server.Security;

namespace Server.Tests.Security
{
    [TestFixture]
    public class SecurityManagerTests
    {
        private SecurityManager manager;
        private const string TestIp = "192.168.1.100";

        [SetUp]
        public void Setup()
        {
            manager = SecurityManager.Instance;
            manager.Reset(); // Reset for clean state
        }

        [Test]
        public void Instance_ReturnsSameInstance()
        {
            // Act
            var instance1 = SecurityManager.Instance;
            var instance2 = SecurityManager.Instance;

            // Assert
            Assert.AreSame(instance1, instance2);
        }

        [Test]
        public void ValidateConnection_NewIp_ReturnsTrue()
        {
            // Act
            bool allowed = manager.ValidateConnection(TestIp);

            // Assert
            Assert.IsTrue(allowed);
        }

        [Test]
        public void ValidateConnection_ExceedRate_ReturnsFalse()
        {
            // Arrange
            for (int i = 0; i < 15; i++)
            {
                manager.ValidateConnection(TestIp);
            }

            // Act
            bool allowed = manager.ValidateConnection(TestIp);

            // Assert
            Assert.IsFalse(allowed);
        }

        [Test]
        public void ValidateCommand_FirstCommand_ReturnsTrue()
        {
            // Act
            bool allowed = manager.ValidateCommand(TestIp);

            // Assert
            Assert.IsTrue(allowed);
        }

        [Test]
        public void ValidateDataTransfer_SmallData_ReturnsTrue()
        {
            // Arrange
            long dataSize = 1024; // 1 KB

            // Act
            bool allowed = manager.ValidateDataTransfer(TestIp, dataSize);

            // Assert
            Assert.IsTrue(allowed);
        }

        [Test]
        public void ValidateDataTransfer_LargeData_ReturnsFalse()
        {
            // Arrange
            long dataSize = 15 * 1024 * 1024; // 15 MB

            // Act
            bool allowed = manager.ValidateDataTransfer(TestIp, dataSize);

            // Assert
            Assert.IsFalse(allowed);
        }

        [Test]
        public void BlockIp_PreventsConnection()
        {
            // Act
            manager.BlockIp(TestIp);
            bool allowed = manager.ValidateConnection(TestIp);

            // Assert
            Assert.IsFalse(allowed);
        }

        [Test]
        public void UnblockIp_AllowsConnection()
        {
            // Arrange
            manager.BlockIp(TestIp);

            // Act
            manager.UnblockIp(TestIp);
            bool allowed = manager.ValidateConnection(TestIp);

            // Assert
            Assert.IsTrue(allowed);
        }

        [Test]
        public void WhitelistIp_AlwaysAllows()
        {
            // Arrange
            manager.WhitelistIp(TestIp);

            // Act - Try to exceed rate limit
            for (int i = 0; i < 20; i++)
            {
                manager.ValidateConnection(TestIp);
            }
            bool allowed = manager.ValidateConnection(TestIp);

            // Assert
            Assert.IsTrue(allowed);
        }

        [Test]
        public void GetStatistics_ReturnsNonNull()
        {
            // Arrange
            manager.ValidateConnection(TestIp);
            manager.ValidateCommand(TestIp);

            // Act
            var stats = manager.GetStatistics(TestIp);

            // Assert
            Assert.IsNotNull(stats);
        }

        [Test]
        public void Reset_ClearsAllLimits()
        {
            // Arrange
            for (int i = 0; i < 15; i++)
            {
                manager.ValidateConnection(TestIp);
            }

            // Act
            manager.Reset();
            bool allowed = manager.ValidateConnection(TestIp);

            // Assert
            Assert.IsTrue(allowed);
        }

        [Test]
        public void EnableRateLimiting_ControlsRateLimit()
        {
            // Arrange
            manager.EnableRateLimiting(false);

            // Act - Try to exceed rate limit
            for (int i = 0; i < 20; i++)
            {
                manager.ValidateConnection(TestIp);
            }
            bool allowed = manager.ValidateConnection(TestIp);

            // Assert
            Assert.IsTrue(allowed); // Should pass because rate limiting is disabled
        }

        [Test]
        public void EnableIpFiltering_ControlsFiltering()
        {
            // Arrange
            manager.EnableIpFiltering(false);
            manager.BlockIp(TestIp);

            // Act
            bool allowed = manager.ValidateConnection(TestIp);

            // Assert
            Assert.IsTrue(allowed); // Should pass because filtering is disabled
        }
    }
}
