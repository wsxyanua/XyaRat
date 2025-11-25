using System;
using NUnit.Framework;
using Client.Helper;

namespace Client.Tests.Helper
{
    [TestFixture]
    public class PluginCommunicationTests
    {
        private PluginCommunication comm;
        private const string TestPluginName = "TestPlugin";

        [SetUp]
        public void Setup()
        {
            comm = PluginCommunication.Instance;
        }

        [Test]
        public void Instance_ReturnsSameInstance()
        {
            // Act
            var instance1 = PluginCommunication.Instance;
            var instance2 = PluginCommunication.Instance;

            // Assert
            Assert.AreSame(instance1, instance2);
        }

        [Test]
        public void RegisterPlugin_NewPlugin_Succeeds()
        {
            // Act
            bool result = comm.RegisterPlugin(TestPluginName);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void RegisterPlugin_DuplicatePlugin_ReturnsFalse()
        {
            // Arrange
            comm.RegisterPlugin(TestPluginName);

            // Act
            bool result = comm.RegisterPlugin(TestPluginName);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void EnqueueMessage_WithRegisteredPlugin_Succeeds()
        {
            // Arrange
            comm.RegisterPlugin(TestPluginName);
            byte[] testData = new byte[] { 1, 2, 3, 4 };

            // Act
            bool result = comm.EnqueueMessage(TestPluginName, testData, 1);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void EnqueueMessage_WithUnregisteredPlugin_ReturnsFalse()
        {
            // Arrange
            byte[] testData = new byte[] { 1, 2, 3, 4 };

            // Act
            bool result = comm.EnqueueMessage("UnregisteredPlugin", testData, 1);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void DequeueMessage_AfterEnqueue_ReturnsMessage()
        {
            // Arrange
            comm.RegisterPlugin(TestPluginName);
            byte[] testData = new byte[] { 1, 2, 3, 4 };
            comm.EnqueueMessage(TestPluginName, testData, 1);

            // Act
            byte[] result = comm.DequeueMessage(TestPluginName);

            // Assert
            Assert.IsNotNull(result);
            CollectionAssert.AreEqual(testData, result);
        }

        [Test]
        public void DequeueMessage_EmptyQueue_ReturnsNull()
        {
            // Arrange
            comm.RegisterPlugin(TestPluginName);

            // Act
            byte[] result = comm.DequeueMessage(TestPluginName);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void GetPendingCount_AfterEnqueue_ReturnsCorrectCount()
        {
            // Arrange
            comm.RegisterPlugin(TestPluginName);
            byte[] testData = new byte[] { 1, 2, 3, 4 };
            comm.EnqueueMessage(TestPluginName, testData, 1);
            comm.EnqueueMessage(TestPluginName, testData, 1);

            // Act
            int count = comm.GetPendingCount(TestPluginName);

            // Assert
            Assert.AreEqual(2, count);
        }

        [Test]
        public void ClearQueue_RemovesAllMessages()
        {
            // Arrange
            comm.RegisterPlugin(TestPluginName);
            byte[] testData = new byte[] { 1, 2, 3, 4 };
            comm.EnqueueMessage(TestPluginName, testData, 1);
            comm.EnqueueMessage(TestPluginName, testData, 1);

            // Act
            comm.ClearQueue(TestPluginName);
            int count = comm.GetPendingCount(TestPluginName);

            // Assert
            Assert.AreEqual(0, count);
        }

        [Test]
        public void GetNextPriorityMessage_ReturnsHighestPriority()
        {
            // Arrange
            comm.RegisterPlugin(TestPluginName);
            byte[] lowPriority = new byte[] { 1, 2 };
            byte[] highPriority = new byte[] { 3, 4 };
            comm.EnqueueMessage(TestPluginName, lowPriority, 1);
            comm.EnqueueMessage(TestPluginName, highPriority, 10);

            // Act
            byte[] result = comm.GetNextPriorityMessage(TestPluginName);

            // Assert
            CollectionAssert.AreEqual(highPriority, result);
        }

        [Test]
        public void UnregisterPlugin_RemovesPlugin()
        {
            // Arrange
            comm.RegisterPlugin(TestPluginName);

            // Act
            comm.UnregisterPlugin(TestPluginName);
            bool canEnqueue = comm.EnqueueMessage(TestPluginName, new byte[] { 1 }, 1);

            // Assert
            Assert.IsFalse(canEnqueue);
        }

        [Test]
        public void GetMetrics_ReturnsNonNull()
        {
            // Arrange
            comm.RegisterPlugin(TestPluginName);

            // Act
            var metrics = comm.GetMetrics(TestPluginName);

            // Assert
            Assert.IsNotNull(metrics);
        }

        [TearDown]
        public void Cleanup()
        {
            // Clean up test plugin
            comm.UnregisterPlugin(TestPluginName);
        }
    }
}
