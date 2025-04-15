// <copyright file="BuyerRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SharedClassLibrary.IRepository
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.IRepository;

    /// <summary>
    /// Repository class for managing buyer-related database operations.
    /// </summary>
    /// <param name="databaseConnection">The database connection instance.</param>
    public class BuyerProxyRepository : IBuyerRepository
    {
        public Task<bool> CheckIfBuyerExists(int buyerId)
        {
            throw new System.NotImplementedException();
        }

        public Task CreateBuyer(Buyer buyerEntity)
        {
            throw new System.NotImplementedException();
        }

        public Task CreateLinkageRequest(int requestingBuyerId, int receivingBuyerId)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> DeleteLinkageRequest(int requestingBuyerId, int receivingBuyerId)
        {
            throw new System.NotImplementedException();
        }

        public Task<List<Buyer>> FindBuyersWithShippingAddress(Address shippingAddress)
        {
            throw new System.NotImplementedException();
        }

        public Task FollowSeller(int buyerId, int sellerId)
        {
            throw new System.NotImplementedException();
        }

        public Task<List<Seller>> GetAllSellers()
        {
            throw new System.NotImplementedException();
        }

        public Task<List<BuyerLinkage>> GetBuyerLinkages(int buyerId)
        {
            throw new System.NotImplementedException();
        }

        public Task<List<Seller>> GetFollowedSellers(List<int>? followingUsersIds)
        {
            throw new System.NotImplementedException();
        }

        public Task<List<int>> GetFollowingUsersIds(int buyerId)
        {
            throw new System.NotImplementedException();
        }

        public Task<List<Product>> GetProductsFromSeller(int sellerId)
        {
            throw new System.NotImplementedException();
        }

        public Task<int> GetTotalCount()
        {
            throw new System.NotImplementedException();
        }

        public Task<BuyerWishlist> GetWishlist(int buyerId)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> IsFollowing(int buyerId, int sellerId)
        {
            throw new System.NotImplementedException();
        }

        public Task LoadBuyerInfo(Buyer buyerEntity)
        {
            throw new System.NotImplementedException();
        }

        public Task RemoveWishilistItem(int buyerId, int productId)
        {
            throw new System.NotImplementedException();
        }

        public Task SaveInfo(Buyer buyerEntity)
        {
            throw new System.NotImplementedException();
        }

        public Task UnfollowSeller(int buyerId, int sellerId)
        {
            throw new System.NotImplementedException();
        }

        public Task UpdateAfterPurchase(Buyer buyerEntity)
        {
            throw new System.NotImplementedException();
        }

        public Task UpdateLinkageRequest(int requestingBuyerId, int receivingBuyerId)
        {
            throw new System.NotImplementedException();
        }
    }
}
