using SharedClassLibrary.Domain;
using SharedClassLibrary.IRepository;
using MarketPlace924.Service;
using Moq;

namespace XUnitTestProject.ServiceTests
{
    public class UserServiceTests
    {
        private readonly UserService userService;

        public UserServiceTests()
        {
            var userRepositoryMock = new Mock<IUserRepository>();
            userService = new UserService(userRepositoryMock.Object);
        }

        [Fact]
        public void HashPassword_WithValidInput_ReturnsConsistentHashedValue()
        {
            var password = "TestPassword123!";

            var hash1 = userService.HashPassword(password);
            var hash2 = userService.HashPassword(password);

            Assert.False(string.IsNullOrEmpty(hash1));
            Assert.Equal(hash1, hash2); // same input = same hash
            Assert.NotEqual(password, hash1); // hash should be different than input
        }

        [Theory]
        [InlineData("ABC123", "ABC123", true)]
        [InlineData("abc123", "ABC123", false)]
        [InlineData("abc", "abcd", false)]
        [InlineData("", "", true)]
        [InlineData("123456", "", false)]
        public void VerifyCaptcha_ReturnsExpectedResult(string entered, string generated, bool expected)
        {
            var result = userService.VerifyCaptcha(entered, generated);

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("user@example.com", true)]
        [InlineData("user.name+tag@example.co.uk", true)]
        [InlineData("user@", false)]
        [InlineData("@example.com", false)]
        [InlineData("userexample.com", false)]
        [InlineData("", false)]
        public void IsEmailValidForLogin_ReturnsExpectedResult(string email, bool expected)
        {
            var result = userService.IsEmailValidForLogin(email);

            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task RegisterUser_WithValidData_AddsUserAndReturnsTrue()
        {
            var mockRepository = new Mock<IUserRepository>();
            var service = new UserService(mockRepository.Object);

            string username = "testuser";
            string password = "Parola123!";
            string email = "test@example.com";
            string phone = "+40712345678";
            int role = (int)UserRole.Buyer;

            mockRepository.Setup(repository => repository.UsernameExists(username)).ReturnsAsync(false);
            mockRepository.Setup(repository => repository.EmailExists(email)).ReturnsAsync(false);
            mockRepository.Setup(repository => repository.AddUser(It.IsAny<User>())).Returns(Task.CompletedTask);

            var result = await service.RegisterUser(username, password, email, phone, role);

            Assert.True(result);
            mockRepository.Verify(repository => repository.AddUser(It.Is<User>(user =>
                user.Username == username &&
                user.Email == email &&
                user.PhoneNumber == phone &&
                user.Role == UserRole.Buyer &&
                user.Password != password // Password should be hashed
            )), Times.Once);
        }

        [Fact]
        public async Task RegisterUser_InvalidUsername_ThrowsException()
        {
            var mockRepository = new Mock<IUserRepository>();
            var service = new UserService(mockRepository.Object);
            var username = "abc";

            var exception = await Assert.ThrowsAsync<Exception>(() =>
                service.RegisterUser(username, "Parola123!", "user@test.com", "+40712345678", (int)UserRole.Buyer));

            Assert.Equal("Username must be at least 4 characters long.", exception.Message);
        }

        [Fact]
        public async Task RegisterUser_InvalidEmail_ThrowsException()
        {
            var mockRepository = new Mock<IUserRepository>();
            var service = new UserService(mockRepository.Object);
            var email = "bad-email";

            var exception = await Assert.ThrowsAsync<Exception>(() =>
                service.RegisterUser("validuser", "Parola123!", email, "+40712345678", (int)UserRole.Buyer));

            Assert.Equal("Invalid email address format.", exception.Message);
        }

        [Fact]
        public async Task RegisterUser_InvalidPassword_ThrowsException()
        {
            var mockRepository = new Mock<IUserRepository>();
            var service = new UserService(mockRepository.Object);
            var password = "123";

            var exception = await Assert.ThrowsAsync<Exception>(() =>
                service.RegisterUser("validuser", password, "user@test.com", "+40712345678", (int)UserRole.Buyer));

            Assert.Contains("password must be at least 8 characters", exception.Message);
        }

        [Fact]
        public async Task RegisterUser_InvalidPhoneNumber_ThrowsException()
        {
            var mockRepository = new Mock<IUserRepository>();
            var service = new UserService(mockRepository.Object);
            var phoneNumber = "075123456";

            var exception = await Assert.ThrowsAsync<Exception>(() =>
                service.RegisterUser("validuser", "Parola123!", "user@test.com", phoneNumber, (int)UserRole.Buyer));

            Assert.Contains("should start with +40", exception.Message);
        }

        [Fact]
        public async Task RegisterUser_InvalidRole_ThrowsException()
        {
            var mockRepository = new Mock<IUserRepository>();
            var service = new UserService(mockRepository.Object);
            var role = 99;

            var exception = await Assert.ThrowsAsync<Exception>(() =>
                service.RegisterUser("validuser", "Parola123!", "user@test.com", "+40712345678", role));

            Assert.Equal("Please select an account type (Buyer or Seller).", exception.Message);
        }

        [Fact]
        public async Task RegisterUser_UsernameExists_ThrowsException()
        {
            var mockRepository = new Mock<IUserRepository>();
            mockRepository.Setup(repository => repository.UsernameExists("takenuser")).ReturnsAsync(true);
            var service = new UserService(mockRepository.Object);
            var username = "takenuser";

            var exception = await Assert.ThrowsAsync<Exception>(() =>
                service.RegisterUser(username, "Parola123!", "user@test.com", "+40712345678", (int)UserRole.Buyer));

            Assert.Equal("Username already exists.", exception.Message);
        }

        [Fact]
        public async Task RegisterUser_EmailExists_ThrowsException()
        {
            var mockRepository = new Mock<IUserRepository>();
            var username = "uniqueuser";
            var email = "used@test.com";
            mockRepository.Setup(repository => repository.UsernameExists(username)).ReturnsAsync(false);
            mockRepository.Setup(repository => repository.EmailExists(email)).ReturnsAsync(true);

            var service = new UserService(mockRepository.Object);

            var exception = await Assert.ThrowsAsync<Exception>(() =>
                service.RegisterUser(username, "Parola123!", email, "+40712345678", (int)UserRole.Buyer));

            Assert.Equal("Email is already in use.", exception.Message);
        }

        [Fact]
        public async Task CanUserLogin_ValidHashedPassword_ReturnsTrue()
        {
            var mockRepository = new Mock<IUserRepository>();
            var service = new UserService(mockRepository.Object);

            string email = "user@example.com";
            string password = "Parola123!";
            string hashed = service.HashPassword(password);

            var user = new User { Email = email, Password = hashed };

            mockRepository.Setup(repository => repository.EmailExists(email)).ReturnsAsync(true);
            mockRepository.Setup(repository => repository.GetUserByEmail(email)).ReturnsAsync(user);

            var result = await service.CanUserLogin(email, password);

            Assert.True(result);
        }

        [Fact]
        public async Task CanUserLogin_PlainTextPassword_ReturnsTrue()
        {
            var mockRepository = new Mock<IUserRepository>();
            var service = new UserService(mockRepository.Object);

            string email = "plain@example.com";
            string password = "1234";

            var user = new User { Email = email, Password = $"plain:{password}" };

            mockRepository.Setup(repository => repository.EmailExists(email)).ReturnsAsync(true);
            mockRepository.Setup(repository => repository.GetUserByEmail(email)).ReturnsAsync(user);

            var result = await service.CanUserLogin(email, password);

            Assert.True(result);
        }

        [Fact]
        public async Task CanUserLogin_WrongPassword_ReturnsFalse()
        {
            var mockRepository = new Mock<IUserRepository>();
            var service = new UserService(mockRepository.Object);

            string email = "user@example.com";
            string password = "wrongPassword";
            var user = new User { Email = email, Password = service.HashPassword("CorrectPassword123!") };

            mockRepository.Setup(repository => repository.EmailExists(email)).ReturnsAsync(true);
            mockRepository.Setup(repository => repository.GetUserByEmail(email)).ReturnsAsync(user);

            var result = await service.CanUserLogin(email, password);

            Assert.False(result);
        }

        [Fact]
        public async Task CanUserLogin_UserIsNull_ReturnsFalse()
        {
            var mockRepository = new Mock<IUserRepository>();
            var service = new UserService(mockRepository.Object);

            string email = "missing@example.com";
            string password = "somepassword";

            mockRepository.Setup(repository => repository.EmailExists(email)).ReturnsAsync(true);
            mockRepository.Setup(repository => repository.GetUserByEmail(email)).ReturnsAsync((User?)null);

            var result = await service.CanUserLogin(email, password);

            Assert.False(result);
        }

        [Fact]
        public async Task CanUserLogin_EmailDoesNotExist_ReturnsFalse()
        {
            var mockRepository = new Mock<IUserRepository>();
            var service = new UserService(mockRepository.Object);

            mockRepository.Setup(repository => repository.EmailExists(It.IsAny<string>())).ReturnsAsync(false);

            var result = await service.CanUserLogin("ghost@example.com", "password");

            Assert.False(result);
        }

        [Fact]
        public async Task UpdateUserFailedLoginsCount_DelegatesToRepository()
        {
            var user = new User
            {
                UserId = 1,
                Username = "failuser",
                Email = "fail@example.com",
                PhoneNumber = "+40712345678",
                Password = "secure",
                Role = UserRole.Buyer,
                FailedLogins = 0
            };

            var mockRepository = new Mock<IUserRepository>();
            var service = new UserService(mockRepository.Object);

            int newCount = 4;

            await service.UpdateUserFailedLoginsCount(user, newCount);

            mockRepository.Verify(repository => repository.UpdateUserFailedLoginsCount(user, newCount), Times.Once);
        }

        [Fact]
        public async Task GetUserByEmail_ReturnsUser_WhenFound()
        {
            var email = "user@example.com";
            var expectedUser = new User
            {
                UserId = 1,
                Username = "testuser",
                Email = email,
                PhoneNumber = "+40712345678",
                Password = "pass",
                Role = UserRole.Buyer,
                FailedLogins = 1,
                IsBanned = false
            };

            var mockRepository = new Mock<IUserRepository>();
            mockRepository.Setup(repository => repository.GetUserByEmail(email)).ReturnsAsync(expectedUser);
            var service = new UserService(mockRepository.Object);

            var result = await service.GetUserByEmail(email);

            Assert.NotNull(result);
            Assert.Equal(expectedUser.Username, result!.Username);
            Assert.Equal(expectedUser.Email, result.Email);
        }

        [Fact]
        public async Task GetUserByEmail_ReturnsNull_WhenNotFound()
        {
            var mockRepository = new Mock<IUserRepository>();
            mockRepository.Setup(repository => repository.GetUserByEmail(It.IsAny<string>())).ReturnsAsync((User?)null);
            var service = new UserService(mockRepository.Object);

            var result = await service.GetUserByEmail("missing@example.com");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetFailedLoginsCountByEmail_ValidUser_ReturnsCorrectCount()
        {
            var mockRepository = new Mock<IUserRepository>();
            var email = "test@example.com";
            var userId = 42;
            var user = new User { UserId = userId, Email = email };
            var numberOfFailedLogins = 3;

            mockRepository.Setup(repository => repository.GetUserByEmail(email)).ReturnsAsync(user);
            mockRepository.Setup(repository => repository.GetFailedLoginsCountByUserId(userId)).ReturnsAsync(numberOfFailedLogins);

            var service = new UserService(mockRepository.Object);

            var result = await service.GetFailedLoginsCountByEmail(email);

            Assert.Equal(numberOfFailedLogins, result);
        }

        [Fact]
        public async Task GetFailedLoginsCountByEmail_UserNotFound_ThrowsArgumentNullException()
        {
            var mockRepository = new Mock<IUserRepository>();
            var email = "notfound@example.com";

            mockRepository.Setup(r => r.GetUserByEmail(email)).ReturnsAsync((User?)null);

            var service = new UserService(mockRepository.Object);

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
                service.GetFailedLoginsCountByEmail(email));

            Assert.Contains(email, exception.Message);
        }

        [Fact]
        public async Task IsUser_ReturnsTrue_WhenEmailExists()
        {
            var email = "exists@example.com";
            var mockRepository = new Mock<IUserRepository>();
            mockRepository.Setup(repository => repository.EmailExists(email)).ReturnsAsync(true);
            var service = new UserService(mockRepository.Object);

            var result = await service.IsUser(email);

            Assert.True(result);
        }

        [Fact]
        public async Task IsUser_ReturnsFalse_WhenEmailDoesNotExist()
        {
            var email = "missing@example.com";
            var mockRepository = new Mock<IUserRepository>();
            mockRepository.Setup(repository => repository.EmailExists(email)).ReturnsAsync(false);
            var service = new UserService(mockRepository.Object);

            var result = await service.IsUser(email);

            Assert.False(result);
        }

        [Theory]
        [InlineData(5, true)]
        [InlineData(-5, false)]
        [InlineData(null, false)]
        public async Task IsUserSuspended_CoversAllBranches(int? offsetMinutes, bool expected)
        {
            var mockRepository = new Mock<IUserRepository>();
            var service = new UserService(mockRepository.Object);
            var email = "user@example.com";

            DateTime? bannedUntil = offsetMinutes.HasValue ? DateTime.Now.AddMinutes(offsetMinutes.Value) : null;

            var user = new User { Email = email, BannedUntil = bannedUntil };
            mockRepository.Setup(repository => repository.GetUserByEmail(It.IsAny<string>())).ReturnsAsync(user);

            var result = await service.IsUserSuspended(email);

            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task IsUserSuspended_UserIsNull_ThrowsArgumentNullException()
        {
            var mockRepository = new Mock<IUserRepository>();
            var service = new UserService(mockRepository.Object);

            string email = "ghost@example.com";

            mockRepository.Setup(repository => repository.GetUserByEmail(email)).ReturnsAsync((User?)null);

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
                service.IsUserSuspended(email));

            Assert.Contains(email, exception.Message);
        }

        [Fact]
        public async Task SuspendUserForSeconds_ValidUser_UpdatesBannedUntil()
        {
            var mockRepository = new Mock<IUserRepository>();
            var service = new UserService(mockRepository.Object);

            var email = "test@example.com";
            var user = new User { Email = email };

            mockRepository.Setup(repository => repository.GetUserByEmail(email)).ReturnsAsync(user);
            mockRepository.Setup(repository => repository.UpdateUser(It.IsAny<User>())).Returns(Task.CompletedTask);

            int suspensionDurationSeconds = 10;
            var before = DateTime.Now;
            var numberOfSecondsInTheFutureToBeAdded = 9;

            await service.SuspendUserForSeconds(email, suspensionDurationSeconds);

            Assert.True(user.BannedUntil.HasValue);
            Assert.True(user.BannedUntil.Value > before.AddSeconds(numberOfSecondsInTheFutureToBeAdded));
            mockRepository.Verify(repository => repository.UpdateUser(user), Times.Once);
        }

        [Fact]
        public async Task SuspendUserForSeconds_UserNotFound_ThrowsException()
        {
            var mockRepository = new Mock<IUserRepository>();
            var service = new UserService(mockRepository.Object);

            string email = "ghost@example.com";
            mockRepository.Setup(repository => repository.GetUserByEmail(email)).ReturnsAsync((User?)null);

            int suspensionDurationSeconds = 10;
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
                service.SuspendUserForSeconds(email, suspensionDurationSeconds));

            Assert.Contains(email, exception.Message);
        }

        [Fact]
        public async Task ValidateLoginCredentials_EmptyFields_ReturnsFillAllFields()
        {
            var mockRepository = new Mock<IUserRepository>();
            var service = new UserService(mockRepository.Object);

            var result = await service.ValidateLoginCredentials("", "", "", "anything");

            Assert.Equal("Please fill in all fields.", result);
        }

        [Fact]
        public async Task ValidateLoginCredentials_WrongCaptcha_ReturnsCaptchaFailed()
        {
            var mockRepository = new Mock<IUserRepository>();
            var service = new UserService(mockRepository.Object);

            var result = await service.ValidateLoginCredentials("test@example.com", "Parola123!", "abc", "def");

            Assert.Equal("Captcha verification failed.", result);
        }

        [Fact]
        public async Task ValidateLoginCredentials_EmailDoesNotExist_ReturnsEmailDoesNotExist()
        {
            var mockRepository = new Mock<IUserRepository>();
            mockRepository.Setup(repository => repository.EmailExists("noone@example.com")).ReturnsAsync(false);

            var service = new UserService(mockRepository.Object);

            var result = await service.ValidateLoginCredentials("noone@example.com", "Parola123!", "captcha", "captcha");

            Assert.Equal("Email does not exist.", result);
        }

        [Fact]
        public async Task ValidateLoginCredentials_InvalidPassword_ReturnsLoginFailed()
        {
            var email = "user@example.com";
            var user = new User { Email = email, Password = "plain:WrongPassword" };

            var mockRepository = new Mock<IUserRepository>();
            mockRepository.Setup(repository => repository.EmailExists(email)).ReturnsAsync(true);
            mockRepository.Setup(repository => repository.GetUserByEmail(email)).ReturnsAsync(user);

            var service = new UserService(mockRepository.Object);

            var result = await service.ValidateLoginCredentials(email, "doesNotMatch", "captcha", "captcha");

            Assert.Equal("Login failed", result);
        }

        [Fact]
        public async Task ValidateLoginCredentials_ValidInputs_ReturnsSuccess()
        {
            var email = "success@example.com";
            var password = "Parola123!";
            var hashed = new UserService(new Mock<IUserRepository>().Object).HashPassword(password);

            var user = new User { Email = email, Password = hashed };

            var mockRepository = new Mock<IUserRepository>();
            mockRepository.Setup(repository => repository.EmailExists(email)).ReturnsAsync(true);
            mockRepository.Setup(repository => repository.GetUserByEmail(email)).ReturnsAsync(user);

            var service = new UserService(mockRepository.Object);

            var result = await service.ValidateLoginCredentials(email, password, "captcha", "captcha");

            Assert.Equal("Success", result);
        }

        [Fact]
        public async Task ValidateLoginCredentials_UserSuspended_BannedUntilInFuture_ReturnsWaitMessage()
        {
            var email = "suspended@example.com";
            var numberOfSecondsInTheFutureToBeAdded = 30;
            var bannedUntil = DateTime.Now.AddSeconds(numberOfSecondsInTheFutureToBeAdded);
            var user = new User { Email = email, BannedUntil = bannedUntil };

            var mockRepository = new Mock<IUserRepository>();
            mockRepository.Setup(repository => repository.EmailExists(email)).ReturnsAsync(true);
            mockRepository.Setup(repository => repository.GetUserByEmail(email)).ReturnsAsync(user);

            var service = new UserService(mockRepository.Object);

            var result = await service.ValidateLoginCredentials(email, "password", "captcha", "captcha");

            Assert.Contains("Too many failed attempts", result);
            Assert.Contains("Try again in", result);
        }

        [Fact]
        public async Task HandleFailedLogin_UserIsNull_DoesNothing()
        {
            var email = "ghost@example.com";
            var mockRepository = new Mock<IUserRepository>();
            mockRepository.Setup(repository => repository.GetUserByEmail(email)).ReturnsAsync((User?)null);

            var service = new UserService(mockRepository.Object);

            await service.HandleFailedLogin(email);

            // Since user is null, no further repo methods should be called
            mockRepository.Verify(repository => repository.UpdateUserFailedLoginsCount(It.IsAny<User>(), It.IsAny<int>()), Times.Never);
            mockRepository.Verify(repository => repository.UpdateUser(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task HandleFailedLogin_IncrementsFailedLoginCount()
        {
            var email = "user@example.com";
            var failedLoginsCount = 2;
            var newFailedLoginsCount = failedLoginsCount + 1;
            var userId = 1;
            var user = new User { Email = email, UserId = userId };

            var mockRepository = new Mock<IUserRepository>();
            mockRepository.Setup(repository => repository.GetUserByEmail(email)).ReturnsAsync(user);
            mockRepository.Setup(repository => repository.GetFailedLoginsCountByUserId(userId)).ReturnsAsync(failedLoginsCount);
            mockRepository.Setup(repository => repository.UpdateUserFailedLoginsCount(user, newFailedLoginsCount)).Returns(Task.CompletedTask);

            var service = new UserService(mockRepository.Object);

            await service.HandleFailedLogin(email);

            mockRepository.Verify(repository => repository.UpdateUserFailedLoginsCount(user, newFailedLoginsCount), Times.Once);
            mockRepository.Verify(repository => repository.UpdateUser(It.IsAny<User>()), Times.Never); // no suspension
        }

        [Fact]
        public async Task HandleFailedLogin_ExceedsMaxAttempts_SuspendsUser()
        {
            var email = "lockout@example.com";
            var userId = 1;
            var user = new User { Email = email, UserId = userId };
            var failedLoginsCount = 5;
            var newFailedLoginsCount = failedLoginsCount + 1;

            var mockRepository = new Mock<IUserRepository>();
            mockRepository.Setup(repository => repository.GetUserByEmail(email)).ReturnsAsync(user);
            mockRepository.Setup(repository => repository.GetFailedLoginsCountByUserId(1)).ReturnsAsync(failedLoginsCount); // Already at threshold
            mockRepository.Setup(repository => repository.UpdateUserFailedLoginsCount(user, newFailedLoginsCount)).Returns(Task.CompletedTask);
            mockRepository.Setup(repository => repository.UpdateUser(It.IsAny<User>())).Returns(Task.CompletedTask);

            var service = new UserService(mockRepository.Object);

            await service.HandleFailedLogin(email);

            mockRepository.Verify(repository => repository.UpdateUserFailedLoginsCount(user, newFailedLoginsCount), Times.Once);
            mockRepository.Verify(repository => repository.UpdateUser(user), Times.Once); // Suspended
            Assert.True(user.BannedUntil > DateTime.Now); // suspension applied
        }

        [Fact]
        public async Task ResetFailedLogins_UserExists_ResetsCountToZero()
        {
            var email = "existing@example.com";
            var user = new User { Email = email };
            var failedLoginsCount = 0;

            var mockRepository = new Mock<IUserRepository>();
            mockRepository.Setup(repository => repository.GetUserByEmail(email)).ReturnsAsync(user);
            mockRepository.Setup(repository => repository.UpdateUserFailedLoginsCount(user, failedLoginsCount)).Returns(Task.CompletedTask);

            var service = new UserService(mockRepository.Object);

            await service.ResetFailedLogins(email);

            mockRepository.Verify(repository => repository.UpdateUserFailedLoginsCount(user, failedLoginsCount), Times.Once);
        }

        [Fact]
        public async Task ResetFailedLogins_UserIsNull_DoesNothing()
        {
            var email = "ghost@example.com";

            var mockRepository = new Mock<IUserRepository>();
            mockRepository.Setup(repository => repository.GetUserByEmail(email)).ReturnsAsync((User?)null);

            var service = new UserService(mockRepository.Object);

            await service.ResetFailedLogins(email);

            mockRepository.Verify(repository => repository.UpdateUserFailedLoginsCount(It.IsAny<User>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task LoginAsync_EmailIsEmpty_ReturnsError()
        {
            var service = new UserService(new Mock<IUserRepository>().Object);

            var result = await service.LoginAsync("", "password", "abc", "abc");

            Assert.False(result.Success);
            Assert.Equal("Email cannot be empty!", result.Message);
        }

        [Fact]
        public async Task LoginAsync_PasswordIsEmpty_ReturnsError()
        {
            var service = new UserService(new Mock<IUserRepository>().Object);

            var result = await service.LoginAsync("user@example.com", "", "abc", "abc");

            Assert.False(result.Success);
            Assert.Equal("Password cannot be empty!", result.Message);
        }

        [Fact]
        public async Task LoginAsync_InvalidEmailFormat_ReturnsError()
        {
            var service = new UserService(new Mock<IUserRepository>().Object);

            var result = await service.LoginAsync("invalid-email", "password", "abc", "abc");

            Assert.False(result.Success);
            Assert.Equal("Email does not have the right format!", result.Message);
        }

        [Fact]
        public async Task LoginAsync_CaptchaMismatch_ReturnsError()
        {
            var service = new UserService(new Mock<IUserRepository>().Object);

            var result = await service.LoginAsync("user@example.com", "password", "wrong", "right");

            Assert.False(result.Success);
            Assert.Equal("Captcha verification failed.", result.Message);
        }

        [Fact]
        public async Task LoginAsync_EmailNotFound_ReturnsError()
        {
            var mockRepo = new Mock<IUserRepository>();
            mockRepo.Setup(repository => repository.EmailExists("ghost@example.com")).ReturnsAsync(false);

            var service = new UserService(mockRepo.Object);

            var result = await service.LoginAsync("ghost@example.com", "password", "captcha", "captcha");

            Assert.False(result.Success);
            Assert.Equal("Email does not exist.", result.Message);
        }

        [Fact]
        public async Task LoginAsync_UserIsNull_ReturnsError()
        {
            var email = "user@example.com";

            var mockRepository = new Mock<IUserRepository>();
            mockRepository.Setup(repository => repository.EmailExists(email)).ReturnsAsync(true);
            mockRepository.Setup(repository => repository.GetUserByEmail(email)).ReturnsAsync((User?)null);

            var service = new UserService(mockRepository.Object);

            var result = await service.LoginAsync(email, "password", "captcha", "captcha");

            Assert.False(result.Success);
            Assert.Equal("Email does not exist.", result.Message);
        }

        [Fact]
        public async Task LoginAsync_UserIsBanned_ReturnsError()
        {
            var email = "banned@example.com";
            var user = new User { Email = email, IsBanned = true };

            var mockRepository = new Mock<IUserRepository>();
            mockRepository.Setup(repository => repository.EmailExists(email)).ReturnsAsync(true);
            mockRepository.Setup(repository => repository.GetUserByEmail(email)).ReturnsAsync(user);

            var service = new UserService(mockRepository.Object);

            var result = await service.LoginAsync(email, "password", "captcha", "captcha");

            Assert.False(result.Success);
            Assert.Equal("User is banned.", result.Message);
        }

        [Fact]
        public async Task LoginAsync_ValidInput_ReturnsSuccess()
        {
            var email = "user@example.com";
            var user = new User { Email = email, IsBanned = false };

            var mockRepository = new Mock<IUserRepository>();
            mockRepository.Setup(repository => repository.EmailExists(email)).ReturnsAsync(true);
            mockRepository.Setup(repository => repository.GetUserByEmail(email)).ReturnsAsync(user);

            var service = new UserService(mockRepository.Object);

            var result = await service.LoginAsync(email, "password", "captcha", "captcha");

            Assert.True(result.Success);
            Assert.Equal("Success", result.Message);
            Assert.NotNull(result.User);
        }

        [Fact]
        public async Task GetAllUsers_ReturnsExpectedUsers()
        {
            var numberOfUsers = 2;
            var username1 = "user1";
            var username2 = "user2";
            var mockRepository = new Mock<IUserRepository>();
            var expectedUsers = new List<User>
            {
                new User { UserId = 1, Username = username1, Email = "user1@example.com" },
                new User { UserId = 2, Username = username2, Email = "user2@example.com" }
            };

            mockRepository.Setup(repository => repository.GetAllUsers()).ReturnsAsync(expectedUsers);

            var service = new UserService(mockRepository.Object);

            var result = await service.GetAllUsers();

            Assert.Equal(numberOfUsers, result.Count);
            Assert.Equal("user1", result[0].Username);
            Assert.Equal("user2", result[1].Username);
        }

    }
}
