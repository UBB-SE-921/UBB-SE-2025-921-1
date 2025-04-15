// -----------------------------------------------------------------------
// <copyright file="Review.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SharedClassLibrary.Domain;

/// <summary>
/// Represents a review provided for a seller.
/// </summary>
public class Review
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Review"/> class.
    /// </summary>
    /// <param name="reviewId">The review identifier.</param>
    /// <param name="sellerId">The seller identifier.</param>
    /// <param name="score">The score given in the review.</param>
    public Review(int reviewId, int sellerId, double score)
    {
        this.ReviewId = reviewId;
        this.SellerId = sellerId;
        this.Score = score;
    }

    /// <summary>
    /// Gets or sets the unique identifier for the review.
    /// </summary>
    public int ReviewId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the seller being reviewed.
    /// </summary>
    public int SellerId { get; set; }

    /// <summary>
    /// Gets or sets the score given in the review.
    /// </summary>
    public double Score { get; set; }
}