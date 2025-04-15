using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarketPlace924.Domain;
using MarketPlace924.Repository;
using MarketPlace924.Service;
using Moq;
using Xunit;

namespace XUnitTestProject.ServiceTests
{
    public class BuyerServiceTests
    {
        private readonly Mock<IBuyerRepository> mockBuyerRepository;
        private readonly Mock<IUserRepository> mockUserRepository;
        private readonly BuyerService buyerService;
        private readonly User testUser;
        private readonly Buyer testBuyer;
        private readonly Address testAddress;

        public BuyerServiceTests()
        {
            mockBuyerRepository = new Mock<IBuyerRepository>();
            mockUserRepository = new Mock<IUserRepository>();
            buyerService = new BuyerService(mockBuyerRepository.Object, mockUserRepository.Object);

            // Setup test data
            testUser = new User
            {
                UserId = 1,
                Username = "testuser",
                Email = "test@example.com",
                PhoneNumber = "1234567890"
            };
            testBuyer = new Buyer
            {
                User = testUser,
                FirstName = "Test",
                LastName = "User",
                PhoneNumber = "1234567890",
                Badge = BuyerBadge.BRONZE,
                TotalSpending = 0,
                NumberOfPurchases = 0,
                Discount = 0,
                UseSameAddress = true
            };
            testAddress = new Address
            {
                Id = 1,
                Country = "Test Country",
                City = "Test City",
                StreetLine = "Test Street",
                PostalCode = "12345"
            };
            testBuyer.BillingAddress = testAddress;
            testBuyer.ShippingAddress = testAddress;
        }

        [Fact]
        public async Task GetBuyerByUser_ShouldLoadAllSegments()
        {
            // Arrange
            mockBuyerRepository.Setup(repo => repo.LoadBuyerInfo(It.IsAny<Buyer>()))
                .Returns(Task.CompletedTask);
            mockBuyerRepository.Setup(repo => repo.GetWishlist(It.IsAny<int>()))
                .ReturnsAsync(new BuyerWishlist());
            mockBuyerRepository.Setup(repo => repo.GetBuyerLinkages(It.IsAny<int>()))
                .ReturnsAsync(new List<BuyerLinkage>());

            // Act
            var result = await buyerService.GetBuyerByUser(testUser);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(testUser.UserId, result.User.UserId);
            mockBuyerRepository.Verify(repo => repo.LoadBuyerInfo(It.IsAny<Buyer>()), Times.Once);
            mockBuyerRepository.Verify(repo => repo.GetWishlist(It.IsAny<int>()), Times.Once);
            mockBuyerRepository.Verify(repo => repo.GetBuyerLinkages(It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task CreateBuyer_WithValidData_ShouldSucceed()
        {
            // Arrange
            // Create a mock for IUserValidator
            var mockUserValidator = new Mock<IUserValidator>();

            // Set the mock to return true for phone number validation
            mockUserValidator.Setup(validator => validator.IsValidPhoneNumber(It.IsAny<string>()))
                .Returns(true);

            // Use reflection to replace the private userValidator field in the BuyerService
            var userValidatorField = typeof(BuyerService).GetField("userValidator",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            userValidatorField!.SetValue(buyerService, mockUserValidator.Object);

            mockBuyerRepository.Setup(repo => repo.CreateBuyer(It.IsAny<Buyer>()))
                .Returns(Task.CompletedTask);
            mockUserRepository.Setup(repo => repo.UpdateUserPhoneNumber(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            // Act
            await buyerService.CreateBuyer(testBuyer);

            // Assert
            mockBuyerRepository.Verify(repo => repo.CreateBuyer(testBuyer), Times.Once);
            mockUserRepository.Verify(repo => repo.UpdateUserPhoneNumber(testBuyer.User), Times.Once);
        }

        [Fact]
        public async Task CreateBuyer_WithInvalidData_ShouldThrowArgumentException()
        {
            // Arrange
            var invalidBuyer = new Buyer { FirstName = "", LastName = "", PhoneNumber = "invalid" };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => buyerService.CreateBuyer(invalidBuyer));
            mockBuyerRepository.Verify(repo => repo.CreateBuyer(It.IsAny<Buyer>()), Times.Never);
        }

        [Fact]
        public async Task FindBuyersWithShippingAddress_WithValidAddress_ShouldReturnBuyers()
        {
            // Arrange
            var expectedBuyers = new List<Buyer> { testBuyer };
            mockBuyerRepository.Setup(repo => repo.FindBuyersWithShippingAddress(It.IsAny<Address>()))
                .ReturnsAsync(expectedBuyers);
            mockBuyerRepository.Setup(repo => repo.LoadBuyerInfo(It.IsAny<Buyer>()))
                .Returns(Task.CompletedTask);
            mockUserRepository.Setup(repo => repo.LoadUserPhoneNumberAndEmailById(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await buyerService.FindBuyersWithShippingAddress(testAddress);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            mockBuyerRepository.Verify(repo => repo.FindBuyersWithShippingAddress(testAddress), Times.Once);
        }

        [Fact]
        public async Task FindBuyersWithShippingAddress_WithNullCountry_ShouldReturnEmptyList()
        {
            // Arrange
            var addressWithNullCountry = new Address { Country = null };

            // Act
            var result = await buyerService.FindBuyersWithShippingAddress(addressWithNullCountry);

            // Assert
            Assert.Empty(result);
            mockBuyerRepository.Verify(repo => repo.FindBuyersWithShippingAddress(It.IsAny<Address>()), Times.Never);
        }

        [Fact]
        public async Task LoadBuyer_WithBasicInfo_ShouldLoadBasicInfo()
        {
            // Arrange
            mockBuyerRepository.Setup(repo => repo.LoadBuyerInfo(It.IsAny<Buyer>()))
                .Returns(Task.CompletedTask);

            // Act
            await buyerService.LoadBuyer(testBuyer, BuyerDataSegments.BasicInfo);

            // Assert
            mockBuyerRepository.Verify(repo => repo.LoadBuyerInfo(testBuyer), Times.Once);
            mockUserRepository.Verify(repo => repo.LoadUserPhoneNumberAndEmailById(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task LoadBuyer_WithUserInfo_ShouldLoadUserInfo()
        {
            // Arrange
            mockUserRepository.Setup(repo => repo.LoadUserPhoneNumberAndEmailById(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            // Act
            await buyerService.LoadBuyer(testBuyer, BuyerDataSegments.User);

            // Assert
            mockUserRepository.Verify(repo => repo.LoadUserPhoneNumberAndEmailById(testBuyer.User), Times.Once);
            mockBuyerRepository.Verify(repo => repo.LoadBuyerInfo(It.IsAny<Buyer>()), Times.Never);
        }

        [Fact]
        public async Task LoadBuyer_WithWishlist_ShouldLoadWishlist()
        {
            // Arrange
            var expectedWishlist = new BuyerWishlist();
            mockBuyerRepository.Setup(repo => repo.GetWishlist(It.IsAny<int>()))
                .ReturnsAsync(expectedWishlist);

            // Act
            await buyerService.LoadBuyer(testBuyer, BuyerDataSegments.Wishlist);

            // Assert
            Assert.Equal(expectedWishlist, testBuyer.Wishlist);
            mockBuyerRepository.Verify(repo => repo.GetWishlist(testBuyer.Id), Times.Once);
        }

        [Fact]
        public async Task LoadBuyer_WithLinkages_ShouldPopulateLinkedBuyersList()
        {
            // Arrange
            var linkedBuyer = CreateLinkedBuyer();
            var linkages = new List<BuyerLinkage> { new BuyerLinkage { Buyer = linkedBuyer } };

            // Setup mock expectations
            SetupMocksForLinkageTests(linkedBuyer, linkages);

            // Act
            await buyerService.LoadBuyer(testBuyer, BuyerDataSegments.Linkages);

            // Assert
            Assert.Equal(linkages, testBuyer.Linkages);
            Assert.Single(testBuyer.Linkages);
            
            // Verify repository calls
            VerifyRepositoryCallsForLinkageTest(linkedBuyer);
        }
        
        [Fact]
        public async Task LoadBuyer_WithLinkages_ShouldLoadCorrectBuyerData()
        {
            // Arrange
            var linkedBuyer = CreateLinkedBuyer();
            var linkages = new List<BuyerLinkage> { new BuyerLinkage { Buyer = linkedBuyer } };

            // Setup mock expectations
            SetupMocksForLinkageTests(linkedBuyer, linkages);

            // Act
            await buyerService.LoadBuyer(testBuyer, BuyerDataSegments.Linkages);

            // Assert
            Assert.Equal(linkedBuyer.User.UserId, testBuyer.Linkages[0].Buyer.User.UserId);
            Assert.Equal(linkedBuyer.FirstName, testBuyer.Linkages[0].Buyer.FirstName);
            Assert.Equal(linkedBuyer.LastName, testBuyer.Linkages[0].Buyer.LastName);
            Assert.Equal(linkedBuyer.PhoneNumber, testBuyer.Linkages[0].Buyer.PhoneNumber);
            Assert.Equal(linkedBuyer.Badge, testBuyer.Linkages[0].Buyer.Badge);
        }
        
        [Fact]
        public async Task LoadBuyer_WithLinkages_ShouldLoadCorrectAddressData()
        {
            // Arrange
            var linkedBuyer = CreateLinkedBuyer();
            var linkages = new List<BuyerLinkage> { new BuyerLinkage { Buyer = linkedBuyer } };

            // Setup mock expectations
            SetupMocksForLinkageTests(linkedBuyer, linkages);

            // Act
            await buyerService.LoadBuyer(testBuyer, BuyerDataSegments.Linkages);

            // Assert
            Assert.Equal(linkedBuyer.BillingAddress.Country, testBuyer.Linkages[0].Buyer.BillingAddress.Country);
            Assert.Equal(linkedBuyer.ShippingAddress.Country, testBuyer.Linkages[0].Buyer.ShippingAddress.Country);
        }
        
        private Buyer CreateLinkedBuyer()
        {
            return new Buyer
            {
                UserId = 2,
                User = new User(2, "linkeduser", "linked@example.com", "0987654321"),
                FirstName = "Linked",
                LastName = "User",
                PhoneNumber = "0987654321",
                Badge = BuyerBadge.BRONZE,
                TotalSpending = 0,
                NumberOfPurchases = 0,
                Discount = 0,
                UseSameAddress = true,
                BillingAddress = new Address
                {
                    Id = 2,
                    Country = "Linked Country",
                    City = "Linked City",
                    StreetLine = "Linked Street",
                    PostalCode = "54321",
                },
                ShippingAddress = new Address
                {
                    Id = 2,
                    Country = "Linked Country",
                    City = "Linked City",
                    StreetLine = "Linked Street",
                    PostalCode = "54321"
                }
            };
        }
        
        private void SetupMocksForLinkageTests(Buyer linkedBuyer, List<BuyerLinkage> linkages)
        {
            mockBuyerRepository.Setup(repo => repo.GetBuyerLinkages(It.IsAny<int>()))
                .ReturnsAsync(linkages);
            mockBuyerRepository.Setup(repo => repo.LoadBuyerInfo(It.IsAny<Buyer>()))
                .Returns(Task.CompletedTask);
            mockUserRepository.Setup(repo => repo.LoadUserPhoneNumberAndEmailById(It.IsAny<User>()))
                .Returns(Task.CompletedTask);
            mockBuyerRepository.Setup(repo => repo.GetWishlist(It.Is<int>(id => id == linkedBuyer.Id)))
                .ReturnsAsync(new BuyerWishlist());
        }
        
        private void VerifyRepositoryCallsForLinkageTest(Buyer linkedBuyer)
        {
            mockBuyerRepository.Verify(repo => repo.GetBuyerLinkages(testBuyer.Id), Times.Once);
            mockBuyerRepository.Verify(repo => repo.LoadBuyerInfo(It.Is<Buyer>(b => b.Id == testBuyer.Id)), Times.Once);
            mockBuyerRepository.Verify(repo => repo.LoadBuyerInfo(It.Is<Buyer>(b => b.Id == linkedBuyer.Id)), Times.Once);
            mockUserRepository.Verify(repo => repo.LoadUserPhoneNumberAndEmailById(It.Is<User>(u => u.UserId == testBuyer.User.UserId)), Times.Once);
            mockUserRepository.Verify(repo => repo.LoadUserPhoneNumberAndEmailById(It.Is<User>(u => u.UserId == linkedBuyer.User.UserId)), Times.Once);
            // The main buyer's wishlist is NOT loaded when only Linkages flag is specified
            mockBuyerRepository.Verify(repo => repo.GetWishlist(It.Is<int>(id => id == linkedBuyer.Id)), Times.Once);
        }

        [Fact]
        public async Task CreateLinkageRequest_ShouldCreateRequest()
        {
            // Arrange
            var linkedBuyer = new Buyer { UserId = 2 };
            mockBuyerRepository.Setup(repo => repo.CreateLinkageRequest(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            // Act
            await buyerService.CreateLinkageRequest(testBuyer, linkedBuyer);

            // Assert
            mockBuyerRepository.Verify(repo => repo.CreateLinkageRequest(testBuyer.Id, linkedBuyer.Id), Times.Once);
        }

        [Fact]
        public async Task BreakLinkage_ShouldDeleteLinkage()
        {
            // Arrange
            var linkedBuyer = new Buyer { UserId = 2 };
            mockBuyerRepository.Setup(repo => repo.DeleteLinkageRequest(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(true);

            // Act
            await buyerService.BreakLinkage(testBuyer, linkedBuyer);

            // Assert
            mockBuyerRepository.Verify(repo => repo.DeleteLinkageRequest(testBuyer.Id, linkedBuyer.Id), Times.Once);
        }

        [Fact]
        public async Task CancelLinkageRequest_ShouldDeleteRequest()
        {
            // Arrange
            var linkedBuyer = new Buyer { UserId = 2 };
            mockBuyerRepository.Setup(repo => repo.DeleteLinkageRequest(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(true);

            // Act
            await buyerService.CancelLinkageRequest(testBuyer, linkedBuyer);

            // Assert
            mockBuyerRepository.Verify(repo => repo.DeleteLinkageRequest(testBuyer.Id, linkedBuyer.Id), Times.Once);
        }

        [Fact]
        public async Task AcceptLinkageRequest_ShouldUpdateRequest()
        {
            // Arrange
            var linkedBuyer = new Buyer { UserId = 2 };
            mockBuyerRepository.Setup(repo => repo.UpdateLinkageRequest(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            // Act
            await buyerService.AcceptLinkageRequest(testBuyer, linkedBuyer);

            // Assert
            mockBuyerRepository.Verify(repo => repo.UpdateLinkageRequest(linkedBuyer.Id, testBuyer.Id), Times.Once);
        }

        [Fact]
        public async Task RefuseLinkageRequest_ShouldDeleteRequest()
        {
            // Arrange
            var linkedBuyer = new Buyer { UserId = 2 };
            mockBuyerRepository.Setup(repo => repo.DeleteLinkageRequest(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(true);

            // Act
            await buyerService.RefuseLinkageRequest(testBuyer, linkedBuyer);

            // Assert
            mockBuyerRepository.Verify(repo => repo.DeleteLinkageRequest(linkedBuyer.Id, testBuyer.Id), Times.Once);
        }

        [Fact]
        public async Task GetFollowingUsersIDs_ShouldReturnIds()
        {
            // Arrange
            var expectedIds = new List<int> { 1, 2, 3 };
            mockBuyerRepository.Setup(repo => repo.GetFollowingUsersIds(It.IsAny<int>()))
                .ReturnsAsync(expectedIds);

            // Act
            var result = await buyerService.GetFollowingUsersIDs(testBuyer.Id);

            // Assert
            Assert.Equal(expectedIds, result);
            mockBuyerRepository.Verify(repo => repo.GetFollowingUsersIds(testBuyer.Id), Times.Once);
        }

        [Fact]
        public async Task GetProductsFromFollowedSellers_ShouldReturnProducts()
        {
            // Arrange
            var sellerIds = new List<int> { 1, 2 };
            var expectedProducts = new List<Product> { new Product(1, "name1", "desc1", 1.2, 3, 1), new Product(2, "name2", "desc2", 1.22, 32, 12) };
            mockBuyerRepository.Setup(repo => repo.GetProductsFromSeller(It.IsAny<int>()))
                .ReturnsAsync(expectedProducts);

            // Act
            var result = await buyerService.GetProductsFromFollowedSellers(sellerIds);

            // Assert
            var expectedProductCount = expectedProducts.Count * sellerIds.Count;
            Assert.Equal(expectedProductCount, result.Count);
            mockBuyerRepository.Verify(repo => repo.GetProductsFromSeller(It.IsAny<int>()), Times.Exactly(2));
        }

        [Fact]
        public async Task GetFollowedSellers_ShouldReturnSellers()
        {
            // Arrange
            var followingIds = new List<int> { 1, 2 };
            // Create a User object to pass to the Seller constructor
            var user1 = new User(1, "seller1", "seller1@example.com");
            var user2 = new User(2, "seller2", "seller2@example.com");

            // Use the proper constructor
            var expectedSellers = new List<Seller> {
                new Seller(user1),
                new Seller(user2)
           };
            mockBuyerRepository.Setup(repo => repo.GetFollowedSellers(It.IsAny<List<int>>()))
                .ReturnsAsync(expectedSellers);

            // Act
            var result = await buyerService.GetFollowedSellers(followingIds);

            // Assert
            Assert.Equal(expectedSellers, result);
            mockBuyerRepository.Verify(repo => repo.GetFollowedSellers(followingIds), Times.Once);
        }

        [Fact]
        public async Task GetAllSellers_ShouldReturnAllSellers()
        {
            // Arrange
            var user1 = new User(1, "seller1", "seller1@example.com");
            var user2 = new User(2, "seller2", "seller2@example.com");
            var expectedSellers = new List<Seller> { new Seller(user1), new Seller(user2) };
            mockBuyerRepository.Setup(repo => repo.GetAllSellers())
                .ReturnsAsync(expectedSellers);

            // Act
            var result = await buyerService.GetAllSellers();

            // Assert
            Assert.Equal(expectedSellers, result);
            mockBuyerRepository.Verify(repo => repo.GetAllSellers(), Times.Once);
        }

        [Fact]
        public async Task UpdateAfterPurchase_ShouldUpdateBuyerStats()
        {
            // Arrange
            const decimal purchaseAmount = 100.0m;
            mockBuyerRepository.Setup(repo => repo.UpdateAfterPurchase(It.IsAny<Buyer>()))
                .Returns(Task.CompletedTask);

            // Act
            await buyerService.UpdateAfterPurchase(testBuyer, purchaseAmount);

            // Assert
            var expectedNumberOfPurchases = 1;
            Assert.Equal(purchaseAmount, testBuyer.TotalSpending);
            Assert.Equal(expectedNumberOfPurchases, testBuyer.NumberOfPurchases);
            mockBuyerRepository.Verify(repo => repo.UpdateAfterPurchase(testBuyer), Times.Once);
        }

        [Fact]
        public async Task RemoveWishilistItem_ShouldRemoveItem()
        {
            // Arrange
            const int productId = 1;
            testBuyer.Wishlist = new BuyerWishlist();
            var wishlistItem = new BuyerWishlistItem(productId);
            testBuyer.Wishlist.Items.Add(wishlistItem);
            mockBuyerRepository.Setup(repo => repo.RemoveWishilistItem(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            // Act
            await buyerService.RemoveWishilistItem(testBuyer, productId);

            // Assert
            Assert.Empty(testBuyer.Wishlist.Items);
            mockBuyerRepository.Verify(repo => repo.RemoveWishilistItem(testBuyer.Id, productId), Times.Once);
        }

        [Fact]
        public async Task RemoveWishilistItem_ShouldNotModifyWishlistWhenProductNotFound()
        {
            // Arrange
            const int productId = 1;
            const int nonExistentProductId = 999;
            testBuyer.Wishlist = new BuyerWishlist();
            var wishlistItem = new BuyerWishlistItem(productId);
            testBuyer.Wishlist.Items.Add(wishlistItem);
            mockBuyerRepository.Setup(repo => repo.RemoveWishilistItem(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            // Act
            await buyerService.RemoveWishilistItem(testBuyer, nonExistentProductId);

            // Assert
            Assert.Single(testBuyer.Wishlist.Items);
            Assert.Equal(productId, testBuyer.Wishlist.Items[0].ProductId);
            mockBuyerRepository.Verify(repo => repo.RemoveWishilistItem(testBuyer.Id, nonExistentProductId), Times.Once);
        }

        [Fact]
        public async Task GetProductsForViewProfile_ShouldReturnProducts()
        {
            // Arrange
            const int sellerId = 1;
            var expectedProducts = new List<Product> { new Product(1, "name1", "desc1", 1.2, 3, 1), new Product(2, "name2", "desc2", 1.22, 32, 12) };
            mockBuyerRepository.Setup(repo => repo.GetProductsFromSeller(It.IsAny<int>()))
                .ReturnsAsync(expectedProducts);

            // Act
            var result = await buyerService.GetProductsForViewProfile(sellerId);

            // Assert
            Assert.Equal(expectedProducts, result);
            mockBuyerRepository.Verify(repo => repo.GetProductsFromSeller(sellerId), Times.Once);
        }

        [Fact]
        public async Task CheckIfBuyerExists_ShouldReturnTrue()
        {
            // Arrange
            mockBuyerRepository.Setup(repo => repo.CheckIfBuyerExists(It.IsAny<int>()))
                .ReturnsAsync(true);

            // Act
            var result = await buyerService.CheckIfBuyerExists(testBuyer.Id);

            // Assert
            Assert.True(result);
            mockBuyerRepository.Verify(repo => repo.CheckIfBuyerExists(testBuyer.Id), Times.Once);
        }

        [Fact]
        public async Task IsFollowing_ShouldReturnTrue()
        {
            // Arrange
            const int sellerId = 1;
            mockBuyerRepository.Setup(repo => repo.IsFollowing(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(true);

            // Act
            var result = await buyerService.IsFollowing(testBuyer.Id, sellerId);

            // Assert
            Assert.True(result);
            mockBuyerRepository.Verify(repo => repo.IsFollowing(testBuyer.Id, sellerId), Times.Once);
        }

        [Fact]
        public async Task FollowSeller_ShouldFollow()
        {
            // Arrange
            const int sellerId = 1;
            mockBuyerRepository.Setup(repo => repo.FollowSeller(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            // Act
            await buyerService.FollowSeller(testBuyer.Id, sellerId);

            // Assert
            mockBuyerRepository.Verify(repo => repo.FollowSeller(testBuyer.Id, sellerId), Times.Once);
        }

        [Fact]
        public async Task UnfollowSeller_ShouldUnfollow()
        {
            // Arrange
            const int sellerId = 1;
            mockBuyerRepository.Setup(repo => repo.UnfollowSeller(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            // Act
            await buyerService.UnfollowSeller(testBuyer.Id, sellerId);

            // Assert
            mockBuyerRepository.Verify(repo => repo.UnfollowSeller(testBuyer.Id, sellerId), Times.Once);
        }

        [Fact]
        public void GetBadgeProgress_ShouldCalculateProgress()
        {
            // Arrange
            testBuyer.TotalSpending = 500.0m;
            testBuyer.NumberOfPurchases = 50;

            // Act
            var result = buyerService.GetBadgeProgress(testBuyer);

            // Assert
            var minimumBadgeProgressValue = 1;
            var maximumBadgeProgressValue = 100;
            Assert.True(result >= minimumBadgeProgressValue && result <= maximumBadgeProgressValue);
        }

        [Fact]
        public async Task SaveInfo_WithValidData_ShouldUpdateRepositories()
        {
            // Arrange
            var mockUserValidator = new Mock<IUserValidator>();
            mockUserValidator.Setup(validator => validator.IsValidPhoneNumber(It.IsAny<string>()))
                .Returns(true);
            var userValidatorField = typeof(BuyerService).GetField("userValidator",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            userValidatorField!.SetValue(buyerService, mockUserValidator.Object);

            mockBuyerRepository.Setup(repo => repo.SaveInfo(It.IsAny<Buyer>()))
                .Returns(Task.CompletedTask);
            mockUserRepository.Setup(repo => repo.UpdateUserPhoneNumber(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            // Act
            await buyerService.SaveInfo(testBuyer);

            // Assert
            mockBuyerRepository.Verify(repo => repo.SaveInfo(testBuyer), Times.Once);
            mockUserRepository.Verify(repo => repo.UpdateUserPhoneNumber(testBuyer.User), Times.Once);
        }

        [Fact]
        public async Task ValidateAddress_WithNullStreetLine_ShouldThrowArgumentException()
        {
            // Arrange
            var mockUserValidator = new Mock<IUserValidator>();
            mockUserValidator.Setup(validator => validator.IsValidPhoneNumber(It.IsAny<string>()))
                .Returns(true);
            var userValidatorField = typeof(BuyerService).GetField("userValidator",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            userValidatorField!.SetValue(buyerService, mockUserValidator.Object);

            var invalidAddress = new Address
            {
                Id = 1,
                StreetLine = null,
                City = "Test City",
                Country = "Test Country",
                PostalCode = "12345"
            };

            testBuyer.BillingAddress = invalidAddress;
            testBuyer.UseSameAddress = true; // This ensures the shipping address is also set to the invalid address

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => buyerService.SaveInfo(testBuyer));
            var expectedExceptionMessage = "Street Name is required";
            Assert.Contains(expectedExceptionMessage, exception.Message);
            mockBuyerRepository.Verify(repo => repo.SaveInfo(It.IsAny<Buyer>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAfterPurchase_ShouldUpdateBadgeToSilver()
        {
            // Arrange
            const decimal silverThresholdPurchase = BuyerConfiguration.SilverThreshold;

            mockBuyerRepository.Setup(repo => repo.UpdateAfterPurchase(It.IsAny<Buyer>()))
                .Returns(Task.CompletedTask);

            testBuyer.Badge = BuyerBadge.BRONZE;
            testBuyer.TotalSpending = 0;
            testBuyer.NumberOfPurchases = 0;

            // Act
            await buyerService.UpdateAfterPurchase(testBuyer, silverThresholdPurchase);

            // Assert
            var expectedNumberOfPurchases = 1;
            var expectedBadge = BuyerBadge.SILVER;
            Assert.Equal(silverThresholdPurchase, testBuyer.TotalSpending);
            Assert.Equal(expectedNumberOfPurchases, testBuyer.NumberOfPurchases);
            Assert.Equal(expectedBadge, testBuyer.Badge);
            mockBuyerRepository.Verify(repo => repo.UpdateAfterPurchase(testBuyer), Times.Once);
        }


        [Fact]
        public async Task UpdateAfterPurchase_ShouldUpdateDiscount()
        {
            // Arrange
            const decimal goldThresholdPurchase = BuyerConfiguration.GoldThreshold;

            mockBuyerRepository.Setup(repo => repo.UpdateAfterPurchase(It.IsAny<Buyer>()))
                .Returns(Task.CompletedTask);

            testBuyer.Badge = BuyerBadge.BRONZE;
            testBuyer.TotalSpending = 0;
            testBuyer.Discount = 0;

            // Act
            await buyerService.UpdateAfterPurchase(testBuyer, goldThresholdPurchase);

            // Assert
            var expectedNumberOfPurchases = 1;
            var expectedBadge = BuyerBadge.GOLD;
            Assert.Equal(goldThresholdPurchase, testBuyer.TotalSpending);
            Assert.Equal(expectedNumberOfPurchases, testBuyer.NumberOfPurchases);
            Assert.Equal(expectedBadge, testBuyer.Badge);
            Assert.Equal(BuyerConfiguration.GoldDiscount, testBuyer.Discount);
            mockBuyerRepository.Verify(repo => repo.UpdateAfterPurchase(testBuyer), Times.Once);
        }


        [Fact]
        public void ValidateMandatoryField_WithEmptyString_ShouldThrowArgumentException()
        {
            // Arrange
            var methodInfo = typeof(BuyerService).GetMethod("ValidateMandatoryField",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act & Assert
            var exception = Assert.Throws<System.Reflection.TargetInvocationException>(() =>
                methodInfo!.Invoke(buyerService, new object[] { "Test Field", string.Empty }));

            Assert.IsType<ArgumentException>(exception.InnerException);
            var expectedExceptionMessage = "Test Field is required";
            Assert.Contains(expectedExceptionMessage, exception.InnerException.Message);
        }

        [Fact]
        public void UserValidator_ShouldBeInitializedInConstructor()
        {
            // Arrange & Act
            var newBuyerService = new BuyerService(mockBuyerRepository.Object, mockUserRepository.Object);

            // Assert
            var userValidatorField = typeof(BuyerService).GetField("userValidator",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var userValidator = userValidatorField!.GetValue(newBuyerService);

            Assert.NotNull(userValidator);
            Assert.IsAssignableFrom<IUserValidator>(userValidator);
        }

        [Fact]
        public async Task ValidateBuyerInfo_WithEmptyLastName_ShouldThrowArgumentException()
        {
            // Arrange
            var mockUserValidator = new Mock<IUserValidator>();
            mockUserValidator.Setup(validator => validator.IsValidPhoneNumber(It.IsAny<string>()))
                .Returns(true);
            var userValidatorField = typeof(BuyerService).GetField("userValidator",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            userValidatorField!.SetValue(buyerService, mockUserValidator.Object);

            testBuyer.LastName = string.Empty;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => buyerService.SaveInfo(testBuyer));
            var expectedExceptionMessage = "Last name cannot be empty";
            Assert.Contains(expectedExceptionMessage, exception.Message);
            mockBuyerRepository.Verify(repo => repo.SaveInfo(It.IsAny<Buyer>()), Times.Never);
        }

        [Fact]
        public async Task ValidateAddress_WithNullCity_ShouldThrowArgumentException()
        {
            // Arrange
            var mockUserValidator = new Mock<IUserValidator>();
            mockUserValidator.Setup(validator => validator.IsValidPhoneNumber(It.IsAny<string>()))
                .Returns(true);
            var userValidatorField = typeof(BuyerService).GetField("userValidator",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            userValidatorField!.SetValue(buyerService, mockUserValidator.Object);

            var invalidAddress = new Address
            {
                Id = 1,
                StreetLine = "Test Street",
                City = null,
                Country = "Test Country",
                PostalCode = "12345"
            };

            testBuyer.BillingAddress = invalidAddress;
            testBuyer.UseSameAddress = true;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => buyerService.SaveInfo(testBuyer));
            var expectedExceptionMessage = "City is required";
            Assert.Contains(expectedExceptionMessage, exception.Message);
            mockBuyerRepository.Verify(repo => repo.SaveInfo(It.IsAny<Buyer>()), Times.Never);
        }

        [Fact]
        public async Task BreakLinkage_WhenFirstDeleteFails_ShouldTrySecondDelete()
        {
            // Arrange
            var linkedBuyer = new Buyer { UserId = 2 };
            mockBuyerRepository.Setup(repo => repo.DeleteLinkageRequest(testBuyer.Id, linkedBuyer.Id))
                .ReturnsAsync(false);
            mockBuyerRepository.Setup(repo => repo.DeleteLinkageRequest(linkedBuyer.Id, testBuyer.Id))
                .ReturnsAsync(true);

            // Act
            await buyerService.BreakLinkage(testBuyer, linkedBuyer);

            // Assert
            mockBuyerRepository.Verify(repo => repo.DeleteLinkageRequest(testBuyer.Id, linkedBuyer.Id), Times.Once);
            mockBuyerRepository.Verify(repo => repo.DeleteLinkageRequest(linkedBuyer.Id, testBuyer.Id), Times.Once);
        }

        [Fact]
        public async Task LoadBuyer_WithLinkagesAndNoLinkedBuyers_ShouldNotLoadAdditionalInfo()
        {
            // Arrange
            var linkages = new List<BuyerLinkage>();
            mockBuyerRepository.Setup(repo => repo.GetBuyerLinkages(It.IsAny<int>()))
                .ReturnsAsync(linkages);
            mockBuyerRepository.Setup(repo => repo.LoadBuyerInfo(It.IsAny<Buyer>()))
                .Returns(Task.CompletedTask);
            mockUserRepository.Setup(repo => repo.LoadUserPhoneNumberAndEmailById(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            // Act
            await buyerService.LoadBuyer(testBuyer, BuyerDataSegments.Linkages);

            // Assert
            Assert.Equal(linkages, testBuyer.Linkages);
            mockBuyerRepository.Verify(repo => repo.GetBuyerLinkages(testBuyer.Id), Times.Once);
            // The LoadBuyerInfo is called once for the main buyer
            mockBuyerRepository.Verify(repo => repo.LoadBuyerInfo(It.Is<Buyer>(b => b.Id == testBuyer.Id)), Times.Once);
            // The LoadUserPhoneNumberAndEmailById is called once for the main buyer
            mockUserRepository.Verify(repo => repo.LoadUserPhoneNumberAndEmailById(It.Is<User>(u => u.UserId == testBuyer.User.UserId)), Times.Once);
            // The wishlist is NOT loaded when there are no linked buyers and only Linkages flag is specified
            mockBuyerRepository.Verify(repo => repo.GetWishlist(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task ValidateBuyerInfo_WithInvalidPhoneNumber_ShouldThrowArgumentException()
        {
            // Arrange
            var mockUserValidator = new Mock<IUserValidator>();
            mockUserValidator.Setup(validator => validator.IsValidPhoneNumber(It.IsAny<string>()))
                .Returns(false);
            var userValidatorField = typeof(BuyerService).GetField("userValidator",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            userValidatorField!.SetValue(buyerService, mockUserValidator.Object);

            testBuyer.PhoneNumber = "invalid";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => buyerService.SaveInfo(testBuyer));
            var expectedExceptionMessage = "Invalid Phone Number";
            Assert.Contains(expectedExceptionMessage, exception.Message);
            mockBuyerRepository.Verify(repo => repo.SaveInfo(It.IsAny<Buyer>()), Times.Never);
        }

        [Fact]
        public async Task ValidateBuyerInfo_WithNullBillingAddress_ShouldThrowArgumentException()
        {
            // Arrange
            var mockUserValidator = new Mock<IUserValidator>();
            mockUserValidator.Setup(validator => validator.IsValidPhoneNumber(It.IsAny<string>()))
                .Returns(true);
            var userValidatorField = typeof(BuyerService).GetField("userValidator",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            userValidatorField!.SetValue(buyerService, mockUserValidator.Object);

            testBuyer.BillingAddress = null;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => buyerService.SaveInfo(testBuyer));
            var expectedExceptionMessage = "Address cannot be null";
            Assert.Contains(expectedExceptionMessage, exception.Message);
            mockBuyerRepository.Verify(repo => repo.SaveInfo(It.IsAny<Buyer>()), Times.Never);
        }

        [Fact]
        public async Task ValidateBuyerInfo_WithDifferentShippingAddress_ShouldValidateBothAddresses()
        {
            // Arrange
            var mockUserValidator = new Mock<IUserValidator>();
            mockUserValidator.Setup(validator => validator.IsValidPhoneNumber(It.IsAny<string>()))
                .Returns(true);
            var userValidatorField = typeof(BuyerService).GetField("userValidator",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            userValidatorField!.SetValue(buyerService, mockUserValidator.Object);

            testBuyer.UseSameAddress = false;
            testBuyer.ShippingAddress = new Address
            {
                Id = 2,
                StreetLine = "Different Street",
                City = "Different City",
                Country = "Different Country",
                PostalCode = "54321"
            };

            // Act
            await buyerService.SaveInfo(testBuyer);

            // Assert
            mockBuyerRepository.Verify(repo => repo.SaveInfo(testBuyer), Times.Once);
            mockUserRepository.Verify(repo => repo.UpdateUserPhoneNumber(testBuyer.User), Times.Once);
        }

        [Fact]
        public async Task ValidateBuyerInfo_WithInvalidShippingAddress_ShouldThrowArgumentException()
        {
            // Arrange
            var mockUserValidator = new Mock<IUserValidator>();
            mockUserValidator.Setup(validator => validator.IsValidPhoneNumber(It.IsAny<string>()))
                .Returns(true);
            var userValidatorField = typeof(BuyerService).GetField("userValidator",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            userValidatorField!.SetValue(buyerService, mockUserValidator.Object);

            testBuyer.UseSameAddress = false;
            testBuyer.ShippingAddress = new Address
            {
                Id = 2,
                StreetLine = null,
                City = "Different City",
                Country = "Different Country",
                PostalCode = "54321"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => buyerService.SaveInfo(testBuyer));
            var expectedExceptionMessage = "Street Name is required";
            Assert.Contains(expectedExceptionMessage, exception.Message);
            mockBuyerRepository.Verify(repo => repo.SaveInfo(It.IsAny<Buyer>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAfterPurchase_ShouldUpdateBadgeToPlatinum()
        {
            // Arrange
            const decimal platinumThresholdPurchase = BuyerConfiguration.PlatinumThreshold;

            mockBuyerRepository.Setup(repo => repo.UpdateAfterPurchase(It.IsAny<Buyer>()))
                .Returns(Task.CompletedTask);

            testBuyer.Badge = BuyerBadge.BRONZE;
            testBuyer.TotalSpending = 0;
            testBuyer.NumberOfPurchases = 0;
            testBuyer.Discount = 0;

            // Act
            await buyerService.UpdateAfterPurchase(testBuyer, platinumThresholdPurchase);

            // Assert
            var expectedNumberOfPurchases = 1;
            var expectedBadge = BuyerBadge.PLATINUM;
            Assert.Equal(platinumThresholdPurchase, testBuyer.TotalSpending);
            Assert.Equal(expectedNumberOfPurchases, testBuyer.NumberOfPurchases);
            Assert.Equal(expectedBadge, testBuyer.Badge);
            Assert.Equal(BuyerConfiguration.PlatinumDiscount, testBuyer.Discount);
            mockBuyerRepository.Verify(repo => repo.UpdateAfterPurchase(testBuyer), Times.Once);
        }

    }
}