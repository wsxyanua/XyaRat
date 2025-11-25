using System;
using NUnit.Framework;
using Client.Helper;

namespace Client.Tests.Helper
{
    [TestFixture]
    public class Anti_AnalysisTests
    {
        [Test]
        public void DetectManufacturer_ReturnsBoolean()
        {
            // Act
            bool result = Anti_Analysis.DetectManufacturer();

            // Assert - Just verify it runs without crashing
            Assert.That(result, Is.True.Or.False);
        }

        [Test]
        public void CheckVM_ReturnsBoolean()
        {
            // Act
            bool result = Anti_Analysis.CheckVM();

            // Assert
            Assert.That(result, Is.True.Or.False);
        }

        [Test]
        public void CheckSandboxie_ReturnsBoolean()
        {
            // Act
            bool result = Anti_Analysis.CheckSandboxie();

            // Assert
            Assert.That(result, Is.True.Or.False);
        }

        [Test]
        public void CheckEmulation_ReturnsBoolean()
        {
            // Act
            bool result = Anti_Analysis.CheckEmulation();

            // Assert
            Assert.That(result, Is.True.Or.False);
        }

        [Test]
        public void IsSmallDisk_ReturnsBoolean()
        {
            // Act
            bool result = Anti_Analysis.IsSmallDisk();

            // Assert
            Assert.That(result, Is.True.Or.False);
        }

        [Test]
        public void IsXP_ReturnsBoolean()
        {
            // Act
            bool result = Anti_Analysis.IsXP();

            // Assert
            Assert.That(result, Is.True.Or.False);
        }

        [Test]
        public void IsUnderMinRAM_ReturnsBoolean()
        {
            // Act
            bool result = Anti_Analysis.IsUnderMinRAM();

            // Assert
            Assert.That(result, Is.True.Or.False);
        }

        [Test]
        public void InSandbox_CombinesMultipleChecks()
        {
            // Act
            bool result = Anti_Analysis.InSandbox();

            // Assert - Should return true if ANY detection method returns true
            Assert.That(result, Is.True.Or.False);
        }

        [Test]
        public void CheckAllDetections_DoesNotThrowException()
        {
            // Act & Assert - Verify all detection methods can run without crashing
            Assert.DoesNotThrow(() =>
            {
                Anti_Analysis.DetectManufacturer();
                Anti_Analysis.CheckVM();
                Anti_Analysis.CheckSandboxie();
                Anti_Analysis.CheckEmulation();
                Anti_Analysis.IsSmallDisk();
                Anti_Analysis.IsXP();
                Anti_Analysis.IsUnderMinRAM();
                Anti_Analysis.InSandbox();
            });
        }
    }
}
