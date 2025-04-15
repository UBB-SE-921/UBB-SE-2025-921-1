using System.Threading.Tasks;
using MarketPlace924.Service;
using MarketPlace924.Repository;
using Moq;
using Xunit;

namespace XUnitTestProject.ServiceTests
{
    public class AnalyticsServiceTests
    {
        private readonly Mock<IUserRepository> mockUserRepository;
        private readonly Mock<IBuyerRepository> mockBuyerRepository;
        private readonly AnalyticsService analyticsService;

        public AnalyticsServiceTests()
        {
            mockUserRepository = new Mock<IUserRepository>();
            mockBuyerRepository = new Mock<IBuyerRepository>();
            analyticsService = new AnalyticsService(mockUserRepository.Object, mockBuyerRepository.Object);
        }

        [Fact]
        public async Task GetTotalNumberOfUsers_ShouldReturnCorrectCount()
        {
            // Arrange
            const int expectedUserCount = 42;
            mockUserRepository.Setup(repository => repository.GetTotalNumberOfUsers())
                .ReturnsAsync(expectedUserCount);

            // Act
            var numberOfUsers = await analyticsService.GetTotalNumberOfUsers();

            // Assert
            Assert.Equal(expectedUserCount, numberOfUsers);
            mockUserRepository.Verify(repository => repository.GetTotalNumberOfUsers(), Times.Once);
        }

        [Fact]
        public async Task GetTotalNumberOfBuyers_ShouldReturnCorrectCount()
        {
            // Arrange
            const int expectedBuyerCount = 15;
            mockBuyerRepository.Setup(repository => repository.GetTotalCount())
                .ReturnsAsync(expectedBuyerCount);

            // Act
            var numberOfBuyers = await analyticsService.GetTotalNumberOfBuyers();

            // Assert
            Assert.Equal(expectedBuyerCount, numberOfBuyers);
            mockBuyerRepository.Verify(repository => repository.GetTotalCount(), Times.Once);
        }

        [Fact]
        public async Task GetTotalNumberOfUsers_WhenRepositoryThrows_ShouldPropagateException()
        {
            // Arrange
            mockUserRepository.Setup(repository => repository.GetTotalNumberOfUsers())
                .ThrowsAsync(new System.Exception());

            // Act & Assert
            await Assert.ThrowsAsync<System.Exception>(() => analyticsService.GetTotalNumberOfUsers());
        }

        [Fact]
        public async Task GetTotalNumberOfBuyers_WhenRepositoryThrows_ShouldPropagateException()
        {
            // Arrange
            mockBuyerRepository.Setup(repository => repository.GetTotalCount())
                .ThrowsAsync(new System.Exception());

            // Act & Assert
            await Assert.ThrowsAsync<System.Exception>(() => analyticsService.GetTotalNumberOfBuyers());
        }

        [Fact]
        public async Task GetTotalNumberOfUsers_WhenRepositoryReturnsZero_ShouldReturnZero()
        {
            // Arrange
            const int expectedUserCount = 0;
            mockUserRepository.Setup(repository => repository.GetTotalNumberOfUsers())
                .ReturnsAsync(expectedUserCount);

            // Act
            var numberOfUsers = await analyticsService.GetTotalNumberOfUsers();

            // Assert
            Assert.Equal(expectedUserCount, numberOfUsers);
            mockUserRepository.Verify(repository => repository.GetTotalNumberOfUsers(), Times.Once);
        }

        [Fact]
        public async Task GetTotalNumberOfBuyers_WhenRepositoryReturnsZero_ShouldReturnZero()
        {
            // Arrange
            const int expectedBuyerCount = 0;
            mockBuyerRepository.Setup(repository => repository.GetTotalCount())
                .ReturnsAsync(expectedBuyerCount);

            // Act
            var numberOfBuyers = await analyticsService.GetTotalNumberOfBuyers();

            // Assert
            Assert.Equal(expectedBuyerCount, numberOfBuyers);
            mockBuyerRepository.Verify(repository => repository.GetTotalCount(), Times.Once);
        }
    }
} 