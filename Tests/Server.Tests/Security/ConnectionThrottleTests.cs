using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Server.Security;

namespace Server.Tests.Security
{
    [TestFixture]
    public class ConnectionThrottleTests
    {
        private ConnectionThrottle throttle;

        [SetUp]
        public void Setup()
        {
            throttle = new ConnectionThrottle(maxConcurrent: 3);
        }

        [Test]
        public async Task ExecuteAsync_WithinLimit_Executes()
        {
            // Arrange
            bool executed = false;

            // Act
            await throttle.ExecuteAsync(async () =>
            {
                executed = true;
                await Task.Delay(10);
            });

            // Assert
            Assert.IsTrue(executed);
        }

        [Test]
        public async Task ExecuteAsync_ExceedLimit_Waits()
        {
            // Arrange
            int executionCount = 0;
            var tasks = new Task[5];

            // Act
            for (int i = 0; i < 5; i++)
            {
                tasks[i] = throttle.ExecuteAsync(async () =>
                {
                    System.Threading.Interlocked.Increment(ref executionCount);
                    await Task.Delay(100);
                });
            }

            await Task.WhenAll(tasks);

            // Assert
            Assert.AreEqual(5, executionCount);
        }

        [Test]
        public void Execute_SyncVersion_Works()
        {
            // Arrange
            bool executed = false;

            // Act
            throttle.Execute(() =>
            {
                executed = true;
            });

            // Assert
            Assert.IsTrue(executed);
        }

        [Test]
        public async Task ExecuteAsync_WithTimeout_TimesOut()
        {
            // Arrange
            var slowThrottle = new ConnectionThrottle(maxConcurrent: 1);
            
            // Fill the throttle
            var longTask = slowThrottle.ExecuteAsync(async () =>
            {
                await Task.Delay(5000);
            });

            // Act & Assert
            Assert.ThrowsAsync<TimeoutException>(async () =>
            {
                await slowThrottle.ExecuteAsync(async () =>
                {
                    await Task.Delay(100);
                }, timeout: TimeSpan.FromMilliseconds(500));
            });
        }

        [Test]
        public void GetCurrentCount_ReturnsCorrectValue()
        {
            // Arrange
            int initialCount = throttle.GetCurrentCount();

            // Act
            var task = throttle.ExecuteAsync(async () =>
            {
                await Task.Delay(100);
            });

            int duringCount = throttle.GetCurrentCount();
            task.Wait();
            int afterCount = throttle.GetCurrentCount();

            // Assert
            Assert.AreEqual(0, initialCount);
            Assert.LessOrEqual(afterCount, duringCount);
        }

        [Test]
        public async Task MultipleExecutions_RespectLimit()
        {
            // Arrange
            var throttleWithLimit = new ConnectionThrottle(maxConcurrent: 2);
            int maxConcurrent = 0;
            int currentConcurrent = 0;
            object lockObj = new object();

            // Act
            var tasks = new Task[10];
            for (int i = 0; i < 10; i++)
            {
                tasks[i] = throttleWithLimit.ExecuteAsync(async () =>
                {
                    lock (lockObj)
                    {
                        currentConcurrent++;
                        if (currentConcurrent > maxConcurrent)
                        {
                            maxConcurrent = currentConcurrent;
                        }
                    }

                    await Task.Delay(50);

                    lock (lockObj)
                    {
                        currentConcurrent--;
                    }
                });
            }

            await Task.WhenAll(tasks);

            // Assert
            Assert.LessOrEqual(maxConcurrent, 2);
        }
    }
}
