﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Marketplace924.Domain;

namespace Marketplace924.ViewModel
{
    /// <summary>
    /// Defines the methods required for a billing information view model.
    /// </summary>
    public interface IBillingInfoViewModel
    {
        /// <summary>
        /// Asynchronously retrieves dummy products associated with the specified order history.
        /// </summary>
        /// <param name="orderHistoryID">The unique identifier for the order history.</param>
        /// <returns>A task representing the asynchronous operation that returns a list of <see cref="DummyProduct"/> objects.</returns>
        Task<List<DummyProduct>> GetDummyProductsFromOrderHistoryAsync(int orderHistoryID);

        /// <summary>
        /// Calculates the total order amount for the specified order history, including applicable fees.
        /// </summary>
        /// <param name="orderHistoryID">The unique identifier for the order history.</param>
        void CalculateOrderTotal(int orderHistoryID);

        /// <summary>
        /// Applies the borrowed tax calculation to the specified dummy product.
        /// </summary>
        /// <param name="dummyProduct">The dummy product on which to apply the borrowed tax.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ApplyBorrowedTax(DummyProduct dummyProduct);

        /// <summary>
        /// Updates the start date for the product's rental period.
        /// </summary>
        /// <param name="date">The new start date as a <see cref="DateTimeOffset"/>.</param>
        void UpdateStartDate(DateTimeOffset date);

        /// <summary>
        /// Updates the end date for the product's rental period.
        /// </summary>
        /// <param name="date">The new end date as a <see cref="DateTimeOffset"/>.</param>
        void UpdateEndDate(DateTimeOffset date);
    }
}
