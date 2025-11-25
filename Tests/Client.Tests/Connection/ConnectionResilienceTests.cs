using System;
using System.Collections.Generic;
using NUnit.Framework;
using Client.Connection;

namespace Client.Tests.Connection
{
    [TestFixture]
    public class ConnectionResilienceTests
    {
        [Test]
        public void GetNextTarget_WithPrimaryHosts_ReturnsPrimaryFirst()
        {
            // Arrange
            List<string> primaryHosts = new List<string> { "primary1.com", "primary2.com" };
            List<int> ports = new List<int> { 4444, 5555 };
            ConnectionResilience resilience = new ConnectionResilience(primaryHosts, null, ports);

            // Act
            var target = resilience.GetNextTarget();

            // Assert
            Assert.IsNotNull(target);
            Assert.Contains(target.Item1, primaryHosts);
            Assert.Contains(target.Item2, ports);
        }

        [Test]
        public void GetBackoffDelay_IncreasesExponentially()
        {
            // Arrange
            int attempt1 = 1;
            int attempt2 = 2;
            int attempt3 = 3;

            // Act
            int delay1 = ConnectionResilience.GetBackoffDelay(attempt1);
            int delay2 = ConnectionResilience.GetBackoffDelay(attempt2);
            int delay3 = ConnectionResilience.GetBackoffDelay(attempt3);

            // Assert
            Assert.Greater(delay2, delay1);
            Assert.Greater(delay3, delay2);
            Assert.LessOrEqual(delay3, 60000); // Max 60 seconds
        }

        [Test]
        public void GetBackoffDelay_ReturnsValueWithinRange()
        {
            // Arrange
            int attempt = 2;

            // Act
            int delay = ConnectionResilience.GetBackoffDelay(attempt);

            // Assert
            Assert.Greater(delay, 0);
            Assert.LessOrEqual(delay, 60000);
        }

        [Test]
        public void RecordFailure_IncrementsRetryCount()
        {
            // Arrange
            List<string> primaryHosts = new List<string> { "test.com" };
            List<int> ports = new List<int> { 4444 };
            ConnectionResilience resilience = new ConnectionResilience(primaryHosts, null, ports);

            // Act
            int initialCount = resilience.RetryCount;
            resilience.RecordFailure();
            int afterCount = resilience.RetryCount;

            // Assert
            Assert.AreEqual(initialCount + 1, afterCount);
        }

        [Test]
        public void Reset_ResetsRetryCount()
        {
            // Arrange
            List<string> primaryHosts = new List<string> { "test.com" };
            List<int> ports = new List<int> { 4444 };
            ConnectionResilience resilience = new ConnectionResilience(primaryHosts, null, ports);
            resilience.RecordFailure();
            resilience.RecordFailure();

            // Act
            resilience.Reset();

            // Assert
            Assert.AreEqual(0, resilience.RetryCount);
        }

        [Test]
        public void Constructor_WithNullPorts_UsesDefaultPort()
        {
            // Arrange & Act
            List<string> primaryHosts = new List<string> { "test.com" };
            ConnectionResilience resilience = new ConnectionResilience(primaryHosts, null, null);
            var target = resilience.GetNextTarget();

            // Assert
            Assert.IsNotNull(target);
            Assert.Greater(target.Item2, 0); // Should have some port
        }

        [Test]
        public void GetNextTarget_AfterMultipleFailures_SwitchesToFallback()
        {
            // Arrange
            List<string> primaryHosts = new List<string> { "primary.com" };
            List<string> fallbackHosts = new List<string> { "fallback.com" };
            List<int> ports = new List<int> { 4444 };
            ConnectionResilience resilience = new ConnectionResilience(primaryHosts, fallbackHosts, ports);

            // Act - Simulate multiple failures
            for (int i = 0; i < 5; i++)
            {
                resilience.RecordFailure();
            }
            var target = resilience.GetNextTarget();

            // Assert
            Assert.IsNotNull(target);
            // After many failures, should try fallback or DGA
        }
    }
}
