using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using NUnit.Framework;
using Server.Helper;

namespace Server.Tests.Helper
{
    [TestFixture]
    public class CertificateManagerTests
    {
        private string tempCertPath;

        [SetUp]
        public void Setup()
        {
            tempCertPath = Path.Combine(Path.GetTempPath(), "test_cert.pfx");
        }

        [TearDown]
        public void Cleanup()
        {
            if (File.Exists(tempCertPath))
            {
                File.Delete(tempCertPath);
            }
        }

        [Test]
        public void GenerateSelfSignedCertificate_CreatesValidCertificate()
        {
            // Arrange
            string subject = "CN=TestServer";
            string password = "TestPassword123";

            // Act
            var cert = CertificateManager.GenerateSelfSignedCertificate(subject, password, tempCertPath);

            // Assert
            Assert.IsNotNull(cert);
            Assert.IsTrue(cert.HasPrivateKey);
            Assert.IsTrue(File.Exists(tempCertPath));
        }

        [Test]
        public void LoadCertificate_WithValidFile_ReturnsCertificate()
        {
            // Arrange
            string subject = "CN=TestServer";
            string password = "TestPassword123";
            CertificateManager.GenerateSelfSignedCertificate(subject, password, tempCertPath);

            // Act
            var cert = CertificateManager.LoadCertificate(tempCertPath, password);

            // Assert
            Assert.IsNotNull(cert);
            Assert.IsTrue(cert.HasPrivateKey);
        }

        [Test]
        public void LoadCertificate_WithWrongPassword_ThrowsException()
        {
            // Arrange
            string subject = "CN=TestServer";
            string password = "TestPassword123";
            CertificateManager.GenerateSelfSignedCertificate(subject, password, tempCertPath);

            // Act & Assert
            Assert.Throws<System.Security.Cryptography.CryptographicException>(() =>
            {
                CertificateManager.LoadCertificate(tempCertPath, "WrongPassword");
            });
        }

        [Test]
        public void LoadCertificate_WithNonExistentFile_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<FileNotFoundException>(() =>
            {
                CertificateManager.LoadCertificate("nonexistent.pfx", "password");
            });
        }

        [Test]
        public void ValidateCertificate_WithValidCert_ReturnsTrue()
        {
            // Arrange
            string subject = "CN=TestServer";
            string password = "TestPassword123";
            var cert = CertificateManager.GenerateSelfSignedCertificate(subject, password, tempCertPath);

            // Act
            bool isValid = CertificateManager.ValidateCertificate(cert);

            // Assert
            Assert.IsTrue(isValid);
        }

        [Test]
        public void GetCertificateThumbprint_ReturnsNonEmpty()
        {
            // Arrange
            string subject = "CN=TestServer";
            string password = "TestPassword123";
            var cert = CertificateManager.GenerateSelfSignedCertificate(subject, password, tempCertPath);

            // Act
            string thumbprint = CertificateManager.GetCertificateThumbprint(cert);

            // Assert
            Assert.IsNotNull(thumbprint);
            Assert.IsNotEmpty(thumbprint);
            Assert.Greater(thumbprint.Length, 0);
        }

        [Test]
        public void GetPublicKey_ReturnsValidKey()
        {
            // Arrange
            string subject = "CN=TestServer";
            string password = "TestPassword123";
            var cert = CertificateManager.GenerateSelfSignedCertificate(subject, password, tempCertPath);

            // Act
            byte[] publicKey = CertificateManager.GetPublicKey(cert);

            // Assert
            Assert.IsNotNull(publicKey);
            Assert.Greater(publicKey.Length, 0);
        }

        [Test]
        public void GenerateSelfSignedCertificate_WithCustomValidity_CreatesCorrectCert()
        {
            // Arrange
            string subject = "CN=TestServer";
            string password = "TestPassword123";
            int yearsValid = 3;

            // Act
            var cert = CertificateManager.GenerateSelfSignedCertificate(subject, password, tempCertPath, yearsValid);

            // Assert
            Assert.IsNotNull(cert);
            Assert.LessOrEqual(cert.NotBefore, DateTime.Now);
            Assert.GreaterOrEqual(cert.NotAfter, DateTime.Now.AddYears(yearsValid - 1));
        }
    }
}
