// <copyright file="BuyerWishlistItemViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketPlace924.ViewModel
{
    using System;

    /// <summary>
    /// View model class for managing buyer wishlist item data and operations.
    /// </summary>
    public class BuyerWishlistItemViewModel : IBuyerWishlistItemViewModel
    {
        /// <inheritdoc/>
        public int ProductId { get; set; }

        /// <inheritdoc/>
        public string Title { get; set; } = string.Empty;

        /// <inheritdoc/>
        public decimal Price { get; set; }

        /// <inheritdoc/>
        public string Description { get; set; } = string.Empty;

        /// <inheritdoc/>
        public string ImageSource { get; set; } = string.Empty;

        /// <inheritdoc/>
        public bool OwnItem { get; set; } = true;

        /// <inheritdoc/>
        public IOnBuyerWishlistItemRemoveCallback RemoveCallback { get; set; } = null!;

        /// <inheritdoc/>
        public async void Remove()
        {
            await this.RemoveCallback.OnBuyerWishlistItemRemove(this.ProductId);
        }
    }
}