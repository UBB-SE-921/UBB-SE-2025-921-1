using System;
using System.Data;
using System.Threading.Tasks;
using SharedClassLibrary.Domain;
using SharedClassLibrary.Shared;
using SharedClassLibrary.IRepository;

namespace Marketplace924.Repository
{
    /// <summary>
    /// Provides database operations for order summary management.
    /// </summary>
    public class OrderSummaryProxyRepository : IOrderSummaryRepository
    {
        public Task<OrderSummary> GetOrderSummaryByIdAsync(int orderSummaryId)
        {
            throw new NotImplementedException();
        }

        public Task UpdateOrderSummaryAsync(int id, float subtotal, float warrantyTax, float deliveryFee, float finalTotal, string fullName, string email, string phoneNumber, string address, string postalCode, string additionalInfo, string contractDetails)
        {
            throw new NotImplementedException();
        }
    }
} 