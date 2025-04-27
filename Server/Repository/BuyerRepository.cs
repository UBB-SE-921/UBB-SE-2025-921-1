// <copyright file="BuyerRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Server.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Server.DataModels;
    using Server.DBConnection;
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.IRepository;

    /// <summary>
    /// Repository class for managing buyer-related database operations.
    /// </summary>
    /// <param name="dbContext">The database context instance.</param>
    public class BuyerRepository : IBuyerRepository
    {
        private readonly MarketPlaceDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuyerRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The database context instance.</param>
        public BuyerRepository(MarketPlaceDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        public async Task LoadBuyerInfo(Buyer buyerEntity)
        {
            int buyerId = buyerEntity.Id;
            Buyer? buyer = await this.dbContext.Buyers.FindAsync(buyerId);
            if (buyer == null)
            {
                throw new Exception("Loading buyer info failed: Buyer not found");
            }

            User? user = await this.dbContext.Users.FindAsync(buyerId);
            if (user == null)
            {
                throw new Exception("Loading buyer info failed: User not found");
            }

            buyerEntity.Badge = buyer.Badge;
            buyerEntity.Wishlist = await this.GetWishlist(buyerId);
            buyerEntity.Linkages = await this.GetBuyerLinkages(buyerId);
            buyerEntity.TotalSpending = buyer.TotalSpending;
            buyerEntity.NumberOfPurchases = buyer.NumberOfPurchases;
            buyerEntity.Discount = buyer.Discount;
            buyerEntity.UseSameAddress = buyer.UseSameAddress;
            buyerEntity.FollowingUsersIds = await this.GetFollowingUsersIds(buyerId);
            buyerEntity.User = user;
            buyerEntity.FirstName = buyer.FirstName;
            buyerEntity.LastName = buyer.LastName;
            buyerEntity.ShippingAddress = await this.LoadAddress(buyer.ShippingAddress.Id);
            if (buyer.UseSameAddress)
            {
                buyerEntity.BillingAddress = buyer.ShippingAddress;
            }
            else
            {
                buyerEntity.BillingAddress = await this.LoadAddress(buyer.BillingAddress.Id);
            }

            // SyncedBuyerIds is not used in the application, so it is not loaded
        }

        /// <inheritdoc/>
        public async Task SaveInfo(Buyer buyerEntity)
        {
            if (!await this.CheckIfBuyerExists(buyerEntity.Id))
            {
                throw new Exception("Saving buyer info failed: Buyer not found");
            }

            // Make sure the addresses are persisted
            await this.PersistAddress(buyerEntity.ShippingAddress);
            await this.PersistAddress(buyerEntity.BillingAddress);

            this.dbContext.Buyers.Update(buyerEntity);
            await this.dbContext.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task<BuyerWishlist> GetWishlist(int buyerId)
        {
            List<BuyerWishlistItemsEntity> buyerWishlistItems = await this.dbContext.BuyersWishlistItems.Where(item => item.BuyerId == buyerId).ToListAsync();
            BuyerWishlist buyerWishlist = new BuyerWishlist();
            foreach (BuyerWishlistItemsEntity item in buyerWishlistItems)
            {
                buyerWishlist.Items.Add(new BuyerWishlistItem(item.ProductId));
            }

            return buyerWishlist;
        }

        /// <inheritdoc/>
        public async Task<List<BuyerLinkage>> GetBuyerLinkages(int buyerId)
        {
            List<BuyerLinkageEntity> buyerLinkagesEntities = await this.dbContext.BuyerLinkages
                .Where(linkage => linkage.RequestingBuyerId == buyerId || linkage.ReceivingBuyerId == buyerId).ToListAsync();
            List<BuyerLinkage> buyerLinkages = new List<BuyerLinkage>();
            foreach (BuyerLinkageEntity linkageEntity in buyerLinkagesEntities)
            {
                BuyerLinkage buyerLinkage = ReadBuyerLinkage(linkageEntity, buyerId);
                buyerLinkages.Add(buyerLinkage);
            }

            return buyerLinkages;
        }

        /// <inheritdoc/>
        public async Task CreateLinkageRequest(int requestingBuyerId, int receivingBuyerId)
        {
            BuyerLinkageEntity linkageEntity = new BuyerLinkageEntity
            {
                RequestingBuyerId = requestingBuyerId,
                ReceivingBuyerId = receivingBuyerId,
                IsApproved = false,
            };
            this.dbContext.BuyerLinkages.Add(linkageEntity);
            await this.dbContext.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task UpdateLinkageRequest(int requestingBuyerId, int receivingBuyerId)
        {
            BuyerLinkageEntity? linkageEntity = await this.dbContext.BuyerLinkages
                .FirstOrDefaultAsync(linkage => linkage.RequestingBuyerId == requestingBuyerId && linkage.ReceivingBuyerId == receivingBuyerId);

            if (linkageEntity == null)
            {
                return;
            }

            linkageEntity.IsApproved = true;
            await this.dbContext.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteLinkageRequest(int requestingBuyerId, int receivingBuyerId)
        {
            BuyerLinkageEntity? linkageEntity = await this.dbContext.BuyerLinkages
                .FirstOrDefaultAsync(linkage => linkage.RequestingBuyerId == requestingBuyerId && linkage.ReceivingBuyerId == receivingBuyerId);

            if (linkageEntity == null)
            {
                return false;
            }

            this.dbContext.BuyerLinkages.Remove(linkageEntity);
            await this.dbContext.SaveChangesAsync();
            return true;
        }

        /// <inheritdoc/>
        public async Task<List<Buyer>> FindBuyersWithShippingAddress(Address shippingAddress)
        {
            return await this.dbContext.Buyers
                .Where(b => b.ShippingAddress.StreetLine.Equals(shippingAddress.StreetLine, StringComparison.CurrentCultureIgnoreCase) &&
                           b.ShippingAddress.City.Equals(shippingAddress.City, StringComparison.CurrentCultureIgnoreCase) &&
                           b.ShippingAddress.Country.Equals(shippingAddress.Country, StringComparison.CurrentCultureIgnoreCase) &&
                           b.ShippingAddress.PostalCode.Equals(shippingAddress.PostalCode, StringComparison.CurrentCultureIgnoreCase))
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task CreateBuyer(Buyer buyerEntity)
        {
            this.dbContext.Buyers.Add(buyerEntity);
            await this.dbContext.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task<List<int>> GetFollowingUsersIds(int buyerId)
        {
            return await this.dbContext.Followings
                .Where(f => f.BuyerId == buyerId)
                .Select(f => f.SellerId)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<List<Seller>> GetFollowedSellers(List<int>? followingUsersIds)
        {
            if (followingUsersIds == null || followingUsersIds.Count == 0)
            {
                return new List<Seller>();
            }

            return await this.dbContext.Sellers
                .Where(s => followingUsersIds.Contains(s.Id))
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<List<Seller>> GetAllSellers()
        {
            return await this.dbContext.Sellers.ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<List<Product>> GetProductsFromSeller(int sellerId)
        {
            return await this.dbContext.Products.Where(p => p.SellerId == sellerId).ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<bool> CheckIfBuyerExists(int buyerId)
        {
            return await this.dbContext.Buyers.AnyAsync(b => b.Id == buyerId);
        }

        /// <inheritdoc/>
        public async Task<bool> IsFollowing(int buyerId, int sellerId)
        {
            return await this.dbContext.Followings.AnyAsync(f => f.BuyerId == buyerId && f.SellerId == sellerId);
        }

        /// <inheritdoc/>
        public async Task FollowSeller(int buyerId, int sellerId)
        {
            this.dbContext.Followings.Add(new FollowingEntity { BuyerId = buyerId, SellerId = sellerId });
            await this.dbContext.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task UnfollowSeller(int buyerId, int sellerId)
        {
            this.dbContext.Followings.Remove(new FollowingEntity { BuyerId = buyerId, SellerId = sellerId });
            await this.dbContext.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task<int> GetTotalCount()
        {
            return await this.dbContext.Buyers.CountAsync();
        }

        /// <inheritdoc/>
        public async Task UpdateAfterPurchase(Buyer buyerEntity)
        {
            this.dbContext.Buyers.Update(buyerEntity);
            await this.dbContext.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task RemoveWishilistItem(int buyerId, int productId)
        {
            this.dbContext.BuyersWishlistItems.Remove(new BuyerWishlistItemsEntity { BuyerId = buyerId, ProductId = productId });
            await this.dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Loads address information from the database.
        /// </summary>
        /// <param name="addressId">The ID of the address to load.</param>
        /// <returns>A task containing the loaded address or null if not found.</returns>
        public async Task<Address?> LoadAddress(int addressId)
        {
            return await this.dbContext.Addresses.FindAsync(addressId);
        }

        /// <summary>
        /// Reads buyer linkage information from a SQL data reader.
        /// </summary>
        /// <param name="linkageEntity">The linkage entity containing linkage information.</param>
        /// <param name="buyerId">The ID of the current user.</param>
        /// <returns>A BuyerLinkage object containing the read information.</returns>
        private static BuyerLinkage ReadBuyerLinkage(BuyerLinkageEntity linkageEntity, int buyerId)
        {
            int requestingBuyerId = linkageEntity.RequestingBuyerId;
            int receivingBuyerId = linkageEntity.ReceivingBuyerId;
            bool isApproved = linkageEntity.IsApproved;
            int linkedBuyerId = requestingBuyerId;
            BuyerLinkageStatus buyerLinkageStatus = BuyerLinkageStatus.Confirmed;

            if (requestingBuyerId == buyerId)
            {
                linkedBuyerId = receivingBuyerId;
                if (!isApproved)
                {
                    buyerLinkageStatus = BuyerLinkageStatus.PendingSelf;
                }
            }
            else if (receivingBuyerId == buyerId)
            {
                linkedBuyerId = requestingBuyerId;
                if (!isApproved)
                {
                    buyerLinkageStatus = BuyerLinkageStatus.PendingOther;
                }
            }

            return new BuyerLinkage
            {
                Buyer = new Buyer
                {
                    User = new User { UserId = linkedBuyerId },
                },
                Status = buyerLinkageStatus,
            };
        }

        /// <summary>
        /// Persists an address to the database.
        /// </summary>
        /// <param name="address">The address to persist.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task PersistAddress(Address address)
        {
            if (this.dbContext.Addresses.Any(a => a.Id == address.Id))
            {
                // if the address is already in the database, update it
                this.dbContext.Addresses.Update(address);
            }
            else
            {
                // if the address is not in the database, add it
                this.dbContext.Addresses.Add(address);
            }

            await this.dbContext.SaveChangesAsync();
        }
    }
}
