using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedClassLibrary.Domain;
using SharedClassLibrary.ProxyRepository; // Added using for Repository namespace
using SharedClassLibrary.IRepository;

namespace SharedClassLibrary.Service
{
    public class ContractRenewalService : IContractRenewalService
    {
        private readonly IContractRenewalRepository contractRenewalRepository; // Added repository field

        // Added constructor for dependency injection
        public ContractRenewalService(IContractRenewalRepository contractRenewalRepository)
        {
            contractRenewalRepository = contractRenewalRepository ?? throw new ArgumentNullException(nameof(contractRenewalRepository));
        }

        // Implemented interface methods by calling repository methods
        public Task AddRenewedContractAsync(IContract contract)
        {
            // You might add business logic here before or after calling the repository
            return contractRenewalRepository.AddRenewedContractAsync(contract);
        }

        public Task<List<IContract>> GetRenewedContractsAsync()
        {
            // You might add business logic here
            return contractRenewalRepository.GetRenewedContractsAsync();
        }

        public Task<bool> HasContractBeenRenewedAsync(long contractId)
        {
            // You might add business logic here
            return contractRenewalRepository.HasContractBeenRenewedAsync(contractId);
        }
    }
}
