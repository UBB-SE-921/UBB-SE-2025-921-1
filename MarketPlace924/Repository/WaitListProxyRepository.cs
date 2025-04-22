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

namespace Marketplace924.Repository
{
    public class WaitListProxyRepository : IWaitListRepository
    {
        public void AddUserToWaitlist(int userId, int productWaitListId)
        {
            throw new NotImplementedException();
        }

        public List<UserWaitList> GetUsersInWaitlist(int waitListProductId)
        {
            throw new NotImplementedException();
        }

        public List<UserWaitList> GetUsersInWaitlistOrdered(int productId)
        {
            throw new NotImplementedException();
        }

        public int GetUserWaitlistPosition(int userId, int productId)
        {
            throw new NotImplementedException();
        }

        public List<UserWaitList> GetUserWaitlists(int userId)
        {
            throw new NotImplementedException();
        }

        public int GetWaitlistSize(int productWaitListId)
        {
            throw new NotImplementedException();
        }

        public bool IsUserInWaitlist(int userId, int productId)
        {
            throw new NotImplementedException();
        }

        public void RemoveUserFromWaitlist(int userId, int productWaitListId)
        {
            throw new NotImplementedException();
        }
    }
}