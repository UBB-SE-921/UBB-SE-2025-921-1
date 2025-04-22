using System;
using System.Data;
using System.Threading.Tasks;
using SharedClassLibrary.Shared;
using SharedClassLibrary.IRepository;

namespace Marketplace924.Repository
{
    /// <summary>
    /// Provides database operations for wallet management.
    /// </summary>
    public class DummyWalletProxyRepository : IDummyWalletRepository
    {
        public Task<float> GetWalletBalanceAsync(int userId)
        {
            throw new NotImplementedException();
        }

        public Task UpdateWalletBalance(int userId, float newBalance)
        {
            throw new NotImplementedException();
        }
    }
} 