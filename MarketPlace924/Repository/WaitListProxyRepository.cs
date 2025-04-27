using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using SharedClassLibrary.Domain;
using SharedClassLibrary.Shared;
using SharedClassLibrary.IRepository;

namespace MarketPlace924.Repository
{
    public class WaitListProxyRepository : IWaitListRepository
    {
        public Task AddUserToWaitlist(int userId, int productId)
        {
            throw new NotImplementedException();
        }

        public async Task<List<UserWaitList>> GetUsersInWaitlist(int productId)
        {
            throw new NotImplementedException();
        }

        public async Task<List<UserWaitList>> GetUsersInWaitlistOrdered(int productId)
        {
            throw new NotImplementedException();
        }

        public async Task<int> GetUserWaitlistPosition(int userId, int productId)
        {
            throw new NotImplementedException();
        }

        public async Task<List<UserWaitList>> GetUserWaitlists(int userId)
        {
            throw new NotImplementedException();
        }

        public async Task<int> GetWaitlistSize(int productId)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> IsUserInWaitlist(int userId, int productId)
        {
            throw new NotImplementedException();
        }

        public async Task RemoveUserFromWaitlist(int userId, int productId)
        {
            throw new NotImplementedException();
        }
    }
}