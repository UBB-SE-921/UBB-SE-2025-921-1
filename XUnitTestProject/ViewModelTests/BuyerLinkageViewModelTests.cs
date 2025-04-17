using SharedClassLibrary.Domain;
using MarketPlace924.Service;
using MarketPlace924.ViewModel;
using Microsoft.UI.Xaml;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Xunit;

namespace XUnitTestProject.ViewModelTests
{
    public class BuyerLinkageViewModelTests
    {
        private readonly Mock<IBuyerService> mockBuyerService;
        private readonly Mock<IOnBuyerLinkageUpdatedCallback> mockCallback;
        private readonly Buyer userBuyer;
        private readonly Buyer linkedBuyer;
        private BuyerLinkageViewModel viewModel;

        public BuyerLinkageViewModelTests()
        {
            // Setup mocks
            mockBuyerService = new Mock<IBuyerService>();
            mockCallback = new Mock<IOnBuyerLinkageUpdatedCallback>();
            
            // Create test buyers
            userBuyer = new Buyer
            {
                User = new User { UserId = 1 },
                FirstName = "Current",
                LastName = "User"
            };
            
            linkedBuyer = new Buyer
            {
                User = new User { UserId = 2 },
                FirstName = "Linked",
                LastName = "User"
            };
            
            // Create the view model
            viewModel = new BuyerLinkageViewModel
            {
                Service = mockBuyerService.Object,
                UserBuyer = userBuyer,
                LinkedBuyer = linkedBuyer,
                LinkageUpdatedCallback = mockCallback.Object,
                Status = BuyerLinkageStatus.Possible
            };
        }

        [Fact]
        public void Status_Possible_SetsCorrectVisibility()
        {
            // Act
            viewModel.Status = BuyerLinkageStatus.Possible;
            
            // Assert
            Assert.Equal(Visibility.Visible, viewModel.RequestSyncVsbl);
            Assert.Equal(Visibility.Collapsed, viewModel.UnsyncVsbl);
            Assert.Equal(Visibility.Collapsed, viewModel.AcceptVsbl);
            Assert.Equal(Visibility.Collapsed, viewModel.DeclineVsbl);
            
            // DisplayName should be masked
            var displayName = viewModel.DisplayName;
            Assert.Contains("L", displayName);
            Assert.Contains("U", displayName);
        }

        [Fact]
        public void Status_PendingSelf_SetsCorrectVisibility()
        {
            // Act
            viewModel.Status = BuyerLinkageStatus.PendingSelf;
            
            // Assert
            Assert.Equal(Visibility.Collapsed, viewModel.RequestSyncVsbl);
            Assert.Equal(Visibility.Collapsed, viewModel.UnsyncVsbl);
            Assert.Equal(Visibility.Visible, viewModel.AcceptVsbl);
            Assert.Equal(Visibility.Visible, viewModel.DeclineVsbl);
            
            // DisplayName should be unmasked for pending self
            Assert.Equal("Linked User", viewModel.DisplayName);
        }

        [Fact]
        public void Status_PendingOther_SetsCorrectVisibility()
        {
            // Act
            viewModel.Status = BuyerLinkageStatus.PendingOther;
            
            // Assert
            Assert.Equal(Visibility.Collapsed, viewModel.RequestSyncVsbl);
            Assert.Equal(Visibility.Visible, viewModel.UnsyncVsbl);
            Assert.Equal(Visibility.Collapsed, viewModel.AcceptVsbl);
            Assert.Equal(Visibility.Collapsed, viewModel.DeclineVsbl);
            
            // DisplayName should be masked for pending other
            var displayName = viewModel.DisplayName;
            Assert.Contains("L", displayName);
            Assert.Contains("U", displayName);
        }

        [Fact]
        public void Status_Confirmed_SetsCorrectVisibility()
        {
            // Act
            viewModel.Status = BuyerLinkageStatus.Confirmed;
            
            // Assert
            Assert.Equal(Visibility.Collapsed, viewModel.RequestSyncVsbl);
            Assert.Equal(Visibility.Visible, viewModel.UnsyncVsbl);
            Assert.Equal(Visibility.Collapsed, viewModel.AcceptVsbl);
            Assert.Equal(Visibility.Collapsed, viewModel.DeclineVsbl);
            
            // DisplayName should be unmasked for confirmed
            Assert.Equal("Linked User", viewModel.DisplayName);
        }

        [Fact]
        public void Status_RaisesPropertyChangedEvents()
        {
            // Arrange
            var raisedProperties = new List<string>();
            viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName != null)
                {
                    raisedProperties.Add(args.PropertyName);
                }
            };
            
            // Act
            viewModel.Status = BuyerLinkageStatus.PendingSelf;
            
            // Assert
            Assert.Contains(nameof(BuyerLinkageViewModel.Status), raisedProperties);
            Assert.Contains(nameof(BuyerLinkageViewModel.RequestSyncVsbl), raisedProperties);
            Assert.Contains(nameof(BuyerLinkageViewModel.UnsyncVsbl), raisedProperties);
            Assert.Contains(nameof(BuyerLinkageViewModel.AcceptVsbl), raisedProperties);
            Assert.Contains(nameof(BuyerLinkageViewModel.DeclineVsbl), raisedProperties);
            Assert.Contains(nameof(BuyerLinkageViewModel.DisplayName), raisedProperties);
        }

        [Fact]
        public async Task RequestSync_CallsServiceAndUpdatesStatus()
        {
            // Arrange
            mockBuyerService
                .Setup(s => s.CreateLinkageRequest(userBuyer, linkedBuyer))
                .Returns(Task.CompletedTask);
                
            // Act
            await viewModel.RequestSync();
            
            // Assert
            mockBuyerService.Verify(s => s.CreateLinkageRequest(userBuyer, linkedBuyer), Times.Once);
            Assert.Equal(BuyerLinkageStatus.PendingOther, viewModel.Status);
        }

        [Fact]
        public async Task Accept_CallsServiceAndUpdatesStatus()
        {
            // Arrange
            mockBuyerService
                .Setup(s => s.AcceptLinkageRequest(userBuyer, linkedBuyer))
                .Returns(Task.CompletedTask);
                
            mockCallback
                .Setup(c => c.OnBuyerLinkageUpdated())
                .Returns(Task.CompletedTask);
                
            // Act
            await viewModel.Accept();
            
            // Assert
            mockBuyerService.Verify(s => s.AcceptLinkageRequest(userBuyer, linkedBuyer), Times.Once);
            mockCallback.Verify(c => c.OnBuyerLinkageUpdated(), Times.Once);
            Assert.Equal(BuyerLinkageStatus.Confirmed, viewModel.Status);
        }

        [Fact]
        public async Task Decline_WhenPendingSelf_CallsServiceAndNotifiesCallback()
        {
            // Arrange
            viewModel.Status = BuyerLinkageStatus.PendingSelf;
            
            mockBuyerService
                .Setup(s => s.RefuseLinkageRequest(userBuyer, linkedBuyer))
                .Returns(Task.CompletedTask);
                
            mockCallback
                .Setup(c => c.OnBuyerLinkageUpdated())
                .Returns(Task.CompletedTask);
                
            // Act
            await viewModel.Decline();
            
            // Assert
            mockBuyerService.Verify(s => s.RefuseLinkageRequest(userBuyer, linkedBuyer), Times.Once);
            mockCallback.Verify(c => c.OnBuyerLinkageUpdated(), Times.Once);
            Assert.Equal(BuyerLinkageStatus.Possible, viewModel.Status);
        }

        [Fact]
        public async Task Decline_WhenNotPendingSelf_DoesNotCallService()
        {
            // Arrange
            viewModel.Status = BuyerLinkageStatus.Possible;
            
            // Act
            await viewModel.Decline();
            
            // Assert
            mockBuyerService.Verify(s => s.RefuseLinkageRequest(It.IsAny<Buyer>(), It.IsAny<Buyer>()), Times.Never);
            mockCallback.Verify(c => c.OnBuyerLinkageUpdated(), Times.Never);
            Assert.Equal(BuyerLinkageStatus.Possible, viewModel.Status);
        }

        [Fact]
        public async Task Unsync_WhenConfirmed_BreaksLinkage()
        {
            // Arrange
            viewModel.Status = BuyerLinkageStatus.Confirmed;
            
            mockBuyerService
                .Setup(s => s.BreakLinkage(userBuyer, linkedBuyer))
                .Returns(Task.CompletedTask);
                
            mockCallback
                .Setup(c => c.OnBuyerLinkageUpdated())
                .Returns(Task.CompletedTask);
                
            // Act
            await viewModel.Unsync();
            
            // Assert
            mockBuyerService.Verify(s => s.BreakLinkage(userBuyer, linkedBuyer), Times.Once);
            mockBuyerService.Verify(s => s.CancelLinkageRequest(It.IsAny<Buyer>(), It.IsAny<Buyer>()), Times.Never);
            mockCallback.Verify(c => c.OnBuyerLinkageUpdated(), Times.Once);
            Assert.Equal(BuyerLinkageStatus.Possible, viewModel.Status);
        }

        [Fact]
        public async Task Unsync_WhenPendingOther_CancelsRequest()
        {
            // Arrange
            viewModel.Status = BuyerLinkageStatus.PendingOther;
            
            mockBuyerService
                .Setup(s => s.CancelLinkageRequest(userBuyer, linkedBuyer))
                .Returns(Task.CompletedTask);
                
            mockCallback
                .Setup(c => c.OnBuyerLinkageUpdated())
                .Returns(Task.CompletedTask);
                
            // Act
            await viewModel.Unsync();
            
            // Assert
            mockBuyerService.Verify(s => s.BreakLinkage(It.IsAny<Buyer>(), It.IsAny<Buyer>()), Times.Never);
            mockBuyerService.Verify(s => s.CancelLinkageRequest(userBuyer, linkedBuyer), Times.Once);
            mockCallback.Verify(c => c.OnBuyerLinkageUpdated(), Times.Once);
            Assert.Equal(BuyerLinkageStatus.Possible, viewModel.Status);
        }

        [Fact]
        public async Task Unsync_WhenPossible_OnlyNotifiesCallback()
        {
            // Arrange
            viewModel.Status = BuyerLinkageStatus.Possible;
            
            mockCallback
                .Setup(c => c.OnBuyerLinkageUpdated())
                .Returns(Task.CompletedTask);
                
            // Act
            await viewModel.Unsync();
            
            // Assert
            mockBuyerService.Verify(s => s.BreakLinkage(It.IsAny<Buyer>(), It.IsAny<Buyer>()), Times.Never);
            mockBuyerService.Verify(s => s.CancelLinkageRequest(It.IsAny<Buyer>(), It.IsAny<Buyer>()), Times.Never);
            mockCallback.Verify(c => c.OnBuyerLinkageUpdated(), Times.Once);
            Assert.Equal(BuyerLinkageStatus.Possible, viewModel.Status);
        }

        [Fact]
        public void OnPropertyChanged_RaisesPropertyChangedEvent()
        {
            // Arrange
            var propertyChangedRaised = false;
            var propertyName = string.Empty;
            
            viewModel.PropertyChanged += (sender, args) =>
            {
                propertyChangedRaised = true;
                propertyName = args.PropertyName ?? string.Empty;
            };
            
            // Use reflection to access the protected OnPropertyChanged method
            var method = typeof(BuyerLinkageViewModel).GetMethod(
                "OnPropertyChanged",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            // Act
            method?.Invoke(viewModel, new object[] { "TestProperty" });
            
            // Assert
            Assert.True(propertyChangedRaised);
            Assert.Equal("TestProperty", propertyName);
        }

        [Fact]
        public void OnPropertyChanged_NullHandler_DoesNotThrow()
        {
            // Arrange
            var newViewModel = new BuyerLinkageViewModel
            {
                Service = mockBuyerService.Object,
                UserBuyer = userBuyer,
                LinkedBuyer = linkedBuyer
            };
            
            // PropertyChanged is null by default
            
            // Act & Assert - this should not throw
            newViewModel.Status = BuyerLinkageStatus.Confirmed;
        }

        [Fact]
        public void KeepFirstLetter_MasksNameCorrectly()
        {
            // Arrange
            var testLinkedBuyer = new Buyer
            {
                User = new User { UserId = 3 },
                FirstName = "John",
                LastName = "Test"
            };
            
            var testViewModel = new BuyerLinkageViewModel
            {
                Service = mockBuyerService.Object,
                UserBuyer = userBuyer,
                LinkedBuyer = testLinkedBuyer,
                Status = BuyerLinkageStatus.Possible
            };
            
            // Act - will trigger UpdateDisplayName which calls KeepFirstLetter
            var displayName = testViewModel.DisplayName;
            
            // Assert
            Assert.StartsWith("J", displayName);
            Assert.Contains("*", displayName);
        }
        
        [Fact]
        public void KeepFirstLetter_WithSingleCharName_HandlesCorrectly()
        {
            // Arrange
            var testLinkedBuyer = new Buyer
            {
                User = new User { UserId = 3 },
                FirstName = "A",
                LastName = "Test"
            };
            
            var testViewModel = new BuyerLinkageViewModel
            {
                Service = mockBuyerService.Object,
                UserBuyer = userBuyer,
                LinkedBuyer = testLinkedBuyer,
                Status = BuyerLinkageStatus.Possible
            };
            
            // Act - will trigger UpdateDisplayName which calls KeepFirstLetter
            var displayName = testViewModel.DisplayName;
            
            // Assert
            Assert.Contains("A", displayName);
        }
        
    }
}
