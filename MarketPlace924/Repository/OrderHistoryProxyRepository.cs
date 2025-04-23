using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using SharedClassLibrary.Domain;
using SharedClassLibrary.Shared;
using SharedClassLibrary.IRepository;

namespace MarketPlace924.Repository
{
    /// <summary>
    /// Provides database operations for order history management.
    /// </summary>
    public class OrderHistoryProxyRepository : IOrderHistoryRepository
    {
        public Task<List<DummyProduct>> GetDummyProductsFromOrderHistoryAsync(int orderHistoryId)
        {
            throw new NotImplementedException();
        }
    }
} 