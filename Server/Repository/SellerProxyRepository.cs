// -----------------------------------------------------------------------
// <copyright file="SellerRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SharedClassLibrary.IRepository
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.IRepository;

    /// <summary>
    /// Repository for managing seller-related data operations.
    /// </summary>
    public class SellerProxyRepository : ISellerRepository
    {
        public SellerProxyRepository(IUserRepository userRepository)
        {
            throw new System.NotImplementedException();
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
