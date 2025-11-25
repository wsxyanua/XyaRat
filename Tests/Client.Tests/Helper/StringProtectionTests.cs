using System;
using NUnit.Framework;
using Client.Helper;

namespace Client.Tests.Helper
{
    [TestFixture]
    public class StringProtectionTests
    {
        private const string TestString = "SensitiveData123!";
        private const string TestKey = "TestKey";

        [Test]
        public void Encode_WithValidString_ReturnsEncodedString()
        {
            // Act
            string encoded = StringProtection.Encode(TestString, TestKey);

            // Assert
            Assert.IsNotNull(encoded);
            Assert.AreNotEqual(TestString, encoded);
            Assert.Greater(encoded.Length, 0);
        }

        [Test]
        public void Decode_WithValidEncodedString_ReturnsOriginalString()
        {
            // Arrange
            string encoded = StringProtection.Encode(TestString, TestKey);

            // Act
            string decoded = StringProtection.Decode(encoded, TestKey);

            // Assert
            Assert.AreEqual(TestString, decoded);
        }

        [Test]
        public void Encode_SameStringTwice_ReturnsSameResult()
        {
            // Act
            string encoded1 = StringProtection.Encode(TestString, TestKey);
            string encoded2 = StringProtection.Encode(TestString, TestKey);

            // Assert
            Assert.AreEqual(encoded1, encoded2); // XOR with same key should be deterministic
        }

        [Test]
        public void Decode_WithWrongKey_ReturnsGarbage()
        {
            // Arrange
            string encoded = StringProtection.Encode(TestString, TestKey);

            // Act
            string decoded = StringProtection.Decode(encoded, "WrongKey");

            // Assert
            Assert.AreNotEqual(TestString, decoded);
        }

        [Test]
        public void Encode_WithEmptyString_HandlesGracefully()
        {
            // Act
            string encoded = StringProtection.Encode("", TestKey);

            // Assert
            Assert.IsNotNull(encoded);
        }

        [Test]
        public void Decode_WithEmptyString_HandlesGracefully()
        {
            // Act
            string decoded = StringProtection.Decode("", TestKey);

            // Assert
            Assert.IsNotNull(decoded);
        }

        [Test]
        public void Encode_WithSpecialCharacters_WorksCorrectly()
        {
            // Arrange
            string specialString = "Test!@#$%^&*()_+-=[]{}|;':\",./<>?";

            // Act
            string encoded = StringProtection.Encode(specialString, TestKey);
            string decoded = StringProtection.Decode(encoded, TestKey);

            // Assert
            Assert.AreEqual(specialString, decoded);
        }

        [Test]
        public void Encode_WithUnicodeCharacters_WorksCorrectly()
        {
            // Arrange
            string unicodeString = "Tiếng Việt 中文 日本語 العربية";

            // Act
            string encoded = StringProtection.Encode(unicodeString, TestKey);
            string decoded = StringProtection.Decode(encoded, TestKey);

            // Assert
            Assert.AreEqual(unicodeString, decoded);
        }
    }
}
