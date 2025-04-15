using MarketPlace924.DBConnection;
using MarketPlace924.Domain;
using MarketPlace924.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace XUnitTestProject.RepositoryTests
{
    [Collection("DatabaseCollection")]
    public class UserRepositoryTests
    {
        private readonly DatabaseFixture _fixture;

        public UserRepositoryTests(DatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        private UserRepository CreateRepository()
        {
            var connection = new DatabaseConnection(_fixture.TestConnectionString);
            return new UserRepository(connection);
        }

        private async Task<int> SeedUserAsync(User user)
        {
            using var connection = new SqlConnection(_fixture.TestConnectionString);
            await connection.OpenAsync();
            using var command = connection.CreateCommand();

            command.CommandText = @"
            INSERT INTO Users (Username, Email, PhoneNumber, Password, Role, FailedLogins, BannedUntil, IsBanned)
            VALUES (@Username, @Email, @PhoneNumber, @Password, @Role, @FailedLogins, @BannedUntil, @IsBanned);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

            command.Parameters.AddWithValue("@Username", user.Username);
            command.Parameters.AddWithValue("@Email", user.Email);
            command.Parameters.AddWithValue("@PhoneNumber", user.PhoneNumber);
            command.Parameters.AddWithValue("@Password", user.Password);
            command.Parameters.AddWithValue("@Role", (int)user.Role);
            command.Parameters.AddWithValue("@FailedLogins", user.FailedLogins);
            command.Parameters.AddWithValue("@BannedUntil", (object?)user.BannedUntil ?? DBNull.Value);
            command.Parameters.AddWithValue("@IsBanned", user.IsBanned);

            var result = await command.ExecuteScalarAsync();
            return (int)result;
        }

        [Fact]
        public async Task AddUser_ShouldInsertUserSuccessfully()
        {
            var repository = CreateRepository();
            var user = new User(0, "adduser", "adduser_only@example.com", "+40700000001", "hashedpass", UserRole.Buyer, 0, null, false);

            await repository.AddUser(user);
            var totalNumberOfUsers = await repository.GetTotalNumberOfUsers();

            Assert.True(totalNumberOfUsers > 0);
        }

        [Fact]
        public async Task AddUser_WithBannedUntilNotNull_InsertsCorrectly()
        {
            var dbConnection = new DatabaseConnection(_fixture.TestConnectionString);
            var repository = new UserRepository(dbConnection);
            var numberOfMinutesInTheFutureToBeAdded = 15;

            var bannedUntil = DateTime.Now.AddMinutes(numberOfMinutesInTheFutureToBeAdded);
            var user = new User
            {
                Username = "banneduser",
                Email = "banned@example.com",
                PhoneNumber = "+40700000000",
                Password = "hashedPassword",
                Role = UserRole.Buyer,
                FailedLogins = 2,
                BannedUntil = bannedUntil,
                IsBanned = false
            };

            await repository.AddUser(user);

            var insertedUser = await repository.GetUserByEmail(user.Email);
            Assert.NotNull(insertedUser);
            Assert.Equal(bannedUntil.ToString("s"), insertedUser!.BannedUntil?.ToString("s"));
        }

        [Fact]
        public async Task GetUserByUsername_ShouldReturnCorrectUser()
        {
            var repository = CreateRepository();
            var username = "byusername";
            var email = "email@byusername.com";
            var user = new User(0, username, email, "+40700001111", "pwd", UserRole.Seller, 0, null, false);
            await repository.AddUser(user);

            var fetchedUser = await repository.GetUserByUsername(username);

            Assert.NotNull(fetchedUser);
            Assert.Equal(email, fetchedUser.Email);
        }

        [Fact]
        public async Task GetUserByUsername_NonExistentUser_ReturnsNull()
        {
            var dbConnection = new DatabaseConnection(_fixture.TestConnectionString);
            var repository = new UserRepository(dbConnection);
            var randomUsername = "ghost_user_does_not_exist";

            var result = await repository.GetUserByUsername(randomUsername);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserByUsername_WithBannedUntilSet_ReturnsCorrectUser()
        {
            var dbConnection = new DatabaseConnection(_fixture.TestConnectionString);
            var repository = new UserRepository(dbConnection);
            var numberOfMinutesInTheFutureToBeAdded = 20;

            var bannedUntil = DateTime.Now.AddMinutes(numberOfMinutesInTheFutureToBeAdded);
            var user = new User
            {
                Username = "bannedUser123",
                Email = "bannedUser@example.com",
                PhoneNumber = "+40700001111",
                Password = "someHashedPassword",
                Role = UserRole.Buyer,
                FailedLogins = 1,
                BannedUntil = bannedUntil,
                IsBanned = false
            };

            await repository.AddUser(user);

            var result = await repository.GetUserByUsername(user.Username);

            Assert.NotNull(result);
            Assert.Equal(bannedUntil.ToString("s"), result!.BannedUntil?.ToString("s"));
        }

        [Fact]
        public async Task UpdateUserFailedLoginsCount_ShouldChangeValue()
        {
            var newCountOfFailedLogins = 3;
            var email = "fail@logins.com";
            var repository = CreateRepository();

            var user = new User(0, "failuser", email, "+40712345678", "pwd", UserRole.Buyer, 0, null, false);
            await repository.AddUser(user);

            var dbUser = await repository.GetUserByEmail(email);
            Assert.NotNull(dbUser);

            await repository.UpdateUserFailedLoginsCount(dbUser, newCountOfFailedLogins);

            var count = await repository.GetFailedLoginsCountByUserId(dbUser.UserId);
            Assert.Equal(newCountOfFailedLogins, count);
        }

        [Fact]
        public async Task UpdateUser_ShouldModifyValues()
        {
            var repository = CreateRepository();
            var user = new User(0, "origuser", "orig@e.com", "+40788888888", "pwd", UserRole.Buyer, 0, null, false);
            var id = await SeedUserAsync(user);
            user.UserId = id;

            var newUsername = "updateduser";
            var newEmail = "new@email.com";
            var newPhoneNumber = "+40799999999";
            var newPassword = "newpwd";

            user.Username = newUsername;
            user.Email = newEmail;
            user.PhoneNumber = newPhoneNumber;
            user.Password = newPassword;

            await repository.UpdateUser(user);
            var updated = await repository.GetUserByUsername(newUsername);

            Assert.Equal(newEmail, updated.Email);
            Assert.Equal(newPhoneNumber, updated.PhoneNumber);
        }

        [Fact]
        public async Task UpdateUser_WithBannedUntilNotNull_UpdatesCorrectly()
        {
            var repository = CreateRepository();
            var numberOfMinutesInTheFutureToBeAdded = 30;
            var newPassword = "updatedPassword";

            var user = new User
            {
                Username = "updateTest",
                Email = "update@example.com",
                PhoneNumber = "+40712345678",
                Password = "initialPassword",
                Role = UserRole.Buyer,
                FailedLogins = 0,
                BannedUntil = null,
                IsBanned = false
            };

            await repository.AddUser(user);

            var saved = await repository.GetUserByUsername(user.Username);
            Assert.NotNull(saved);

            saved!.BannedUntil = DateTime.Now.AddMinutes(numberOfMinutesInTheFutureToBeAdded);
            saved.Password = newPassword;
            await repository.UpdateUser(saved);

            var updated = await repository.GetUserByUsername(saved.Username);
            Assert.NotNull(updated);
            Assert.Equal(newPassword, updated!.Password);
            Assert.NotNull(updated.BannedUntil);
            Assert.True(updated.BannedUntil > DateTime.Now);
        }

        [Fact]
        public async Task GetUserByEmail_ShouldReturnCorrectUser()
        {
            var repository = CreateRepository();
            var email = "getuser_only@example.com";
            var username = "getuser";
            var user = new User(0, username, email, "+40700000002", "hashedpass", UserRole.Buyer, 0, null, false);

            await repository.AddUser(user);

            var retrievedUser = await repository.GetUserByEmail(email);

            Assert.NotNull(retrievedUser);
            Assert.Equal(username, retrievedUser.Username);
        }

        [Fact]
        public async Task GetUserByEmail_WithNonExistentEmail_ReturnsNull()
        {
            var repository = CreateRepository();

            var email = "nonexistent@example.com";

            var user = await repository.GetUserByEmail(email);

            Assert.Null(user);
        }

        [Fact]
        public async Task EmailExists_ShouldReturnTrue()
        {
            var email = "email@check.com";
            var repository = CreateRepository();
            var user = new User(0, "emailuser", email, "+40712345678", "pwd", UserRole.Seller, 0, null, false);
            await repository.AddUser(user);

            var emailExists = await repository.EmailExists(email);
            Assert.True(emailExists);
        }

        [Fact]
        public async Task UsernameExists_ShouldReturnTrue()
        {
            var username = "existinguser";
            var repository = CreateRepository();
            var user = new User(0, username, "exists@example.com", "+40712345678", "pwd", UserRole.Seller, 0, null, false);
            await repository.AddUser(user);

            var exists = await repository.UsernameExists(username);
            Assert.True(exists);
        }

        [Fact]
        public async Task GetFailedLoginsCountByUserId_ReturnsCorrectValue()
        {
            var repository = CreateRepository();
            var expectedFailedLogins = 4;
            var email = "failcount@example.com";
            var user = new User(0, "faileduser", email, "+40712345678", "pwd", UserRole.Buyer, expectedFailedLogins, null, false);
            await repository.AddUser(user);

            var dbUser = await repository.GetUserByEmail(email);
            Assert.NotNull(dbUser);

            var count = await repository.GetFailedLoginsCountByUserId(dbUser.UserId);

            Assert.Equal(expectedFailedLogins, count);
        }

        [Fact]
        public async Task UpdateUserPhoneNumber_ShouldUpdatePhoneNumberInDatabase()
        {
            var repository = CreateRepository();
            var username = "updatephone";
            var newPhoneNumber = "+40987654321";
            var user = new User
            {
                Username = username,
                Email = "update@phone.com",
                PhoneNumber = "+40123456789",
                Password = "password",
                Role = UserRole.Buyer,
                FailedLogins = 0,
                BannedUntil = null,
                IsBanned = false
            };

            await repository.AddUser(user);

            var existingUser = await repository.GetUserByUsername(username);
            Assert.NotNull(existingUser);

            existingUser.PhoneNumber = newPhoneNumber;
            await repository.UpdateUserPhoneNumber(existingUser);

            var updatedUser = await repository.GetUserByUsername(username);
            Assert.Equal(newPhoneNumber, updatedUser?.PhoneNumber);
        }

        [Fact]
        public async Task LoadUserPhoneNumberAndEmailById_ShouldPopulateUserProperties()
        {
            var repository = CreateRepository();
            var email = "contact@test.com";
            var user = new User(0, "infoUser", email, "+40740000000", "pwd", UserRole.Buyer, 0, null, false);
            await repository.AddUser(user);

            var insertedUser = await repository.GetUserByEmail(email);
            Assert.NotNull(insertedUser);

            var userToLoad = new User { UserId = insertedUser.UserId };

            await repository.LoadUserPhoneNumberAndEmailById(userToLoad);

            Assert.Equal(user.PhoneNumber, userToLoad.PhoneNumber);
            Assert.Equal(user.Email, userToLoad.Email);
        }

        [Fact]
        public async Task LoadUserPhoneNumberAndEmailById_UserNotFound_DoesNothing()
        {
            var repo = CreateRepository();
            var user = new User { UserId = -999 };

            await repo.LoadUserPhoneNumberAndEmailById(user);

            Assert.True(string.IsNullOrEmpty(user.Email));
            Assert.True(string.IsNullOrEmpty(user.PhoneNumber));
        }

        [Fact]
        public async Task GetAllUsers_ShouldReturnInsertedUser()
        {
            var repository = CreateRepository();
            var email = "all@example.com";
            var user = new User(0, "alluser", email, "+40711111111", "pwd", UserRole.Seller, 0, null, false);
            await repository.AddUser(user);

            var allUsers = await repository.GetAllUsers();
            Assert.Contains(allUsers, u => u.Email == email);
        }

        [Fact]
        public async Task GetAllUsers_IncludesUsersWithBannedUntilNotNull()
        {
            var numberOfMinutesInTheFutureToBeAdded = 10;
            var email = "banned999@example.com";
            var repository = CreateRepository();
            var bannedUntil = DateTime.Now.AddMinutes(numberOfMinutesInTheFutureToBeAdded); 
            var user = new User(0, "banneduser999", email, "+40712345678", "securepass", UserRole.Buyer, 0, bannedUntil, false);

            await repository.AddUser(user);

            var allUsers = await repository.GetAllUsers();

            var retrievedUser = allUsers.FirstOrDefault(u => u.Email == email);

            Assert.NotNull(retrievedUser);
            Assert.Equal(user.Username, retrievedUser.Username);
            Assert.Equal(bannedUntil.Minute, retrievedUser.BannedUntil?.Minute);
            Assert.NotNull(retrievedUser.BannedUntil);
        }

        [Fact]
        public async Task GetTotalNumberOfUsers_ReturnsCorrectCount()
        {
            var repo = CreateRepository();
            var numberOfAddedUsers = 2;
            var uniqueSuffix = Guid.NewGuid().ToString("N").Substring(0, 8);
            var user1 = new User(0, $"user1_{uniqueSuffix}", $"user1_{uniqueSuffix}@example.com", "+40700000001", "pwd1", UserRole.Buyer, 0, null, false);
            var user2 = new User(0, $"user2_{uniqueSuffix}", $"user2_{uniqueSuffix}@example.com", "+40700000002", "pwd2", UserRole.Buyer, 0, null, false);

            await repo.AddUser(user1);
            await repo.AddUser(user2);

            var totalNumberOfUsers = await repo.GetTotalNumberOfUsers();

            Assert.True(totalNumberOfUsers >= numberOfAddedUsers);
        }

    }
}
