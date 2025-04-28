namespace MarketPlace924.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.IRepository;

    /// <summary>
    /// Proxy repository class for managing contract renewal operations via REST API.
    /// </summary>
    public class ContractRenewalProxyRepository : IContractRenewalRepository
    {
        private const string ApiBaseRoute = "api/contract-renewals";
        private readonly HttpClient httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractRenewalProxyRepository"/> class.
        /// </summary>
        /// <param name="baseApiUrl">The base url of the API.</param>
        public ContractRenewalProxyRepository(string baseApiUrl)
        {
            this.httpClient = new HttpClient();
            this.httpClient.BaseAddress = new System.Uri(baseApiUrl);
        }

        /// <inheritdoc />
        public async Task AddRenewedContractAsync(IContract contract)
        {
            var response = await this.httpClient.PostAsJsonAsync($"{ApiBaseRoute}/add-renewed", (Contract)contract);
            response.EnsureSuccessStatusCode();
        }

        /// <inheritdoc />
        public async Task<List<IContract>> GetRenewedContractsAsync()
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/renewed");
            response.EnsureSuccessStatusCode();

            // Deserialize to List<Contract> (concrete type) as interfaces usually can't be deserialized directly
            var contracts = await response.Content.ReadFromJsonAsync<List<Contract>>();
            if (contracts == null)
            {
                contracts = new List<Contract>();
            }

            return contracts.Cast<IContract>().ToList();
        }

        /// <inheritdoc />
        public async Task<bool> HasContractBeenRenewedAsync(long contractId)
        {
            if (contractId <= 0)
            {
                throw new ArgumentException("Contract ID must be positive.", nameof(contractId));
            }

            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/{contractId}/has-been-renewed");
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<bool>();
            return result;
        }
    }
}