using System;
using System.Threading.Tasks;
using SharedClassLibrary.Domain;
using SharedClassLibrary.Helper;
using SharedClassLibrary.Service;
using MarketPlace924.ViewModel.Admin;
using Moq;
using Xunit;

namespace XUnitTestProject.ViewModelTests
{
    public class UserRowViewModelTests
    {
        private readonly Mock<IAdminService> adminServiceMock;
        private readonly Mock<IAdminViewModel> adminViewModelMock;
        private readonly User testUser;
        private readonly UserRowViewModel viewModel;

        public UserRowViewModelTests()
        {
            // Setup mocks
            this.adminServiceMock = new Mock<IAdminService>();
            this.adminViewModelMock = new Mock<IAdminViewModel>();
            
            // Setup test data
            this.testUser = new User
            {
                UserId = 1,
                Username = "testuser",
                Email = "test@example.com",
                Role = UserRole.Buyer,
                FailedLogins = 0,
                BannedUntil = null,
                IsBanned = false
            };
            
            // Create view model
            this.viewModel = new UserRowViewModel(
                this.testUser,
                this.adminServiceMock.Object,
                this.adminViewModelMock.Object);
        }

        [Fact]
        public void Constructor_InitializesPropertiesCorrectly()
        {
            // Assert
            Assert.Equal(this.testUser, this.viewModel.User);
            Assert.Equal(this.testUser.UserId, this.viewModel.UserId);
            Assert.Equal(this.testUser.Username, this.viewModel.Username);
            Assert.Equal(this.testUser.Email, this.viewModel.Email);
            Assert.Equal(this.testUser.Role.ToString(), this.viewModel.Role);
            Assert.Equal(this.testUser.FailedLogins, this.viewModel.FailedLogins);
            Assert.Equal(this.testUser.BannedUntil, this.viewModel.BannedUntil);
            Assert.Equal(this.testUser.IsBanned, this.viewModel.IsBanned);
            Assert.NotNull(this.viewModel.BanUserCommand);
            Assert.NotNull(this.viewModel.SetAdminCommand);
        }

        [Fact]
        public void BanUserCommand_WhenExecuted_CallsToggleUserBanStatus()
        {
            // Arrange
            this.adminServiceMock.Setup(x => x.ToggleUserBanStatus(this.testUser))
                .Returns(Task.FromResult(true));
                
            // Act
            this.viewModel.BanUserCommand.Execute(null);
            
            // Assert
            this.adminServiceMock.Verify(x => x.ToggleUserBanStatus(this.testUser), Times.Once);
            this.adminViewModelMock.Verify(x => x.RefreshUsers(), Times.Once);
        }
        
        [Fact]
        public void SetAdminCommand_WhenExecuted_CallsSetUserAdmin()
        {
            // Arrange
            this.adminServiceMock.Setup(x => x.SetUserAdmin(this.testUser))
                .Returns(Task.FromResult(true));
                
            // Act
            this.viewModel.SetAdminCommand.Execute(null);
            
            // Assert
            this.adminServiceMock.Verify(x => x.SetUserAdmin(this.testUser), Times.Once);
            this.adminViewModelMock.Verify(x => x.RefreshUsers(), Times.Once);
        }
        
        [Fact]
        public void UserProperties_AreCorrectlyExposed()
        {
            // Arrange
            var bannedUser = new User
            {
                UserId = 2,
                Username = "banneduser",
                Email = "banned@example.com",
                Role = UserRole.Seller,
                FailedLogins = 3,
                BannedUntil = DateTime.Now.AddDays(1),
                IsBanned = true
            };
            
            // Act
            var bannedViewModel = new UserRowViewModel(
                bannedUser,
                this.adminServiceMock.Object,
                this.adminViewModelMock.Object);
            
            // Assert
            Assert.Equal(bannedUser.UserId, bannedViewModel.UserId);
            Assert.Equal(bannedUser.Username, bannedViewModel.Username);
            Assert.Equal(bannedUser.Email, bannedViewModel.Email);
            Assert.Equal(bannedUser.Role.ToString(), bannedViewModel.Role);
            Assert.Equal(bannedUser.FailedLogins, bannedViewModel.FailedLogins);
            Assert.Equal(bannedUser.BannedUntil, bannedViewModel.BannedUntil);
            Assert.Equal(bannedUser.IsBanned, bannedViewModel.IsBanned);
        }
    }
}
