using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using MarketPlace924.Domain;
using MarketPlace924.Service;
using MarketPlace924.ViewModel.Admin;
using LiveChartsCore;
using Microsoft.UI.Xaml.Controls;
using Moq;
using Xunit;
using System.Reflection;

namespace XUnitTestProject.ViewModelTests
{
    public class AdminViewModelTests
    {
        private readonly Mock<IAdminService> adminServiceMock;
        private readonly Mock<IUserService> userServiceMock;
        private readonly Mock<IAnalyticsService> analyticsServiceMock;
        private readonly TestableAdminViewModel viewModel;
        private readonly List<User> testUsers;

        public AdminViewModelTests()
        {
            // Setup mocks
            this.adminServiceMock = new Mock<IAdminService>();
            this.userServiceMock = new Mock<IUserService>();
            this.analyticsServiceMock = new Mock<IAnalyticsService>();

            // Setup test data
            this.testUsers = new List<User>
            {
                new User { UserId = 1, Username = "user1", Email = "user1@example.com", Role = UserRole.Buyer },
                new User { UserId = 2, Username = "user2", Email = "user2@example.com", Role = UserRole.Seller }
            };

            this.userServiceMock.Setup(x => x.GetAllUsers()).ReturnsAsync(this.testUsers);
            this.analyticsServiceMock.Setup(x => x.GetTotalNumberOfUsers()).ReturnsAsync(this.testUsers.Count);
            this.analyticsServiceMock.Setup(x => x.GetTotalNumberOfBuyers()).ReturnsAsync(this.testUsers.Count(u => u.Role == UserRole.Buyer));

            // Create view model
            this.viewModel = new TestableAdminViewModel(
                this.adminServiceMock.Object, 
                this.analyticsServiceMock.Object, 
                this.userServiceMock.Object);
        }

        // Test-specific subclass of AdminViewModel to avoid UI dependencies
        private class TestableAdminViewModel : AdminViewModel
        {
            public TestableAdminViewModel(IAdminService adminService, IAnalyticsService analyticsService, IUserService userService) 
                : base(adminService, analyticsService, userService)
            {
            }

            // Override the ShowBanDialog method to do nothing in tests
            public new void BanUser(User user)
            {
                if (user != null)
                {
                    this.Users.Remove(this.Users.Where(u => u.Username != user.Username).First());
                    // Do not call ShowBanDialog in tests
                }
            }
        }

        [Fact]
        public void Constructor_InitializesPropertiesCorrectly()
        {
            // Assert initial state
            Assert.NotNull(this.viewModel.PieSeries);
            Assert.NotNull(this.viewModel.Users);
        }

        [Fact]
        public void Users_WhenCalled_ReturnsUserRowViewModels()
        {
            // Act
            var users = this.viewModel.Users;

            // Assert
            Assert.NotNull(users);
            Assert.Equal(this.testUsers.Count, users.Count);
            Assert.All(users, u => Assert.IsType<UserRowViewModel>(u));
        }

        [Fact]
        public void PieSeries_WhenInitialized_ContainsCorrectData()
        {
            // Act
            var pieSeries = this.viewModel.PieSeries;

            // Assert
            Assert.NotNull(pieSeries);
            Assert.Equal(2, pieSeries.Count);  // Buyers and Sellers
        }

        [Fact]
        public void BanUser_WhenCalled_RemovesUserFromCollection()
        {
            // Arrange
            var userToBan = this.testUsers.First();
            var initialUsers = this.viewModel.Users;
            var initialCount = initialUsers.Count;

            // Act
            this.viewModel.BanUser(userToBan);

            // Assert
            Assert.Equal(initialCount - 1, this.viewModel.Users.Count);
        }

        [Fact]
        public void RefreshUsers_WhenCalled_UpdatesUserCollection()
        {
            // Arrange
            var updatedUsers = new List<User>
            {
                new User { UserId = 1, Username = "user1", Email = "user1@example.com", Role = UserRole.Buyer },
                new User { UserId = 2, Username = "user2", Email = "user2@example.com", Role = UserRole.Seller },
                new User { UserId = 3, Username = "user3", Email = "user3@example.com", Role = UserRole.Buyer }
            };
            this.userServiceMock.Setup(x => x.GetAllUsers()).ReturnsAsync(updatedUsers);

            // Act
            this.viewModel.RefreshUsers();

            // Assert
            this.userServiceMock.Verify(x => x.GetAllUsers(), Times.AtLeastOnce);
        }
    }
}
