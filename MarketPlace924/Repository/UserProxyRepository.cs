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
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.IRepository;

    /// <summary>
    /// Provides methods for interacting with the Users table in the database.
    /// </summary>
    public class UserProxyRepository : IUserRepository
    {
        public Task AddUser(User user)
        {
            throw new NotImplementedException();
        }

        public Task<bool> EmailExists(string email)
        {
            throw new NotImplementedException();
        }

        public Task<List<User>> GetAllUsers()
        {
            throw new NotImplementedException();
        }

        public Task<int> GetFailedLoginsCountByUserId(int userId)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetTotalNumberOfUsers()
        {
            throw new NotImplementedException();
        }

        public Task<User?> GetUserByEmail(string email)
        {
            throw new NotImplementedException();
        }

        public Task<User?> GetUserByUsername(string username)
        {
            throw new NotImplementedException();
        }

        public Task LoadUserPhoneNumberAndEmailById(User user)
        {
            throw new NotImplementedException();
        }

        public Task UpdateUser(User user)
        {
            throw new NotImplementedException();
        }

        public Task UpdateUserFailedLoginsCount(User user, int newValueOfFailedLogIns)
        {
            throw new NotImplementedException();
        }

        public Task UpdateUserPhoneNumber(User user)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UsernameExists(string username)
        {
            throw new NotImplementedException();
        }
    }
}
