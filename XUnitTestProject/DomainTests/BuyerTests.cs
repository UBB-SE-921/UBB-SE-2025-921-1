namespace XUnitTestProject.DomainTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using SharedClassLibrary.Domain;
    using Windows.ApplicationModel.VoiceCommands;

    /// <summary>
    /// Contains unit tests for the <see cref="SharedClassLibrary.Domain.Buyer"/> class.
    /// </summary>
    public class BuyerTests
    {

        /// <summary>
        /// Tests the creation of Buyer object
        /// </summary>
        [Fact]
        public static void Buyer_ShouldInitialize_ByDefault()
        {
            Buyer buyer = new Buyer();

            Assert.Equal(BuyerBadge.BRONZE, buyer.Badge);

            Assert.NotNull(buyer.Wishlist);
            Assert.IsType<BuyerWishlist>(buyer.Wishlist);

            Assert.NotNull(buyer.Linkages);
            Assert.IsType<List<BuyerLinkage>>(buyer.Linkages);
            Assert.Empty(buyer.Linkages);

            Assert.Equal(0, buyer.TotalSpending);

            Assert.Equal(0, buyer.NumberOfPurchases);

            Assert.IsType<List<int>>(buyer.FollowingUsersIds);
            Assert.Empty(buyer.FollowingUsersIds);

            Assert.IsType<User>(buyer.User);
            Assert.Equal(string.Empty, buyer.Email);
            Assert.Equal(string.Empty, buyer.PhoneNumber);
            Assert.Equal(0, buyer.Id);

            Assert.Equal(string.Empty, buyer.FirstName);

            Assert.Equal(string.Empty, buyer.LastName);

            Assert.IsType<Address>(buyer.ShippingAddress);

            Assert.IsType<Address>(buyer.BillingAddress);

            Assert.IsType<List<Buyer>>(buyer.SyncedBuyerIds);
            Assert.Empty(buyer.SyncedBuyerIds);
        }

        /// <summary>
        /// Tests that the Buyer's Id is correctly derived from the associated User object's Id.
        /// </summary>
        [Fact]
        public static void Buyer_ShouldSetId_Correctly()
        {
            var user = new User(userID: 1);
            var buyer = new Buyer();
            buyer.User = user;
            Assert.Equal(1, buyer.Id);
        }

        /// <summary>
        /// Tests that the Buyer's PhoneNumber is correctly derived from the associated User object's PhoneNumber.
        /// </summary>
        [Fact]
        public static void Buyer_ShouldSetPhoneNumber_Correctly()
        {
            var user = new User(phoneNumber: "0777777777");
            var buyer = new Buyer();
            buyer.User = user;
            Assert.Equal("0777777777", buyer.PhoneNumber);
        }

        /// <summary>
        /// Tests that setting the Email property on the Buyer also updates the Email on the underlying User object.
        /// </summary>
        [Fact]
        public void Buyer_ShouldSetAndGet_Email_ThroughUser()
        {
            var buyer = new Buyer();
            buyer.Email = "new@buyer.com";
            Assert.Equal("new@buyer.com", buyer.User.Email);
            Assert.Equal("new@buyer.com", buyer.Email);
        }

        /// <summary>
        /// Tests that setting the PhoneNumber property on the Buyer also updates the PhoneNumber on the underlying User object.
        /// </summary>
        [Fact]
        public void Buyer_ShouldSetAndGet_PhoneNumber_ThroughUser()
        {
            var buyer = new Buyer();
            buyer.PhoneNumber = "555-0000";
            Assert.Equal("555-0000", buyer.User.PhoneNumber);
            Assert.Equal("555-0000", buyer.PhoneNumber);
        }

        /// <summary>
        /// Tests that the FirstName and LastName properties can be set and retrieved.
        /// </summary>
        [Fact]
        public void Buyer_ShouldSetAndGet_FirstName_LastName()
        {
            var buyer = new Buyer
            {
                FirstName = "John",
                LastName = "Doe"
            };

            Assert.Equal("John", buyer.FirstName);
            Assert.Equal("Doe", buyer.LastName);
        }

        /// <summary>
        /// Tests that the Badge property can be set and retrieved.
        /// </summary>
        [Fact]
        public void Buyer_ShouldSetAndGet_Badge()
        {
            var buyer = new Buyer { Badge = BuyerBadge.GOLD };
            Assert.Equal(BuyerBadge.GOLD, buyer.Badge);
        }

        /// <summary>
        /// Tests that the Discount property can be set and retrieved.
        /// </summary>
        [Fact]
        public void Buyer_ShouldSetAndGet_Discount()
        {
            var buyer = new Buyer { Discount = 0.15m };
            Assert.Equal(0.15m, buyer.Discount);
        }

        /// <summary>
        /// Tests that the TotalSpending and NumberOfPurchases properties can be set and retrieved.
        /// </summary>
        [Fact]
        public void Buyer_ShouldSetAndGet_TotalSpending_And_NumberOfPurchases()
        {
            var buyer = new Buyer
            {
                TotalSpending = 250.75m,
                NumberOfPurchases = 3
            };

            Assert.Equal(250.75m, buyer.TotalSpending);
            Assert.Equal(3, buyer.NumberOfPurchases);
        }

        /// <summary>
        /// Tests that the ShippingAddress and BillingAddress properties can be set and retrieved.
        /// </summary>
        [Fact]
        public void Buyer_ShouldSetAndGet_Shipping_And_Billing_Address()
        {
            var shipping = new Address { StreetLine = "123 Elm", City = "Nowhere" };
            var billing = new Address { StreetLine = "456 Oak", City = "Somewhere" };

            var buyer = new Buyer
            {
                ShippingAddress = shipping,
                BillingAddress = billing
            };

            Assert.Equal("123 Elm", buyer.ShippingAddress.StreetLine);
            Assert.Equal("456 Oak", buyer.BillingAddress.StreetLine);
        }

        /// <summary>
        /// Tests that the Wishlist property can be set and retrieved.
        /// </summary>
        [Fact]
        public void Buyer_ShouldSetAndGet_Wishlist()
        {
            var wishlist = new BuyerWishlist();
            var buyer = new Buyer { Wishlist = wishlist };

            Assert.Equal(wishlist, buyer.Wishlist);
        }

        /// <summary>
        /// Tests that the Linkages property can be set and retrieved.
        /// </summary>
        [Fact]
        public void Buyer_ShouldSetAndGet_Linkages()
        {
            var linkage1 = new BuyerLinkage { Buyer = new Buyer() };
            var linkage2 = new BuyerLinkage { Buyer = new Buyer() };
            var linkages = new List<BuyerLinkage> { linkage1, linkage2 };
            var buyer = new Buyer { Linkages = linkages };

            Assert.Equal(2, buyer.Linkages.Count);
        }

        /// <summary>
        /// Tests that the FollowingUsersIds property can be set and retrieved.
        /// </summary>
        [Fact]
        public void Buyer_ShouldSetAndGet_FollowingUserIds()
        {
            var buyer = new Buyer
            {
                FollowingUsersIds = new List<int> { 1, 2, 3 }
            };

            Assert.Contains(2, buyer.FollowingUsersIds);
        }

        /// <summary>
        /// Tests that the SyncedBuyerIds property can be set and retrieved.
        /// </summary>
        [Fact]
        public void Buyer_ShouldSetAndGet_SyncedBuyers()
        {
            var synced = new List<Buyer> { new Buyer(), new Buyer() };
            var buyer = new Buyer { SyncedBuyerIds = synced };

            Assert.Equal(2, buyer.SyncedBuyerIds.Count);
        }

        /// <summary>
        /// Tests that the UseSameAddress property can be set and retrieved.
        /// </summary>
        [Fact]
        public void Buyer_ShouldSetAndGet_UseSameAddress()
        {
            var buyer = new Buyer { UseSameAddress = true };
            Assert.True(buyer.UseSameAddress);
        }
    }
}

