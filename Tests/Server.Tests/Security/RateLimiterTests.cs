using System;
using System.Threading;
using NUnit.Framework;
using Server.Security;

namespace Server.Tests.Security
{
    [TestFixture]
    public class RateLimiterTests
    {
        private RateLimiter limiter;
        private const string TestIp = "192.168.1.100";

        [SetUp]
        public void Setup()
        {
            limiter = new RateLimiter();
        }

        [Test]
        public void CheckConnectionRate_FirstConnection_ReturnsTrue()
        {
            // Act
            bool allowed = limiter.CheckConnectionRate(TestIp);

            // Assert
            Assert.IsTrue(allowed);
        }

        [Test]
        public void CheckConnectionRate_ExceedLimit_ReturnsFalse()
        {
            // Arrange
            int limit = 10; // Default limit per minute

            // Act
            for (int i = 0; i < limit + 5; i++)
            {
                limiter.CheckConnectionRate(TestIp);
            }
            bool allowed = limiter.CheckConnectionRate(TestIp);

            // Assert
            Assert.IsFalse(allowed);
        }

        [Test]
        public void CheckCommandRate_FirstCommand_ReturnsTrue()
        {
            // Act
            bool allowed = limiter.CheckCommandRate(TestIp);

            // Assert
            Assert.IsTrue(allowed);
        }

        [Test]
        public void CheckDataTransferRate_WithinLimit_ReturnsTrue()
        {
            // Arrange
            long smallData = 1024; // 1 KB

            // Act
            bool allowed = limiter.CheckDataTransferRate(TestIp, smallData);

            // Assert
            Assert.IsTrue(allowed);
        }

        [Test]
        public void CheckDataTransferRate_ExceedLimit_ReturnsFalse()
        {
            // Arrange
            long largeData = 11 * 1024 * 1024; // 11 MB (exceeds 10MB limit)

            // Act
            bool allowed = limiter.CheckDataTransferRate(TestIp, largeData);

            // Assert
            Assert.IsFalse(allowed);
        }

        [Test]
        public void GetStatistics_ReturnsNonNull()
        {
            // Arrange
            limiter.CheckConnectionRate(TestIp);
            limiter.CheckCommandRate(TestIp);

            // Act
            var stats = limiter.GetStatistics(TestIp);

            // Assert
            Assert.IsNotNull(stats);
        }

        [Test]
        public void Reset_ClearsLimits()
        {
            // Arrange
            for (int i = 0; i < 15; i++)
            {
                limiter.CheckConnectionRate(TestIp);
            }

            // Act
            limiter.Reset(TestIp);
            bool allowed = limiter.CheckConnectionRate(TestIp);

            // Assert
            Assert.IsTrue(allowed);
        }

        [Test]
        public void CheckConnectionRate_DifferentIps_Independent()
        {
            // Arrange
            string ip1 = "192.168.1.1";
            string ip2 = "192.168.1.2";

            // Act
            for (int i = 0; i < 15; i++)
            {
                limiter.CheckConnectionRate(ip1);
            }
            bool ip1Allowed = limiter.CheckConnectionRate(ip1);
            bool ip2Allowed = limiter.CheckConnectionRate(ip2);

            // Assert
            Assert.IsFalse(ip1Allowed);
            Assert.IsTrue(ip2Allowed);
        }

        [Test]
        public void Cleanup_RemovesOldEntries()
        {
            // Arrange
            limiter.CheckConnectionRate(TestIp);

            // Act
            limiter.Cleanup();

            // Assert - Just verify no exception
            Assert.DoesNotThrow(() => limiter.Cleanup());
        }
    }
}
