using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using Server.Helper;

namespace Server.Tests.Helper
{
    [TestFixture]
    public class EncryptionAtRestTests
    {
        private const string TestData = "Sensitive recovery data 123!";
        private string tempFilePath;

        [SetUp]
        public void Setup()
        {
            tempFilePath = Path.GetTempFileName();
        }

        [TearDown]
        public void Cleanup()
        {
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }

        [Test]
        public void Encrypt_WithValidData_ReturnsEncryptedBytes()
        {
            // Arrange
            byte[] plaintext = Encoding.UTF8.GetBytes(TestData);

            // Act
            byte[] encrypted = EncryptionAtRest.Encrypt(plaintext);

            // Assert
            Assert.IsNotNull(encrypted);
            Assert.Greater(encrypted.Length, plaintext.Length);
            CollectionAssert.AreNotEqual(plaintext, encrypted);
        }

        [Test]
        public void Decrypt_WithValidEncryptedData_ReturnsOriginalData()
        {
            // Arrange
            byte[] plaintext = Encoding.UTF8.GetBytes(TestData);
            byte[] encrypted = EncryptionAtRest.Encrypt(plaintext);

            // Act
            byte[] decrypted = EncryptionAtRest.Decrypt(encrypted);

            // Assert
            CollectionAssert.AreEqual(plaintext, decrypted);
            Assert.AreEqual(TestData, Encoding.UTF8.GetString(decrypted));
        }

        [Test]
        public void Encrypt_SameDataTwice_ReturnsDifferentCiphertext()
        {
            // Arrange
            byte[] plaintext = Encoding.UTF8.GetBytes(TestData);

            // Act
            byte[] encrypted1 = EncryptionAtRest.Encrypt(plaintext);
            byte[] encrypted2 = EncryptionAtRest.Encrypt(plaintext);

            // Assert
            CollectionAssert.AreNotEqual(encrypted1, encrypted2);
        }

        [Test]
        public void Decrypt_WithTamperedData_ThrowsException()
        {
            // Arrange
            byte[] plaintext = Encoding.UTF8.GetBytes(TestData);
            byte[] encrypted = EncryptionAtRest.Encrypt(plaintext);
            encrypted[encrypted.Length / 2] ^= 0xFF; // Tamper

            // Act & Assert
            Assert.Throws<System.Security.Cryptography.CryptographicException>(() =>
            {
                EncryptionAtRest.Decrypt(encrypted);
            });
        }

        [Test]
        public void EncryptToFile_CreatesEncryptedFile()
        {
            // Act
            EncryptionAtRest.EncryptToFile(TestData, tempFilePath);

            // Assert
            Assert.IsTrue(File.Exists(tempFilePath));
            Assert.Greater(new FileInfo(tempFilePath).Length, 0);
        }

        [Test]
        public void DecryptFromFile_ReturnsOriginalData()
        {
            // Arrange
            EncryptionAtRest.EncryptToFile(TestData, tempFilePath);

            // Act
            string decrypted = EncryptionAtRest.DecryptFromFile(tempFilePath);

            // Assert
            Assert.AreEqual(TestData, decrypted);
        }

        [Test]
        public void EncryptFile_InPlace_WorksCorrectly()
        {
            // Arrange
            File.WriteAllText(tempFilePath, TestData);

            // Act
            EncryptionAtRest.EncryptFile(tempFilePath);
            string decrypted = EncryptionAtRest.DecryptFromFile(tempFilePath);

            // Assert
            Assert.AreEqual(TestData, decrypted);
        }

        [Test]
        public void DecryptFile_InPlace_WorksCorrectly()
        {
            // Arrange
            EncryptionAtRest.EncryptToFile(TestData, tempFilePath);

            // Act
            EncryptionAtRest.DecryptFile(tempFilePath);
            string content = File.ReadAllText(tempFilePath);

            // Assert
            Assert.AreEqual(TestData, content);
        }

        [Test]
        public void SecureWipe_ClearsArray()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(TestData);
            byte[] original = (byte[])data.Clone();

            // Act
            EncryptionAtRest.SecureWipe(data);

            // Assert
            CollectionAssert.AreNotEqual(original, data);
            Assert.IsTrue(Array.TrueForAll(data, b => b == 0));
        }

        [Test]
        public void EncryptToFile_WithNonExistentDirectory_CreatesDirectory()
        {
            // Arrange
            string dirPath = Path.Combine(Path.GetTempPath(), "XyaTest_" + Guid.NewGuid());
            string filePath = Path.Combine(dirPath, "test.enc");

            try
            {
                // Act
                EncryptionAtRest.EncryptToFile(TestData, filePath);

                // Assert
                Assert.IsTrue(File.Exists(filePath));
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(dirPath))
                {
                    Directory.Delete(dirPath, true);
                }
            }
        }
    }
}
