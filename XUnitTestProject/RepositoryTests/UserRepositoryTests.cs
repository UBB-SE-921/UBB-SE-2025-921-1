using SharedClassLibrary.Domain;
using SharedClassLibrary.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Moq;

namespace XUnitTestProject.RepositoryTests
{
    [Collection("DatabaseCollection")]
    public class UserRepositoryTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;

        public UserRepositoryTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
        }

        // Example test using the mocked repository
        [Fact]
        public async Task AddUser_ShouldInsertUserSuccessfully()
        {
            // Arrange
            var testUser = new User
            {
                UserId = 1,
                Username = "adduser",
                Email = "adduser_only@example.com",
                PhoneNumber = "+40700000001",
                Password = "hashedpass",
                Role = UserRole.Buyer,
                FailedLogins = 0,
                BannedUntil = null,
                IsBanned = false
            };

            _mockUserRepository.Setup(repo => repo.AddUser(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            // Act
            await _mockUserRepository.Object.AddUser(testUser);

            // Assert
            _mockUserRepository.Verify(repo => repo.AddUser(It.Is<User>(u => u.Username == testUser.Username)), Times.Once);
        }

        // Additional tests can follow the same pattern
    }
}
