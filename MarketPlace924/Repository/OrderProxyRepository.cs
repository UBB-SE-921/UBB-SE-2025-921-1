using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using SharedClassLibrary.Domain;
using SharedClassLibrary.Shared;
using SharedClassLibrary.IRepository;

namespace MarketPlace924.Repository
{
    public class OrderProxyRepository : IOrderRepository
    {
        public Task AddOrderAsync(int productId, int buyerId, string productType, string paymentMethod, int orderSummaryId, DateTime orderDate)
        {
            throw new NotImplementedException();
        }

        public Task DeleteOrderAsync(int orderId)
        {
            throw new NotImplementedException();
        }

        public Task<List<Order>> GetBorrowedOrderHistoryAsync(int buyerId)
        {
            throw new NotImplementedException();
        }

        public Task<List<Order>> GetNewOrUsedOrderHistoryAsync(int buyerId)
        {
            throw new NotImplementedException();
        }

        public Task<List<Order>> GetOrdersByNameAsync(int buyerId, string text)
        {
            throw new NotImplementedException();
        }

        public Task<List<Order>> GetOrdersFrom2024Async(int buyerId)
        {
            throw new NotImplementedException();
        }

        public Task<List<Order>> GetOrdersFrom2025Async(int buyerId)
        {
            throw new NotImplementedException();
        }

        public Task<List<Order>> GetOrdersFromLastSixMonthsAsync(int buyerId)
        {
            throw new NotImplementedException();
        }

        public Task<List<Order>> GetOrdersFromLastThreeMonthsAsync(int buyerId)
        {
            throw new NotImplementedException();
        }

        public Task<List<Order>> GetOrdersFromOrderHistoryAsync(int orderHistoryId)
        {
            throw new NotImplementedException();
        }

        public Task<OrderSummary> GetOrderSummaryAsync(int orderSummaryId)
        {
            throw new NotImplementedException();
        }

        public Task<List<OrderDisplayInfo>> GetOrdersWithProductInfoAsync(int userId, string searchText = null, string timePeriod = null)
        {
            throw new NotImplementedException();
        }

        public Task<Dictionary<int, string>> GetProductCategoryTypesAsync(int userId)
        {
            throw new NotImplementedException();
        }

        public Task UpdateOrderAsync(int orderId, string productType, string paymentMethod, DateTime orderDate)
        {
            throw new NotImplementedException();
        }
    }
}