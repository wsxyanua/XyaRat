using System;
using System.IO;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Server.Helper
{
    /// <summary>
    /// Certificate management and validation for secure TLS communication
    /// Implements certificate pinning and mutual TLS authentication
    /// </summary>
    public static class CertificateManager
    {
        // Pinned certificate thumbprints (SHA-256 hashes of expected certificates)
        // In production, these should be configured via settings
        private static readonly string[] PinnedCertificateThumbprints = new string[]
        {
            // Add your server certificate thumbprint here
            // Example: "A1B2C3D4E5F6G7H8I9J0K1L2M3N4O5P6Q7R8S9T0"
        };

        // Path to server certificate (PFX/P12 file with private key)
        private static readonly string ServerCertificatePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, 
            "Certificates", 
            "server.pfx"
        );

        // Certificate password loaded from secure storage
        private static string GetCertificatePassword()
        {
            return SecureConfig.GetCertificatePassword();
        }

        private static X509Certificate2 _serverCertificate;

        /// <summary>
        /// Loads the server certificate for TLS
        /// </summary>
        public static X509Certificate2 GetServerCertificate()
        {
            if (_serverCertificate != null)
                return _serverCertificate;

            try
            {
                if (File.Exists(ServerCertificatePath))
                {
                    var password = GetCertificatePassword();
                    _serverCertificate = new X509Certificate2(
                        ServerCertificatePath,
                        password,
                        X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet
                    );

                    Logger.Log($"[Certificate] Loaded server certificate: {_serverCertificate.Subject}", Logger.LogLevel.Info);
                    return _serverCertificate;
                }
                else
                {
                    // Generate self-signed certificate if none exists
                    Logger.Log("[Certificate] No certificate found, generating self-signed certificate", Logger.LogLevel.Warning);
                    _serverCertificate = GenerateSelfSignedCertificate();
                    var password = GetCertificatePassword();
                    SaveCertificate(_serverCertificate, ServerCertificatePath, password);
                    return _serverCertificate;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"[Certificate] Error loading certificate: {ex.Message}", Logger.LogLevel.Error);
                return null;
            }
        }

        /// <summary>
        /// Generates a self-signed certificate for development/testing
        /// </summary>
        private static X509Certificate2 GenerateSelfSignedCertificate()
        {
            try
            {
                string subjectName = $"CN=XyaRat Server, O=XyaRat, C=US";
                
                using (RSA rsa = RSA.Create(2048))
                {
                    var request = new CertificateRequest(
                        subjectName,
                        rsa,
                        HashAlgorithmName.SHA256,
                        RSASignaturePadding.Pkcs1
                    );

                    // Add extensions
                    request.CertificateExtensions.Add(
                        new X509KeyUsageExtension(
                            X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment,
                            false
                        )
                    );

                    request.CertificateExtensions.Add(
                        new X509EnhancedKeyUsageExtension(
                            new OidCollection { new Oid("1.3.6.1.5.5.7.3.1") }, // Server Authentication
                            false
                        )
                    );

                    // Add Subject Alternative Names
                    var sanBuilder = new SubjectAlternativeNameBuilder();
                    sanBuilder.AddDnsName("localhost");
                    sanBuilder.AddDnsName(Environment.MachineName);
                    sanBuilder.AddIpAddress(System.Net.IPAddress.Loopback);
                    request.CertificateExtensions.Add(sanBuilder.Build());

                    // Create self-signed certificate
                    var certificate = request.CreateSelfSigned(
                        DateTimeOffset.Now.AddDays(-1),
                        DateTimeOffset.Now.AddYears(5)
                    );
                    // Export with private key
                    var password = GetCertificatePassword();
                    return new X509Certificate2(
                        certificate.Export(X509ContentType.Pfx, password),
                        password,
                        X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet
                    );  X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet
                    );
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"[Certificate] Error generating self-signed certificate: {ex.Message}", Logger.LogLevel.Error);
                return null;
            }
        }

        /// <summary>
        /// Saves certificate to file
        /// </summary>
        private static void SaveCertificate(X509Certificate2 certificate, string path, string password)
        {
            try
            {
                string directory = Path.GetDirectoryName(path);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                byte[] certBytes = certificate.Export(X509ContentType.Pfx, password);
                File.WriteAllBytes(path, certBytes);

                Logger.Log($"[Certificate] Saved certificate to: {path}", Logger.LogLevel.Info);
            }
            catch (Exception ex)
            {
                Logger.Log($"[Certificate] Error saving certificate: {ex.Message}", Logger.LogLevel.Error);
            }
        }

        /// <summary>
        /// Validates client certificate for mutual TLS
        /// </summary>
        public static bool ValidateClientCertificate(
            object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            // For development: accept all certificates
            #if DEBUG
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            Logger.Log($"[Certificate] Client certificate validation (DEBUG): {sslPolicyErrors}", Logger.LogLevel.Warning);
            return true; // Accept all in debug mode
            #endif

            // Production validation
            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                // Certificate is valid, now check if it's pinned
                return IsCertificatePinned(certificate);
            }

            Logger.Log($"[Certificate] Client certificate validation failed: {sslPolicyErrors}", Logger.LogLevel.Error);
            return false;
        }

        /// <summary>
        /// Validates server certificate on client side (certificate pinning)
        /// </summary>
        public static bool ValidateServerCertificate(
            object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            // Check if certificate is pinned
            if (PinnedCertificateThumbprints.Length == 0)
            {
                // No pinning configured, use default validation
                if (sslPolicyErrors == SslPolicyErrors.None)
                    return true;

                #if DEBUG
                // In debug mode, accept self-signed certificates
                Logger.Log($"[Certificate] Server certificate validation (DEBUG): {sslPolicyErrors}", Logger.LogLevel.Warning);
                return true;
                #endif

                Logger.Log($"[Certificate] Server certificate validation failed: {sslPolicyErrors}", Logger.LogLevel.Error);
                return false;
            }

            // Certificate pinning: check if thumbprint matches
            bool isPinned = IsCertificatePinned(certificate);
            
            if (!isPinned)
            {
                Logger.Log("[Certificate] Server certificate not pinned - potential MITM attack!", Logger.LogLevel.Error);
            }

            return isPinned;
        }

        /// <summary>
        /// Checks if certificate thumbprint is in pinned list
        /// </summary>
        private static bool IsCertificatePinned(X509Certificate certificate)
        {
            if (certificate == null)
                return false;

            if (PinnedCertificateThumbprints.Length == 0)
                return true; // No pinning configured

            try
            {
                X509Certificate2 cert2 = new X509Certificate2(certificate);
                string thumbprint = cert2.Thumbprint;

                foreach (string pinnedThumbprint in PinnedCertificateThumbprints)
                {
                    if (string.Equals(thumbprint, pinnedThumbprint, StringComparison.OrdinalIgnoreCase))
                    {
                        Logger.Log($"[Certificate] Certificate pinned successfully: {thumbprint}", Logger.LogLevel.Info);
                        return true;
                    }
                }

                Logger.Log($"[Certificate] Certificate not in pinned list: {thumbprint}", Logger.LogLevel.Warning);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Log($"[Certificate] Error checking certificate pinning: {ex.Message}", Logger.LogLevel.Error);
                return false;
            }
        }

        /// <summary>
        /// Gets the thumbprint of a certificate for pinning
        /// </summary>
        public static string GetCertificateThumbprint(X509Certificate2 certificate)
        {
            if (certificate == null)
                return null;

            return certificate.Thumbprint;
        }

        /// <summary>
        /// Exports certificate public key for client distribution
        /// </summary>
        public static byte[] ExportPublicKey(X509Certificate2 certificate)
        {
            if (certificate == null)
                return null;

            try
            {
                return certificate.Export(X509ContentType.Cert);
            }
            catch (Exception ex)
            {
                Logger.Log($"[Certificate] Error exporting public key: {ex.Message}", Logger.LogLevel.Error);
                return null;
            }
        }

        /// <summary>
        /// Verifies certificate expiration
        /// </summary>
        public static bool IsCertificateValid(X509Certificate2 certificate)
        {
            if (certificate == null)
                return false;

            DateTime now = DateTime.Now;
            bool isValid = now >= certificate.NotBefore && now <= certificate.NotAfter;

            if (!isValid)
            {
                Logger.Log($"[Certificate] Certificate expired or not yet valid. Valid from {certificate.NotBefore} to {certificate.NotAfter}", 
                    Logger.LogLevel.Warning);
            }

            return isValid;
        }

        /// <summary>
        /// Logs certificate information
        /// </summary>
        public static void LogCertificateInfo(X509Certificate2 certificate)
        {
            if (certificate == null)
                return;

            StringBuilder info = new StringBuilder();
            info.AppendLine("[Certificate Information]");
            info.AppendLine($"Subject: {certificate.Subject}");
            info.AppendLine($"Issuer: {certificate.Issuer}");
            info.AppendLine($"Thumbprint: {certificate.Thumbprint}");
            info.AppendLine($"Serial Number: {certificate.SerialNumber}");
            info.AppendLine($"Valid From: {certificate.NotBefore}");
            info.AppendLine($"Valid To: {certificate.NotAfter}");
            info.AppendLine($"Has Private Key: {certificate.HasPrivateKey}");

            Logger.Log(info.ToString(), Logger.LogLevel.Info);
        }
    }
}
