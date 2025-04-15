using MarketPlace924.Domain;
using Xunit;

namespace XUnitTestProject.DomainTests
{
    /// <summary>
    /// Contains unit tests for the <see cref="MarketPlace924.Domain.BuyerLinkage"/> class.
    /// </summary>
    public class BuyerLinkageTests
    {
        /// <summary>
        /// Tests that the Buyer property can be set and retrieved correctly.
        /// </summary>
        [Fact]
        public static void BuyerLinkage_ShouldSetAndGet_Buyer()
        {
            var expectedBuyer = new Buyer { FirstName = "Test", LastName = "Buyer" };
            var linkage = new BuyerLinkage
            {
                Buyer = expectedBuyer
            };

            var actualBuyer = linkage.Buyer;

            Assert.Equal(expectedBuyer, actualBuyer);
            Assert.Equal("Test", actualBuyer.FirstName);
        }

        /// <summary>
        /// Tests that the Status property can be set and retrieved correctly.
        /// </summary>
        [Fact]
        public static void BuyerLinkage_ShouldSetAndGet_Status()
        {
            var linkage = new BuyerLinkage
            {
                Buyer = new Buyer() // Required property
            };

            linkage.Status = BuyerLinkageStatus.Possible;
            var expectedStatus = BuyerLinkageStatus.Possible;

            linkage.Status = expectedStatus;
            var actualStatus = linkage.Status;

            Assert.Equal(expectedStatus, actualStatus);
        }
    }
} 