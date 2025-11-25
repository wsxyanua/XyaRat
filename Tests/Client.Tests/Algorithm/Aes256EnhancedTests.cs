using System;
using System.Text;
using NUnit.Framework;
using Client.Algorithm;

namespace Client.Tests.Algorithm
{
    [TestFixture]
    public class Aes256EnhancedTests
    {
        private const string TestPassword = "TestPassword123!";
        private const string TestData = "This is sensitive test data.";

        [Test]
        public void Encrypt_WithValidData_ReturnsEncryptedBytes()
        {
            // Arrange
            byte[] plaintext = Encoding.UTF8.GetBytes(TestData);

            // Act
            byte[] encrypted = Aes256Enhanced.Encrypt(plaintext, TestPassword);

            // Assert
            Assert.IsNotNull(encrypted);
            Assert.Greater(encrypted.Length, plaintext.Length); // Should be larger due to IV, salt, HMAC
            CollectionAssert.AreNotEqual(plaintext, encrypted);
        }

        [Test]
        public void Decrypt_WithValidEncryptedData_ReturnsOriginalData()
        {
            // Arrange
            byte[] plaintext = Encoding.UTF8.GetBytes(TestData);
            byte[] encrypted = Aes256Enhanced.Encrypt(plaintext, TestPassword);

            // Act
            byte[] decrypted = Aes256Enhanced.Decrypt(encrypted, TestPassword);

            // Assert
            Assert.IsNotNull(decrypted);
            CollectionAssert.AreEqual(plaintext, decrypted);
            Assert.AreEqual(TestData, Encoding.UTF8.GetString(decrypted));
        }

        [Test]
        public void Encrypt_SameDataTwice_ReturnsDifferentCiphertext()
        {
            // Arrange
            byte[] plaintext = Encoding.UTF8.GetBytes(TestData);

            // Act
            byte[] encrypted1 = Aes256Enhanced.Encrypt(plaintext, TestPassword);
            byte[] encrypted2 = Aes256Enhanced.Encrypt(plaintext, TestPassword);

            // Assert
            CollectionAssert.AreNotEqual(encrypted1, encrypted2); // Should differ due to random IV/salt
        }

        [Test]
        public void Decrypt_WithWrongPassword_ThrowsException()
        {
            // Arrange
            byte[] plaintext = Encoding.UTF8.GetBytes(TestData);
            byte[] encrypted = Aes256Enhanced.Encrypt(plaintext, TestPassword);

            // Act & Assert
            Assert.Throws<System.Security.Cryptography.CryptographicException>(() =>
            {
                Aes256Enhanced.Decrypt(encrypted, "WrongPassword");
            });
        }

        [Test]
        public void Decrypt_WithTamperedData_ThrowsException()
        {
            // Arrange
            byte[] plaintext = Encoding.UTF8.GetBytes(TestData);
            byte[] encrypted = Aes256Enhanced.Encrypt(plaintext, TestPassword);
            
            // Tamper with encrypted data
            encrypted[encrypted.Length / 2] ^= 0xFF;

            // Act & Assert
            Assert.Throws<System.Security.Cryptography.CryptographicException>(() =>
            {
                Aes256Enhanced.Decrypt(encrypted, TestPassword);
            });
        }

        [Test]
        public void Encrypt_WithEmptyData_HandlesGracefully()
        {
            // Arrange
            byte[] plaintext = new byte[0];

            // Act
            byte[] encrypted = Aes256Enhanced.Encrypt(plaintext, TestPassword);

            // Assert
            Assert.IsNotNull(encrypted);
            Assert.Greater(encrypted.Length, 0); // Should still have IV, salt, HMAC
        }

        [Test]
        public void Decrypt_WithNullData_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
            {
                Aes256Enhanced.Decrypt(null, TestPassword);
            });
        }

        [Test]
        public void Encrypt_WithLargeData_WorksCorrectly()
        {
            // Arrange
            byte[] plaintext = new byte[1024 * 100]; // 100 KB
            new Random().NextBytes(plaintext);

            // Act
            byte[] encrypted = Aes256Enhanced.Encrypt(plaintext, TestPassword);
            byte[] decrypted = Aes256Enhanced.Decrypt(encrypted, TestPassword);

            // Assert
            CollectionAssert.AreEqual(plaintext, decrypted);
        }

        [Test]
        public void ConstantTimeComparison_WithEqualArrays_ReturnsTrue()
        {
            // Arrange
            byte[] array1 = Encoding.UTF8.GetBytes("TestData");
            byte[] array2 = Encoding.UTF8.GetBytes("TestData");

            // Act
            bool result = Aes256Enhanced.ConstantTimeComparison(array1, array2);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void ConstantTimeComparison_WithDifferentArrays_ReturnsFalse()
        {
            // Arrange
            byte[] array1 = Encoding.UTF8.GetBytes("TestData1");
            byte[] array2 = Encoding.UTF8.GetBytes("TestData2");

            // Act
            bool result = Aes256Enhanced.ConstantTimeComparison(array1, array2);

            // Assert
            Assert.IsFalse(result);
        }
    }
}
