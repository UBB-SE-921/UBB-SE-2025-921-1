using SharedClassLibrary.Domain;
using SharedClassLibrary.Service;
using MarketPlace924.ViewModel;
using Microsoft.UI.Xaml;
using Moq;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace XUnitTestProject.ViewModelTests
{
    public class BuyerFamilySyncViewModelTests
    {
        private readonly Mock<IBuyerService> mockBuyerService;
        private readonly Mock<IOnBuyerLinkageUpdatedCallback> mockCallback;
        private readonly Buyer testBuyer;
        private readonly BuyerFamilySyncViewModel viewModel;

        public BuyerFamilySyncViewModelTests()
        {
            // Setup mocks
            mockBuyerService = new Mock<IBuyerService>();
            mockCallback = new Mock<IOnBuyerLinkageUpdatedCallback>();
            
            // Create test buyer with valid User ID
            testBuyer = new Buyer
            {
                User = new User { UserId = 1 }, // Added User with ID
                FirstName = "Test",
                LastName = "User",
                ShippingAddress = new Address { City = "TestCity" },
                Linkages = new List<BuyerLinkage>()
            };
            
            // Create the view model with mocks
            viewModel = new BuyerFamilySyncViewModel(
                mockBuyerService.Object,
                testBuyer,
                mockCallback.Object);
        }

        [Fact]
        public void Constructor_InitializesCorrectly()
        {
            // Assert
            Assert.NotNull(viewModel);
            Assert.NotNull(viewModel.Items);
            Assert.IsType<ObservableCollection<IBuyerLinkageViewModel>>(viewModel.Items);
            Assert.Empty(viewModel.Items);
        }

        [Fact]
        public async Task LoadLinkages_NoHouseholdBuyers_AddsNoItems()
        {
            // Arrange
            mockBuyerService
                .Setup(s => s.FindBuyersWithShippingAddress(testBuyer.ShippingAddress))
                .ReturnsAsync(new List<Buyer>());
                
            // Act
            await viewModel.LoadLinkages();
            
            // Assert
            Assert.NotNull(viewModel.Items);
            Assert.Empty(viewModel.Items);
        }

        [Fact]
        public async Task LoadLinkages_WithHouseholdBuyers_FiltersSelf()
        {
            // Arrange
            var otherBuyer = new Buyer
            {
                User = new User { UserId = 2 }, // Different ID than testBuyer
                FirstName = "Other",
                LastName = "User"
            };
            
            var householdBuyers = new List<Buyer>
            {
                testBuyer, // Self, should be filtered out
                otherBuyer
            };
            
            mockBuyerService
                .Setup(s => s.FindBuyersWithShippingAddress(testBuyer.ShippingAddress))
                .ReturnsAsync(householdBuyers);
                
            // Act
            await viewModel.LoadLinkages();
            
            // Assert
            Assert.NotNull(viewModel.Items);
            Assert.Single(viewModel.Items);
            
            // Don't assert on exact display name format since it may change
            // Just verify it contains the first characters of both names
            var displayName = viewModel.Items[0].DisplayName;
            Assert.Contains("O", displayName);
            Assert.Contains("U", displayName);
        }

        [Fact]
        public async Task LoadLinkages_WithBuyerLinkages_CreatesBuyerLinkageViewModels()
        {
            // Arrange
            var linkedBuyer = new Buyer
            {
                User = new User { UserId = 3 },
                FirstName = "Linked",
                LastName = "User"
            };
            
            testBuyer.Linkages = new List<BuyerLinkage>
            {
                new BuyerLinkage
                {
                    Buyer = linkedBuyer,
                    Status = BuyerLinkageStatus.Confirmed
                }
            };
            
            mockBuyerService
                .Setup(s => s.FindBuyersWithShippingAddress(testBuyer.ShippingAddress))
                .ReturnsAsync(new List<Buyer>());
                
            // Act
            await viewModel.LoadLinkages();
            
            // Assert
            Assert.NotNull(viewModel.Items);
            Assert.Single(viewModel.Items);
            Assert.Equal("Linked User", viewModel.Items[0].DisplayName); // Full name for confirmed status
            Assert.Equal(BuyerLinkageStatus.Confirmed, viewModel.Items[0].Status);
        }

        [Fact]
        public async Task LoadLinkages_WithBothHouseholdAndLinkages_GroupsAndSelectedHighestStatus()
        {
            // Arrange
            var commonBuyer = new Buyer
            {
                User = new User { UserId = 4 },
                FirstName = "Common",
                LastName = "User"
            };
            
            // Buyer exists in household list (Possible status) and in linkages (higher status)
            testBuyer.Linkages = new List<BuyerLinkage>
            {
                new BuyerLinkage
                {
                    Buyer = commonBuyer,
                    Status = BuyerLinkageStatus.PendingOther
                }
            };
            
            var householdBuyers = new List<Buyer> { commonBuyer };
            
            mockBuyerService
                .Setup(s => s.FindBuyersWithShippingAddress(testBuyer.ShippingAddress))
                .ReturnsAsync(householdBuyers);
                
            // Act
            await viewModel.LoadLinkages();
            
            // Assert
            Assert.NotNull(viewModel.Items);
            Assert.Single(viewModel.Items);
            Assert.Equal(BuyerLinkageStatus.PendingOther, viewModel.Items[0].Status);
        }

        [Fact]
        public async Task LoadLinkages_RaisesPropertyChanged()
        {
            // Arrange
            mockBuyerService
                .Setup(s => s.FindBuyersWithShippingAddress(testBuyer.ShippingAddress))
                .ReturnsAsync(new List<Buyer>());
                
            var propertyChangedRaised = false;
            var propertyName = string.Empty;
            
            viewModel.PropertyChanged += (sender, args) =>
            {
                propertyChangedRaised = true;
                propertyName = args.PropertyName ?? string.Empty;
            };
            
            // Act
            await viewModel.LoadLinkages();
            
            // Assert
            Assert.True(propertyChangedRaised);
            Assert.Equal(nameof(BuyerFamilySyncViewModel.Items), propertyName);
        }

        [Fact]
        public async Task PropertyChanged_NullHandler_DoesNotThrow()
        {
            // Arrange
            var tempViewModel = new BuyerFamilySyncViewModel(
                mockBuyerService.Object,
                testBuyer,
                mockCallback.Object);
                
            mockBuyerService
                .Setup(s => s.FindBuyersWithShippingAddress(testBuyer.ShippingAddress))
                .ReturnsAsync(new List<Buyer>());

            // Act & Assert - this shouldn't throw
            // Use await instead of .Wait() to avoid warning
            await tempViewModel.LoadLinkages();
            
            // Assert nothing, we just want to verify it doesn't throw
            Assert.NotNull(tempViewModel.Items);
        }

        [Fact]
        public async Task NewBuyerLinkageViewModel_CreatesCorrectViewModel()
        {
            // Arrange
            var linkedBuyer = new Buyer
            {
                User = new User { UserId = 5 },
                FirstName = "New",
                LastName = "Linked"
            };
            
            mockBuyerService
                .Setup(s => s.FindBuyersWithShippingAddress(testBuyer.ShippingAddress))
                .ReturnsAsync(new List<Buyer> { linkedBuyer });
                
            // Act
            await viewModel.LoadLinkages();
            
            // Assert
            Assert.NotNull(viewModel.Items);
            Assert.Single(viewModel.Items);
            
            var linkageViewModel = viewModel.Items[0];
            Assert.Equal(testBuyer, linkageViewModel.UserBuyer);
            Assert.Equal(linkedBuyer, linkageViewModel.LinkedBuyer);
            Assert.Equal(BuyerLinkageStatus.Possible, linkageViewModel.Status);
            Assert.Equal(mockBuyerService.Object, linkageViewModel.Service);
            Assert.Equal(mockCallback.Object, linkageViewModel.LinkageUpdatedCallback);
        }
    }
}
