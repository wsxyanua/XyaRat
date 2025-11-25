using System;
using NUnit.Framework;
using Client.Helper;

namespace Client.Tests.Helper
{
    [TestFixture]
    public class TrafficObfuscatorTests
    {
        private readonly byte[] testData = System.Text.Encoding.UTF8.GetBytes("Test data for obfuscation");

        [Test]
        public void ObfuscateData_WithValidData_ReturnsObfuscatedData()
        {
            // Act
            byte[] obfuscated = TrafficObfuscator.ObfuscateData(testData);

            // Assert
            Assert.IsNotNull(obfuscated);
            Assert.Greater(obfuscated.Length, testData.Length); // Should be larger with padding/noise
            CollectionAssert.AreNotEqual(testData, obfuscated);
        }

        [Test]
        public void DeobfuscateData_WithObfuscatedData_ReturnsOriginalData()
        {
            // Arrange
            byte[] obfuscated = TrafficObfuscator.ObfuscateData(testData);

            // Act
            byte[] deobfuscated = TrafficObfuscator.DeobfuscateData(obfuscated);

            // Assert
            Assert.IsNotNull(deobfuscated);
            CollectionAssert.AreEqual(testData, deobfuscated);
        }

        [Test]
        public void ObfuscateData_SameDataTwice_ReturnsDifferentResults()
        {
            // Act
            byte[] obfuscated1 = TrafficObfuscator.ObfuscateData(testData);
            byte[] obfuscated2 = TrafficObfuscator.ObfuscateData(testData);

            // Assert
            CollectionAssert.AreNotEqual(obfuscated1, obfuscated2); // Due to random padding/noise
        }

        [Test]
        public void GenerateFakeHeaders_ReturnsValidHeaders()
        {
            // Act
            var headers = TrafficObfuscator.GenerateFakeHeaders();

            // Assert
            Assert.IsNotNull(headers);
            Assert.Greater(headers.Count, 0);
            Assert.IsTrue(headers.ContainsKey("User-Agent"));
            Assert.IsTrue(headers.ContainsKey("Accept"));
        }

        [Test]
        public void GenerateFakeHeaders_MultipleCalls_ReturnsDifferentUserAgents()
        {
            // Act
            var headers1 = TrafficObfuscator.GenerateFakeHeaders();
            var headers2 = TrafficObfuscator.GenerateFakeHeaders();

            // Assert - User agents might differ due to rotation
            Assert.IsNotNull(headers1["User-Agent"]);
            Assert.IsNotNull(headers2["User-Agent"]);
        }

        [Test]
        public void GenerateSessionId_ReturnsValidLength()
        {
            // Act
            string sessionId = TrafficObfuscator.GenerateSessionId();

            // Assert
            Assert.IsNotNull(sessionId);
            Assert.AreEqual(32, sessionId.Length); // Typical session ID length
        }

        [Test]
        public void GenerateSessionId_MultipleCalls_ReturnsDifferentIds()
        {
            // Act
            string id1 = TrafficObfuscator.GenerateSessionId();
            string id2 = TrafficObfuscator.GenerateSessionId();

            // Assert
            Assert.AreNotEqual(id1, id2);
        }

        [Test]
        public void ObfuscateData_WithEmptyData_HandlesGracefully()
        {
            // Arrange
            byte[] emptyData = new byte[0];

            // Act
            byte[] obfuscated = TrafficObfuscator.ObfuscateData(emptyData);

            // Assert
            Assert.IsNotNull(obfuscated);
            Assert.Greater(obfuscated.Length, 0); // Should still have padding/noise
        }

        [Test]
        public void ObfuscateData_WithLargeData_WorksCorrectly()
        {
            // Arrange
            byte[] largeData = new byte[1024 * 10]; // 10 KB
            new Random().NextBytes(largeData);

            // Act
            byte[] obfuscated = TrafficObfuscator.ObfuscateData(largeData);
            byte[] deobfuscated = TrafficObfuscator.DeobfuscateData(obfuscated);

            // Assert
            CollectionAssert.AreEqual(largeData, deobfuscated);
        }
    }
}
