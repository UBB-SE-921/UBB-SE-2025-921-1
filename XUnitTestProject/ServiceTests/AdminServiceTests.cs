
namespace XUnitTestProject.ServiceTests
{
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.IRepository;
    using MarketPlace924.Service;
    using Moq;
    using Xunit;
    public class AdminServiceTests
    {
        private readonly Mock<IUserRepository> mockUserRepository;
        private readonly AdminService adminService;

        public AdminServiceTests()
        {
            mockUserRepository = new Mock<IUserRepository>();
            adminService = new AdminService(mockUserRepository.Object);
        }

        [Fact]
        public async Task ToggleUserBanStatus_UserNotBanned_BansUserAndUpdatesRepository()
        {
            // Arrange
            var user = new User { IsBanned = false };

            // Act
            await adminService.ToggleUserBanStatus(user);

            // Assert
            Assert.True(user.IsBanned);
            Assert.NotNull(user.BannedUntil);
            mockUserRepository.Verify(repository => repository.UpdateUser(user), Times.Once);
        }

        [Fact]
        public async Task ToggleUserBanStatus_UserIsBanned_UnbansUserAndUpdatesRepository()
        {
            // Arrange
            var user = new User { IsBanned = true, BannedUntil = DateTime.Now.AddYears(5) };

            // Act
            await adminService.ToggleUserBanStatus(user);

            // Assert
            Assert.False(user.IsBanned);
            Assert.Null(user.BannedUntil);
            mockUserRepository.Verify(repository => repository.UpdateUser(user), Times.Once);
        }

        [Fact]
        public async Task SetUserAdmin_WhenCalled_SetsUserRoleToAdminAndUpdatesRepository()
        {
            // Arrange
            var user = new User { Role = UserRole.Unassigned };

            // Act
            await adminService.SetUserAdmin(user);

            // Assert
            Assert.Equal(UserRole.Admin, user.Role);
            mockUserRepository.Verify(repository => repository.UpdateUser(user), Times.Once);
        }
    }
}
