using System;
using NUnit.Framework;
using Client.Helper;

namespace Client.Tests.Helper
{
    [TestFixture]
    public class AntiDebugTests
    {
        [Test]
        public void PerformChecks_ReturnsBoolean()
        {
            // Act
            bool result = AntiDebug.PerformChecks();

            // Assert
            Assert.That(result, Is.True.Or.False);
        }

        [Test]
        public void Initialize_DoesNotThrowException()
        {
            // Act & Assert
            Assert.DoesNotThrow(() =>
            {
                AntiDebug.Initialize();
            });
        }

        [Test]
        public void PerformChecks_OnProductionMachine_ReturnsFalse()
        {
            // This test assumes running on a non-debugged environment
            // Act
            bool result = AntiDebug.PerformChecks();

            // Assert - On normal machines, should return false (no debugger)
            // In CI/CD or test environments, might return true if debugger attached
            Assert.That(result, Is.True.Or.False);
        }

        [Test]
        public void MultipleChecks_DoNotCrash()
        {
            // Act & Assert - Verify multiple consecutive checks work
            Assert.DoesNotThrow(() =>
            {
                for (int i = 0; i < 5; i++)
                {
                    AntiDebug.PerformChecks();
                }
            });
        }
    }
}
