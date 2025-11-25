using System;
using System.Collections.Generic;
using NUnit.Framework;
using Client.Connection;

namespace Client.Tests.Connection
{
    [TestFixture]
    public class DomainGeneratorTests
    {
        [Test]
        public void GenerateDomain_WithValidDate_ReturnsValidDomain()
        {
            // Arrange
            DateTime testDate = new DateTime(2025, 11, 25);

            // Act
            string domain = DomainGenerator.GenerateDomain(testDate);

            // Assert
            Assert.IsNotNull(domain);
            Assert.IsNotEmpty(domain);
            Assert.IsTrue(domain.Contains(".")); // Should have TLD
            Assert.Greater(domain.Length, 5); // Minimum domain length
        }

        [Test]
        public void GenerateDomain_SameDate_ReturnsSameDomain()
        {
            // Arrange
            DateTime testDate = new DateTime(2025, 11, 25);

            // Act
            string domain1 = DomainGenerator.GenerateDomain(testDate);
            string domain2 = DomainGenerator.GenerateDomain(testDate);

            // Assert
            Assert.AreEqual(domain1, domain2); // Should be deterministic
        }

        [Test]
        public void GenerateDomain_DifferentDates_ReturnsDifferentDomains()
        {
            // Arrange
            DateTime date1 = new DateTime(2025, 11, 25);
            DateTime date2 = new DateTime(2025, 11, 26);

            // Act
            string domain1 = DomainGenerator.GenerateDomain(date1);
            string domain2 = DomainGenerator.GenerateDomain(date2);

            // Assert
            Assert.AreNotEqual(domain1, domain2);
        }

        [Test]
        public void GetFallbackDomains_ReturnsNonEmptyList()
        {
            // Act
            List<string> domains = DomainGenerator.GetFallbackDomains();

            // Assert
            Assert.IsNotNull(domains);
            Assert.Greater(domains.Count, 0);
            Assert.LessOrEqual(domains.Count, 11); // Today + past 5 + future 5
        }

        [Test]
        public void GetFallbackDomains_ContainsTodaysDomain()
        {
            // Act
            List<string> domains = DomainGenerator.GetFallbackDomains();
            string todayDomain = DomainGenerator.GenerateDomain(DateTime.Now);

            // Assert
            Assert.Contains(todayDomain, domains);
        }

        [Test]
        public void GetFallbackDomains_AllDomainsAreValid()
        {
            // Act
            List<string> domains = DomainGenerator.GetFallbackDomains();

            // Assert
            foreach (string domain in domains)
            {
                Assert.IsNotNull(domain);
                Assert.IsNotEmpty(domain);
                Assert.IsTrue(domain.Contains(".")); // Has TLD
                Assert.Greater(domain.Length, 5);
            }
        }

        [Test]
        public void GenerateDomain_HasValidTLD()
        {
            // Arrange
            string[] validTLDs = { ".com", ".net", ".org", ".info", ".biz" };
            DateTime testDate = DateTime.Now;

            // Act
            string domain = DomainGenerator.GenerateDomain(testDate);

            // Assert
            bool hasValidTLD = false;
            foreach (string tld in validTLDs)
            {
                if (domain.EndsWith(tld))
                {
                    hasValidTLD = true;
                    break;
                }
            }
            Assert.IsTrue(hasValidTLD, $"Domain '{domain}' does not have a valid TLD");
        }

        [Test]
        public void GetFallbackDomains_NoDuplicates()
        {
            // Act
            List<string> domains = DomainGenerator.GetFallbackDomains();
            HashSet<string> uniqueDomains = new HashSet<string>(domains);

            // Assert
            Assert.AreEqual(domains.Count, uniqueDomains.Count, "Fallback domains contain duplicates");
        }
    }
}
