using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using SharedClassLibrary.Domain;
using SharedClassLibrary.Shared;
using SharedClassLibrary.IRepository;

namespace MarketPlace924.Repository
{
    public class ContractRenewalProxyRepository : IContractRenewalRepository
    {
        public Task AddRenewedContractAsync(IContract contract, byte[] pdfFile)
        {
            throw new NotImplementedException();
        }

        public Task<List<IContract>> GetRenewedContractsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<bool> HasContractBeenRenewedAsync(long contractId)
        {
            throw new NotImplementedException();
        }
    }
}