using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using SharedClassLibrary.Domain;
using SharedClassLibrary.Shared;
using SharedClassLibrary.IRepository;

namespace MarketPlace924.Repository
{
    public class DummyCardProxyRepository : IDummyCardRepository
    {
        public Task DeleteCardAsync(string cardNumber)
        {
            throw new NotImplementedException();
        }

        public Task<float> GetCardBalanceAsync(string cardNumber)
        {
            throw new NotImplementedException();
        }

        public Task UpdateCardBalanceAsync(string cardNumber, float balance)
        {
            throw new NotImplementedException();
        }
    }
}
