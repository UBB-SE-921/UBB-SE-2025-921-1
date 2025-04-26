// -----------------------------------------------------------------------
// <copyright file="SellerProxyRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace MarketPlace924.Repository
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.IRepository;

    /// <summary>
    /// Repository for managing seller-related data operations.
    /// </summary>
    public class SellerProxyRepository : ISellerRepository
    {
        private readonly HttpClient httpClient;

        public SellerProxyRepository(string baseApiUrl)
        {
            this.httpClient = new HttpClient();
            this.httpClient.BaseAddress = new System.Uri(baseApiUrl);
        }

        public Task AddNewFollowerNotification(int sellerId, int currentFollowerCount, string message)
        {
            throw new System.NotImplementedException();
        }

        public Task AddSeller(Seller seller)
        {
            throw new System.NotImplementedException();
        }

        public Task<int> GetLastFollowerCount(int sellerId)
        {
            throw new System.NotImplementedException();
        }

        public Task<List<string>> GetNotifications(int sellerID, int maxNotifications)
        {
            throw new System.NotImplementedException();
        }

        public Task<List<Product>> GetProducts(int sellerID)
        {
            throw new System.NotImplementedException();
        }

        public Task<List<Review>> GetReviews(int sellerId)
        {
            throw new System.NotImplementedException();
        }

        public Task<Seller> GetSellerInfo(User user)
        {
            throw new System.NotImplementedException();
        }

        public Task UpdateSeller(Seller seller)
        {
            throw new System.NotImplementedException();
        }

        public Task UpdateTrustScore(int sellerId, double trustScore)
        {
            throw new System.NotImplementedException();
        }
    }
}
