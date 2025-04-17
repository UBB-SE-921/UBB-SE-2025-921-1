using System.Collections.Generic;
using System.Threading.Tasks;
using Marketplace924.Domain;

namespace Marketplace924.Repository
{
    public interface IContractRenewalRepository
    {
        Task AddRenewedContractAsync(IContract contract, byte[] pdfFile);
        Task<List<IContract>> GetRenewedContractsAsync();
        Task<bool> HasContractBeenRenewedAsync(long contractId);
    }
}