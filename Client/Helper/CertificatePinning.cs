using System;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace Client.Helper
{
    /// <summary>
    /// Certificate pinning and validation for client-side secure communication
    /// Prevents Man-in-the-Middle (MITM) attacks
    /// </summary>
    public static class CertificatePinning
    {
        // Expected server certificate thumbprint (SHA-1 or SHA-256 hash)
        // This should match the server's certificate thumbprint
        // Update this value with your actual server certificate thumbprint
        private static readonly string[] PinnedThumbprints = new string[]
        {
            // Add your server certificate thumbprint here
            // Example: "A1B2C3D4E5F6789012345678901234567890ABCD"
            // You can get this from: Server certificate properties → Details → Thumbprint
        };

        // Alternative: Pin by public key hash (more flexible - survives certificate renewal)
        private static readonly string[] PinnedPublicKeyHashes = new string[]
        {
            // SHA-256 hash of the server's public key
            // This is better than thumbprint pinning as it survives certificate renewal
        };

        /// <summary>
        /// Validates server certificate with pinning
        /// </summary>
        public static bool ValidateServerCertificate(
            object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            // Always reject if certificate is null
            if (certificate == null)
            {
                Logger.Log("[CertPinning] Certificate is null", Logger.LogLevel.Error);
                return false;
            }

            X509Certificate2 cert2 = new X509Certificate2(certificate);

            #if DEBUG
            // In debug mode, log certificate details and be more lenient
            LogCertificateDetails(cert2, sslPolicyErrors);
            
            // Accept self-signed certificates in debug mode
            if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors ||
                sslPolicyErrors == SslPolicyErrors.RemoteCertificateNameMismatch)
            {
                Logger.Log("[CertPinning] DEBUG: Accepting certificate despite errors", Logger.LogLevel.Warning);
                return true;
            }
            #endif

            // If no pinning configured, use standard validation
            if (PinnedThumbprints.Length == 0 && PinnedPublicKeyHashes.Length == 0)
            {
                #if DEBUG
                Logger.Log("[CertPinning] No pinning configured - using standard validation", Logger.LogLevel.Warning);
                return sslPolicyErrors == SslPolicyErrors.None;
                #else
                // In production without pinning, require valid certificate
                if (sslPolicyErrors != SslPolicyErrors.None)
                {
                    Logger.Log($"[CertPinning] Certificate validation failed: {sslPolicyErrors}", Logger.LogLevel.Error);
                    return false;
                }
                return true;
                #endif
            }

            // Check certificate expiration
            DateTime now = DateTime.Now;
            if (now < cert2.NotBefore || now > cert2.NotAfter)
            {
                Logger.Log($"[CertPinning] Certificate expired or not yet valid. Valid: {cert2.NotBefore} to {cert2.NotAfter}", 
                    Logger.LogLevel.Error);
                return false;
            }

            // Verify thumbprint pinning
            if (PinnedThumbprints.Length > 0)
            {
                string thumbprint = cert2.Thumbprint;
                
                foreach (string pinnedThumbprint in PinnedThumbprints)
                {
                    if (string.Equals(thumbprint, pinnedThumbprint, StringComparison.OrdinalIgnoreCase))
                    {
                        Logger.Log("[CertPinning] ✓ Certificate thumbprint matched", Logger.LogLevel.Info);
                        return true;
                    }
                }

                Logger.Log($"[CertPinning] ✗ Certificate thumbprint mismatch! Got: {thumbprint}", Logger.LogLevel.Error);
                Logger.Log("[CertPinning] POTENTIAL MITM ATTACK DETECTED!", Logger.LogLevel.Error);
                return false;
            }

            // Verify public key pinning (more robust)
            if (PinnedPublicKeyHashes.Length > 0)
            {
                string publicKeyHash = GetPublicKeyHash(cert2);
                
                foreach (string pinnedHash in PinnedPublicKeyHashes)
                {
                    if (string.Equals(publicKeyHash, pinnedHash, StringComparison.OrdinalIgnoreCase))
                    {
                        Logger.Log("[CertPinning] ✓ Public key hash matched", Logger.LogLevel.Info);
                        return true;
                    }
                }

                Logger.Log($"[CertPinning] ✗ Public key hash mismatch! Got: {publicKeyHash}", Logger.LogLevel.Error);
                Logger.Log("[CertPinning] POTENTIAL MITM ATTACK DETECTED!", Logger.LogLevel.Error);
                return false;
            }

            return false;
        }

        /// <summary>
        /// Computes SHA-256 hash of certificate's public key
        /// </summary>
        private static string GetPublicKeyHash(X509Certificate2 certificate)
        {
            try
            {
                byte[] publicKey = certificate.GetPublicKey();
                
                using (var sha256 = System.Security.Cryptography.SHA256.Create())
                {
                    byte[] hash = sha256.ComputeHash(publicKey);
                    return BitConverter.ToString(hash).Replace("-", "");
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"[CertPinning] Error computing public key hash: {ex.Message}", Logger.LogLevel.Error);
                return null;
            }
        }

        /// <summary>
        /// Logs certificate details for debugging
        /// </summary>
        private static void LogCertificateDetails(X509Certificate2 certificate, SslPolicyErrors errors)
        {
            try
            {
                Logger.Log($"[CertPinning] Certificate Details:", Logger.LogLevel.Info);
                Logger.Log($"  Subject: {certificate.Subject}", Logger.LogLevel.Info);
                Logger.Log($"  Issuer: {certificate.Issuer}", Logger.LogLevel.Info);
                Logger.Log($"  Thumbprint: {certificate.Thumbprint}", Logger.LogLevel.Info);
                Logger.Log($"  Valid From: {certificate.NotBefore}", Logger.LogLevel.Info);
                Logger.Log($"  Valid To: {certificate.NotAfter}", Logger.LogLevel.Info);
                Logger.Log($"  Public Key Hash: {GetPublicKeyHash(certificate)}", Logger.LogLevel.Info);
                Logger.Log($"  SSL Errors: {errors}", Logger.LogLevel.Info);
            }
            catch { }
        }

        /// <summary>
        /// Checks if certificate pinning is configured
        /// </summary>
        public static bool IsPinningConfigured()
        {
            return PinnedThumbprints.Length > 0 || PinnedPublicKeyHashes.Length > 0;
        }

        /// <summary>
        /// Gets the expected thumbprints for configuration
        /// </summary>
        public static string[] GetPinnedThumbprints()
        {
            return PinnedThumbprints;
        }

        /// <summary>
        /// Anti-SSL-Strip protection: Ensures connection is actually encrypted
        /// </summary>
        public static bool VerifyEncryption(SslStream sslStream)
        {
            if (sslStream == null)
                return false;

            try
            {
                if (!sslStream.IsEncrypted)
                {
                    Logger.Log("[CertPinning] Connection is not encrypted!", Logger.LogLevel.Error);
                    return false;
                }

                if (!sslStream.IsSigned)
                {
                    Logger.Log("[CertPinning] Connection is not signed!", Logger.LogLevel.Error);
                    return false;
                }

                // Check for strong encryption
                if (sslStream.CipherAlgorithm == CipherAlgorithmType.Null ||
                    sslStream.CipherAlgorithm == CipherAlgorithmType.None)
                {
                    Logger.Log("[CertPinning] Weak cipher algorithm detected!", Logger.LogLevel.Error);
                    return false;
                }

                // Require at least 128-bit encryption
                if (sslStream.CipherStrength < 128)
                {
                    Logger.Log($"[CertPinning] Weak cipher strength: {sslStream.CipherStrength} bits", Logger.LogLevel.Error);
                    return false;
                }

                Logger.Log($"[CertPinning] ✓ Encryption verified: {sslStream.CipherAlgorithm} ({sslStream.CipherStrength} bits)", 
                    Logger.LogLevel.Info);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log($"[CertPinning] Error verifying encryption: {ex.Message}", Logger.LogLevel.Error);
                return false;
            }
        }
    }
}
