using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharedClassLibrary.Domain;
using MarketPlace924.Repository;
using SharedClassLibrary.Shared;
using SharedClassLibrary.IRepository;
using MarketPlace924.Helper;

namespace MarketPlace924.Service
{
    /// <summary>
    /// Service for managing order history operations.
    /// </summary>
    public class OrderHistoryService : IOrderHistoryService
    {
        private readonly IOrderHistoryRepository orderHistoryRepository;



        /// <summary>
        /// Initializes a new instance of the <see cref="OrderHistoryService"/> class with a specified database provider.
        /// </summary>
        /// <param name="connectionString">The database connection string.</param>
        /// <param name="databaseProvider">The database provider to use.</param>
        public OrderHistoryService()
        {
            this.orderHistoryRepository = new OrderHistoryProxyRepository(AppConfig.GetBaseApiUrl());
        }

        /// <inheritdoc/>
        public async Task<List<DummyProduct>> GetDummyProductsFromOrderHistoryAsync(int orderHistoryId)
        {
            if (orderHistoryId <= 0)
            {
                throw new ArgumentException("Order history ID must be positive", nameof(orderHistoryId));
            }

            return await orderHistoryRepository.GetDummyProductsFromOrderHistoryAsync(orderHistoryId);
        }
    }
}