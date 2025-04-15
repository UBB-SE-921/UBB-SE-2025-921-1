using MarketPlace924.Domain;
using MarketPlace924.Service;
using MarketPlace924.ViewModel;
using Moq;
using System.ComponentModel;

namespace XUnitTestProject.ViewModelTests
{
    public class BuyerBadgeViewModelTests
    {
        private readonly Mock<IBuyerService> mockBuyerService;
        private readonly BuyerBadgeViewModel viewModel;
        private readonly Buyer testBuyer;

        public BuyerBadgeViewModelTests()
        {
            // Setup mock service
            mockBuyerService = new Mock<IBuyerService>();
            
            // Create the view model with the mock service
            viewModel = new BuyerBadgeViewModel(mockBuyerService.Object);
            
            // Create a test buyer
            testBuyer = new Buyer
            {
                Badge = BuyerBadge.SILVER,
                Discount = 0.05m
            };
        }

        [Fact]
        public void Constructor_InitializesCorrectly()
        {
            // Assert that the view model was created with the correct service
            var model = new BuyerBadgeViewModel(mockBuyerService.Object);
            
            // The constructor doesn't set any properties, so we can't assert much
            // but we can verify it doesn't throw exceptions
            Assert.NotNull(model);
        }

        [Fact]
        public void Progress_WhenBuyerIsNull_ReturnsZero()
        {
            // Act - don't set the Buyer property
            var progress = viewModel.Progress;
            
            // Assert
            Assert.Equal(0, progress);
        }

        [Theory]
        [InlineData(10)]
        [InlineData(50)]
        [InlineData(94)]
        public void Progress_NormalProgress_ReturnsModuloValue(int progressValue)
        {
            // Arrange
            viewModel.Buyer = testBuyer;
            mockBuyerService.Setup(s => s.GetBadgeProgress(testBuyer)).Returns(progressValue);
            
            // Act
            var result = viewModel.Progress;
            
            // Assert - should return progressValue % 25
            Assert.Equal(progressValue % 25, result);
        }

        [Theory]
        [InlineData(95)]
        [InlineData(100)]
        public void Progress_HighProgress_ReturnsCapValue(int progressValue)
        {
            // Arrange
            viewModel.Buyer = testBuyer;
            mockBuyerService.Setup(s => s.GetBadgeProgress(testBuyer)).Returns(progressValue);
            
            // Act
            var result = viewModel.Progress;
            
            // Assert - should return 24 for any value >= 95
            Assert.Equal(24, result);
        }

        [Fact]
        public void Discount_ReturnsFormattedString()
        {
            // Arrange
            viewModel.Buyer = testBuyer;
            
            // Act
            var result = viewModel.Discount;
            
            // Assert
            Assert.Equal("Discount " + testBuyer.Discount, result);
        }

        [Fact]
        public void Discount_WhenBuyerIsNull_ThrowsNullReferenceException()
        {
            // Arrange - don't set Buyer property
            
            // Act & Assert
            Assert.Throws<NullReferenceException>(() => _ = viewModel.Discount);
        }

        [Theory]
        [InlineData(BuyerBadge.BRONZE, "bronze")]
        [InlineData(BuyerBadge.SILVER, "silver")]
        [InlineData(BuyerBadge.GOLD, "gold")]
        [InlineData(BuyerBadge.PLATINUM, "platinum")]
        public void BadgeName_ReturnsBadgeNameLowercase(BuyerBadge badge, string expected)
        {
            // Arrange
            testBuyer.Badge = badge;
            viewModel.Buyer = testBuyer;
            
            // Act
            var result = viewModel.BadgeName;
            
            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void BadgeName_WhenBuyerIsNull_ThrowsNullReferenceException()
        {
            // Arrange - don't set Buyer property
            
            // Act & Assert
            Assert.Throws<NullReferenceException>(() => _ = viewModel.BadgeName);
        }

        [Theory]
        [InlineData(BuyerBadge.BRONZE, "ms-appx:///Assets/BuyerIcons/badge-bronze.svg")]
        [InlineData(BuyerBadge.SILVER, "ms-appx:///Assets/BuyerIcons/badge-silver.svg")]
        [InlineData(BuyerBadge.GOLD, "ms-appx:///Assets/BuyerIcons/badge-gold.svg")]
        [InlineData(BuyerBadge.PLATINUM, "ms-appx:///Assets/BuyerIcons/badge-platinum.svg")]
        public void ImageSource_ReturnsCorrectPath(BuyerBadge badge, string expected)
        {
            // Arrange
            testBuyer.Badge = badge;
            viewModel.Buyer = testBuyer;
            
            // Act
            var result = viewModel.ImageSource;
            
            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ImageSource_WhenBuyerIsNull_ThrowsNullReferenceException()
        {
            // Arrange - don't set Buyer property
            
            // Act & Assert
            Assert.Throws<NullReferenceException>(() => _ = viewModel.ImageSource);
        }

        [Fact]
        public void Updated_WhenBuyerIsNull_DoesNotRaisePropertyChanged()
        {
            // Arrange
            bool propertyChangedRaised = false;
            viewModel.PropertyChanged += (sender, args) => propertyChangedRaised = true;
            
            // Act
            viewModel.Updated();
            
            // Assert
            Assert.False(propertyChangedRaised);
        }

        [Fact]
        public void Updated_RaisesPropertyChangedForAllProperties()
        {
            // Arrange
            viewModel.Buyer = testBuyer;
            var raisedProperties = new List<string>();
            
            viewModel.PropertyChanged += (sender, args) => 
            {
                if (args.PropertyName != null)
                {
                    raisedProperties.Add(args.PropertyName);
                }
            };
            
            // Act
            viewModel.Updated();
            
            // Assert
            Assert.Contains(nameof(BuyerBadgeViewModel.Progress), raisedProperties);
            Assert.Contains(nameof(BuyerBadgeViewModel.Discount), raisedProperties);
            Assert.Contains(nameof(BuyerBadgeViewModel.BadgeName), raisedProperties);
            Assert.Contains(nameof(BuyerBadgeViewModel.ImageSource), raisedProperties);
            Assert.Equal(4, raisedProperties.Count);
        }

        [Fact]
        public void PropertyChanged_EventSubscription()
        {
            // Arrange
            var eventRaised = false;
            var eventPropertyName = string.Empty;
            
            viewModel.PropertyChanged += (sender, args) => 
            {
                eventRaised = true;
                eventPropertyName = args.PropertyName ?? string.Empty;
            };
            
            viewModel.Buyer = testBuyer;
            
            // Act
            viewModel.Updated();
            
            // Assert
            Assert.True(eventRaised);
        }

        [Fact]
        public void OnPropertyChanged_WithNullHandler_DoesNotThrowException()
        {
            // Arrange
            var tempViewModel = new BuyerBadgeViewModel(mockBuyerService.Object);
            tempViewModel.Buyer = testBuyer;
            
            // No event handlers attached, PropertyChanged will be null
            
            // Act & Assert - should not throw
            tempViewModel.Updated();
        }

        [Fact]
        public void Buyer_SetterAndGetter_WorksCorrectly()
        {
            // Act
            viewModel.Buyer = testBuyer;
            
            // Assert
            Assert.Same(testBuyer, viewModel.Buyer);
        }
    }
}
