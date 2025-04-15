// -----------------------------------------------------------------------
// <copyright file="UserRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SharedClassLibrary.IRepository
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Data.SqlClient;
    using Server.DBConnection;
    using SharedClassLibrary.Domain;

    /// <summary>
    /// Provides methods for interacting with the Users table in the database.
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private DatabaseConnection databaseConnection;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserRepository"/> class.
        /// </summary>
        /// <param name="databaseConnection">The database connection to be used by the repository.</param>
        public UserRepository(DatabaseConnection databaseConnection)
        {
            this.databaseConnection = databaseConnection;
        }

        /// <summary>
        /// Connects to the database and adds a new user.
        /// </summary>
        /// <param name="user">The user to be added to the database.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task AddUser(User user)
        {
            await this.databaseConnection.OpenConnection();

            var sqlConnection = this.databaseConnection.GetConnection();
            var sqlCommand = sqlConnection.CreateCommand();

            sqlCommand.CommandText = @"
			INSERT INTO Users (Username, Email, PhoneNumber, Password, Role, FailedLogins, BannedUntil, IsBanned)
			VALUES (@Username, @Email, @PhoneNumber, @Password, @Role, @FailedLogins, @BannedUntil, @IsBanned)";

            sqlCommand.Parameters.AddWithValue("@Username", user.Username);
            sqlCommand.Parameters.AddWithValue("@Email", user.Email);
            sqlCommand.Parameters.AddWithValue("@PhoneNumber", user.PhoneNumber);
            sqlCommand.Parameters.AddWithValue("@Password", user.Password);
            sqlCommand.Parameters.AddWithValue("@Role", (int)user.Role);
            sqlCommand.Parameters.AddWithValue("@FailedLogins", user.FailedLogins);

            if (user.BannedUntil == null)
            {
                sqlCommand.Parameters.AddWithValue("@BannedUntil", DBNull.Value);
            }
            else
            {
                sqlCommand.Parameters.AddWithValue("@BannedUntil", user.BannedUntil);
            }

            sqlCommand.Parameters.AddWithValue("@IsBanned", user.IsBanned);

            sqlCommand.ExecuteNonQuery();
            this.databaseConnection.CloseConnection();
        }

        /// <summary>
        /// Retrieves a user from the database by their username. If no user is found, returns null.
        /// </summary>
        /// <param name="username">The username of the user that we search for.</param>
        /// <returns>A <see cref="Task{User?}"/> representing the result of the asynchronous operation, with a result of the user if found, or null if not found.</returns>
        public async Task<User?> GetUserByUsername(string username)
        {
            await this.databaseConnection.OpenConnection();
            using var sqlCommand = this.databaseConnection.GetConnection().CreateCommand();

            sqlCommand.CommandText = "SELECT * FROM Users WHERE Username = @Username";
            sqlCommand.Parameters.Add(new SqlParameter("@Username", username));

            using var reader = await sqlCommand.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                return null;
            }

            var userId = reader.GetInt32(reader.GetOrdinal("UserID"));
            var email = reader.GetString(reader.GetOrdinal("Email"));
            var phoneNumber = reader.GetString(reader.GetOrdinal("PhoneNumber"));
            var password = reader.GetString(reader.GetOrdinal("Password"));
            var role = (UserRole)reader.GetInt32(reader.GetOrdinal("Role"));
            var failedLoginsCount = reader.GetInt32(reader.GetOrdinal("FailedLogins"));

            DateTime? bannedUntil = null;
            if (!reader.IsDBNull(reader.GetOrdinal("BannedUntil")))
            {
                bannedUntil = reader.GetDateTime(reader.GetOrdinal("BannedUntil"));
            }

            var userIsBanned = reader.GetBoolean(reader.GetOrdinal("IsBanned"));
            this.databaseConnection.CloseConnection();
            return new User(userId, username, email, phoneNumber, password, role, failedLoginsCount, bannedUntil, userIsBanned);
        }

        /// <summary>
        /// Updates the failed login count for a specified user.
        /// </summary>
        /// <param name="user">The user whose failed login count is to be updated.</param>
        /// <param name="newValueOfFailedLogIns">The new value for the failed login count.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task UpdateUserFailedLoginsCount(User user, int newValueOfFailedLogIns)
        {
            await this.databaseConnection.OpenConnection();
            var sqlCommand = this.databaseConnection.GetConnection().CreateCommand();

            sqlCommand.CommandText = "UPDATE Users SET FailedLogins = @FailedLogins WHERE UserID = @UserID";
            user.FailedLogins = newValueOfFailedLogIns;
            sqlCommand.Parameters.Add(new SqlParameter("@FailedLogins", user.FailedLogins));
            sqlCommand.Parameters.Add(new SqlParameter("@UserID", user.UserId));
            await sqlCommand.ExecuteNonQueryAsync();
            this.databaseConnection.CloseConnection();
        }

        /// <summary>
        /// Updates the details of an existing user in the database. The user is identified by their user ID and the other fields are updated with the values from the user object.
        /// </summary>
        /// <param name="user">The user object containing updated information.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task UpdateUser(User user)
        {
            await this.databaseConnection.OpenConnection();
            using var sqlCommand = this.databaseConnection.GetConnection().CreateCommand();

            sqlCommand.CommandText = "UPDATE Users SET Username = @Username, Email = @Email, PhoneNumber = @PhoneNumber, Password = @Password, Role = @Role, FailedLogins = @FailedLogins, BannedUntil = @BannedUntil, IsBanned = @IsBanned WHERE UserID = @UserID";
            sqlCommand.Parameters.Add(new SqlParameter("@Username", user.Username));
            sqlCommand.Parameters.Add(new SqlParameter("@Email", user.Email));
            sqlCommand.Parameters.Add(new SqlParameter("@PhoneNumber", user.PhoneNumber));
            sqlCommand.Parameters.Add(new SqlParameter("@Password", user.Password));
            sqlCommand.Parameters.Add(new SqlParameter("@Role", user.Role));
            sqlCommand.Parameters.Add(new SqlParameter("@FailedLogins", user.FailedLogins));

            if (user.BannedUntil == null)
            {
                sqlCommand.Parameters.Add(new SqlParameter("@BannedUntil", DBNull.Value));
            }
            else
            {
                sqlCommand.Parameters.Add(new SqlParameter("@BannedUntil", user.BannedUntil));
            }

            sqlCommand.Parameters.Add(new SqlParameter("@IsBanned", user.IsBanned));
            sqlCommand.Parameters.Add(new SqlParameter("@UserID", user.UserId));
            sqlCommand.ExecuteNonQuery();
            this.databaseConnection.CloseConnection();
        }

        /// <summary>
        /// Retrieves a user from the database by their email address. If no user is found, returns null.
        /// </summary>
        /// <param name="email">The email address of the user to search for.</param>
        /// <returns>A <see cref="Task{User?}"/> representing the result of the asynchronous operation. The task result contains the user if found or null if no user is found with the specified email address.</returns>
        public async Task<User?> GetUserByEmail(string email)
        {
            await this.databaseConnection.OpenConnection();
            var sqlCommand = this.databaseConnection.GetConnection().CreateCommand();

            sqlCommand.CommandText = "SELECT * FROM Users WHERE Email = @Email";
            sqlCommand.Parameters.Add(new SqlParameter("@Email", email));

            var reader = await sqlCommand.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                return null;
            }

            var userId = reader.GetInt32(reader.GetOrdinal("UserID"));
            var username = reader.GetString(reader.GetOrdinal("Username"));
            var phoneNumber = reader.GetString(reader.GetOrdinal("PhoneNumber"));
            var password = reader.GetString(reader.GetOrdinal("Password"));
            var role = (UserRole)reader.GetInt32(reader.GetOrdinal("Role"));
            var failedLoginsCount = reader.GetInt32(reader.GetOrdinal("FailedLogins"));

            DateTime? bannedUntil = null;
            if (!reader.IsDBNull(reader.GetOrdinal("BannedUntil")))
            {
                bannedUntil = reader.GetDateTime(reader.GetOrdinal("BannedUntil"));
            }

            var isBanned = reader.GetBoolean(reader.GetOrdinal("IsBanned"));

            await reader.CloseAsync();
            this.databaseConnection.CloseConnection();
            return new User(userId, username, email, phoneNumber, password, role, failedLoginsCount, bannedUntil, isBanned);
        }

        /// <summary>
        /// Checks if an email address already exists in the database.
        /// </summary>
        /// <param name="email">The email address to check for existence.</param>
        /// <returns>
        /// A <see cref="Task{bool}"/> representing the result of the asynchronous operation.
        /// The task result contains true if the email exists, otherwise false.
        /// </returns>
        public async Task<bool> EmailExists(string email)
        {
            await this.databaseConnection.OpenConnection();
            var sqlCommand = this.databaseConnection.GetConnection().CreateCommand();

            sqlCommand.CommandText = "SELECT count(1) FROM Users WHERE Email = @Email";
            sqlCommand.Parameters.Add(new SqlParameter("@Email", email));
            var numberOfUsersWithThisEmail = (int)(await sqlCommand.ExecuteScalarAsync() ?? 0);
            this.databaseConnection.CloseConnection();
            return numberOfUsersWithThisEmail > 0;
        }

        /// <summary>
        /// Checks if a username already exists in the database.
        /// </summary>
        /// <param name="username">The username to check for existence.</param>
        /// <returns>
        /// A <see cref="Task{bool}"/> representing the result of the asynchronous operation.
        /// The task result contains true if the username exists, otherwise false.
        /// </returns>
        public async Task<bool> UsernameExists(string username)
        {
            await this.databaseConnection.OpenConnection();
            var sqlCommand = this.databaseConnection.GetConnection().CreateCommand();

            sqlCommand.CommandText = "SELECT COUNT(1) FROM Users WHERE Username = @Username";
            sqlCommand.Parameters.Add(new SqlParameter("@Username", username));

            int usernameExists = 0;
            var numberOfUsersWithThisUsername = await sqlCommand.ExecuteScalarAsync();

            if (numberOfUsersWithThisUsername != null)
            {
                usernameExists = (int)numberOfUsersWithThisUsername;
            }

            return usernameExists > 0;
        }

        /// <summary>
        /// Retrieves the count of failed login attempts for a specified user by their user ID.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>
        /// A <see cref="Task{int}"/> representing the result of the asynchronous operation.
        /// The task result contains the count of failed login attempts for the specified user.
        /// </returns>
        public async Task<int> GetFailedLoginsCountByUserId(int userId)
        {
            await this.databaseConnection.OpenConnection();
            var sqlCommand = this.databaseConnection.GetConnection().CreateCommand();

            sqlCommand.CommandText = "SELECT FailedLogins FROM Users WHERE UserID = @UserID";
            sqlCommand.Parameters.Add(new SqlParameter("@UserID", userId));

            int numberOfFailedLoginsToInt = 0;
            var numberOfFailedLogins = await sqlCommand.ExecuteScalarAsync();
            if (numberOfFailedLogins != null)
            {
                numberOfFailedLoginsToInt = (int)numberOfFailedLogins;
            }

            this.databaseConnection.CloseConnection();
            return numberOfFailedLoginsToInt;
        }

        // region: For Buyer

        /// <summary>
        /// Updates the contact information (phone number) of an existing user in the database.
        /// </summary>
        /// <param name="user">The user object containing the updated phone number.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the result of the asynchronous operation.
        /// </returns>
        public async Task UpdateUserPhoneNumber(User user)
        {
            await this.databaseConnection.OpenConnection();
            var sqlConnection = this.databaseConnection.GetConnection();
            var sqlCommand = sqlConnection.CreateCommand();
            sqlCommand.CommandText = "UPDATE Users SET PhoneNumber = @PhoneNumber WHERE UserID = @UserID";

            sqlCommand.Parameters.Add(new SqlParameter("@PhoneNumber", user.PhoneNumber));
            sqlCommand.Parameters.Add(new SqlParameter("@UserID", user.UserId));
            await sqlCommand.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Loads the contact information (phone number and email) of a user from the database by their user ID.
        /// </summary>
        /// <param name="user">The user object to populate with the contact information.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the result of the asynchronous operation.
        /// </returns>
        public async Task LoadUserPhoneNumberAndEmailById(User user)
        {
            await this.databaseConnection.OpenConnection();
            var sqlConnection = this.databaseConnection.GetConnection();
            var sqlCommand = sqlConnection.CreateCommand();
            sqlCommand.CommandText = "SELECT PhoneNumber, Email FROM Users WHERE UserID = @UserID";
            sqlCommand.Parameters.Add(new SqlParameter("@UserID", user.UserId));
            var reader = await sqlCommand.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                return;
            }

            user.PhoneNumber = reader.GetString(reader.GetOrdinal("PhoneNumber"));
            user.Email = reader.GetString(reader.GetOrdinal("Email"));
            await reader.CloseAsync();
        }

        /// <summary>
        /// Retrieves all users from the database.
        /// </summary>
        /// <returns>
        /// A <see cref="Task{List{User}}"/> representing the result of the asynchronous operation.
        /// The task result contains a list of all users in the database.
        /// </returns>
        public async Task<List<User>> GetAllUsers()
        {
            await this.databaseConnection.OpenConnection();
            var sqlCommand = this.databaseConnection.GetConnection().CreateCommand();

            sqlCommand.CommandText = "SELECT * FROM Users";

            var reader = sqlCommand.ExecuteReader();

            var listOfUsers = new List<User>();

            while (await reader.ReadAsync())
            {
                var userId = reader.GetInt32(reader.GetOrdinal("UserID"));
                var username = reader.GetString(reader.GetOrdinal("Username"));
                var email = reader.GetString(reader.GetOrdinal("Email"));
                var phoneNumber = reader.GetString(reader.GetOrdinal("PhoneNumber"));
                var password = reader.GetString(reader.GetOrdinal("Password"));
                var role = (UserRole)reader.GetInt32(reader.GetOrdinal("Role"));
                var failedLoginsCount = reader.GetInt32(reader.GetOrdinal("FailedLogins"));

                DateTime? bannedUntil = null;
                if (!reader.IsDBNull(reader.GetOrdinal("BannedUntil")))
                {
                    bannedUntil = reader.GetDateTime(reader.GetOrdinal("BannedUntil"));
                }

                var isBanned = reader.GetBoolean(reader.GetOrdinal("IsBanned"));

                listOfUsers.Add(new User(userId, username, email, phoneNumber, password, role, failedLoginsCount, bannedUntil, isBanned));
            }

            await reader.CloseAsync();
            this.databaseConnection.CloseConnection();
            return listOfUsers;
        }

        /// <summary>
        /// Retrieves the total number of users in the database.
        /// </summary>
        /// <returns>
        /// A <see cref="Task{int}"/> representing the result of the asynchronous operation.
        /// The task result contains the total number of users in the database.
        /// </returns>
        public async Task<int> GetTotalNumberOfUsers()
        {
            await this.databaseConnection.OpenConnection();
            var sqlCommand = this.databaseConnection.GetConnection().CreateCommand();

            sqlCommand.CommandText = "SELECT Count(*) FROM Users";

            var numberOfUsers = (int)sqlCommand.ExecuteScalar();

            this.databaseConnection.CloseConnection();
            return numberOfUsers;
        }

        // endregion
    }
}
