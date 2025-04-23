using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using SharedClassLibrary.Domain;
using SharedClassLibrary.Shared;
using SharedClassLibrary.IRepository;

namespace MarketPlace924.Repository
{
    public class ContractProxyRepository : IContractRepository
    {
        public Task<IContract> AddContractAsync(IContract contract, byte[] pdfFile)
        {
            throw new NotImplementedException();
        }

        public Task<List<IContract>> GetAllContractsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<(int BuyerID, string BuyerName)> GetContractBuyerAsync(long contractId)
        {
            throw new NotImplementedException();
        }

        public Task<IContract> GetContractByIdAsync(long contractId)
        {
            throw new NotImplementedException();
        }

        public Task<List<IContract>> GetContractHistoryAsync(long contractId)
        {
            throw new NotImplementedException();
        }

        public Task<List<IContract>> GetContractsByBuyerAsync(int buyerId)
        {
            throw new NotImplementedException();
        }

        public Task<(int SellerID, string SellerName)> GetContractSellerAsync(long contractId)
        {
            throw new NotImplementedException();
        }

        public Task<DateTime?> GetDeliveryDateByContractIdAsync(long contractId)
        {
            throw new NotImplementedException();
        }

        public Task<(string PaymentMethod, DateTime OrderDate)> GetOrderDetailsAsync(long contractId)
        {
            throw new NotImplementedException();
        }

        public Task<Dictionary<string, object>> GetOrderSummaryInformationAsync(long contractId)
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> GetPdfByContractIdAsync(long contractId)
        {
            throw new NotImplementedException();
        }

        public Task<IPredefinedContract> GetPredefinedContractByPredefineContractTypeAsync(PredefinedContractType predefinedContractType)
        {
            throw new NotImplementedException();
        }

        public Task<(DateTime? StartDate, DateTime? EndDate, double price, string name)?> GetProductDetailsByContractIdAsync(long contractId)
        {
            throw new NotImplementedException();
        }
    }
}