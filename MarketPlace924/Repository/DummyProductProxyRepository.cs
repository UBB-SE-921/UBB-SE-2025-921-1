using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using SharedClassLibrary.Domain;
using MarketPlace924.Repository;
using SharedClassLibrary.Shared;
using SharedClassLibrary.IRepository;

namespace MarketPlace924.Repository
{
    public class DummyProductProxyRepository : IDummyProductRepository
    {
        public Task AddDummyProductAsync(string name, float price, int sellerId, string productType, DateTime startDate, DateTime endDate)
        {
            throw new NotImplementedException();
        }

        public Task DeleteDummyProduct(int id)
        {
            throw new NotImplementedException();
        }

        public Task<DummyProduct> GetDummyProductByIdAsync(int productId)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetSellerNameAsync(int? sellerId)
        {
            throw new NotImplementedException();
        }

        public Task UpdateDummyProductAsync(int id, string name, float price, int sellerId, string productType, DateTime startDate, DateTime endDate)
        {
            throw new NotImplementedException();
        }
    }
}